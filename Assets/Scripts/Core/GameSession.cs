using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VertigoCase.Data;
using VertigoCase.Economy;

namespace VertigoCase.Core
{
    /// <summary>
    /// Owns the rules and lifecycle of one run. It has no scene or UI responsibilities, so the
    /// gameplay flow can evolve and be tested independently from Unity components.
    /// </summary>
    public sealed class GameSession : IGameSession
    {
        private readonly IZoneProgression zones;
        private readonly IRewardWallet rewards;
        private readonly IGameStateMachine stateMachine;
        private readonly IZoneStrategyResolver strategyResolver;
        private bool isNotifying;

        public event Action<IReadOnlyDictionary<RewardType, int>> OnInventoryChanged;
        public event Action<int, ZoneType> OnZoneChanged;
        public event Action<GameState> OnStateChanged;
        public event Action<IReadOnlyDictionary<RewardType, int>> OnCashedOut;

        public GameState State => stateMachine.Current;
        public IReadOnlyDictionary<RewardType, int> Inventory => rewards.Amounts;
        public int CurrentZone => zones.CurrentZone;
        public ZoneType CurrentZoneType => zones.CurrentType;
        public bool CanLeave =>
            State == GameState.Idle && strategyResolver.Resolve(zones.CurrentType).CanLeave;
        public ZoneType TypeOf(int zone) => zones.TypeOf(zone);

        public GameSession(
            IZoneProgression zones,
            IRewardWallet rewards,
            IGameStateMachine stateMachine,
            IZoneStrategyResolver strategyResolver)
        {
            this.zones = zones ?? throw new ArgumentNullException(nameof(zones));
            this.rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
            this.stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            this.strategyResolver = strategyResolver ?? throw new ArgumentNullException(nameof(strategyResolver));
        }

        public void Initialize()
        {
            if (isNotifying) return;
            PublishRunState();
        }

        public void StartSpin()
        {
            if (isNotifying || State != GameState.Idle) return;
            stateMachine.ChangeState(GameState.Spinning);
            Notify(PublishState);
        }

        public void CancelSpin()
        {
            if (isNotifying || State != GameState.Spinning) return;
            stateMachine.ChangeState(GameState.Idle);
            Notify(PublishState);
        }

        public void CompleteSpin(WheelSlice slice)
        {
            if (isNotifying || State != GameState.Spinning) return;
            if (slice == null) throw new ArgumentNullException(nameof(slice));
            int rewardAmount = ValidateAndCalculateReward(slice);

            // All failure-prone calculations are completed before mutation. Observers are notified
            // only after the state, wallet and zone have committed as one synchronous operation.
            stateMachine.ChangeState(GameState.Resolving);
            if (slice.IsBomb)
            {
                rewards.Clear();
                zones.Reset();
                stateMachine.ChangeState(GameState.GameOver);
            }
            else
            {
                rewards.Add(slice.Reward.Type, rewardAmount);
                zones.Advance();
                stateMachine.ChangeState(GameState.Idle);
            }

            PublishRunState();
        }

        private void ResetRun()
        {
            zones.Reset();
            rewards.Clear();
            stateMachine.Reset();

            PublishRunState();
        }

        public void Leave()
        {
            if (isNotifying || !CanLeave) return;

            IReadOnlyDictionary<RewardType, int> cashedOutRewards =
                CreateInventorySnapshot();

            // The run reset is committed before observers receive the immutable cash-out snapshot.
            // An observer failure therefore cannot leave half-reset session state behind.
            ResetRun();
            Notify(() => OnCashedOut?.Invoke(cashedOutRewards));
        }

        public void GiveUp()
        {
            if (isNotifying || State != GameState.GameOver) return;
            ResetRun();
        }

        private int ValidateAndCalculateReward(WheelSlice slice)
        {
            if (slice.IsBomb)
            {
                if (slice.Reward != null)
                    throw new InvalidOperationException(
                        "A bomb wheel slice cannot reference collectible reward data.");

                return 0;
            }

            if (slice.Reward == null)
                throw new InvalidOperationException(
                    "A non-bomb wheel slice must reference reward data.");
            if (!Enum.IsDefined(typeof(RewardType), slice.Reward.Type))
                throw new InvalidOperationException("The wheel slice has an unknown reward type.");
            if (slice.Reward.BaseAmount <= 0 || slice.Multiplier <= 0)
                throw new InvalidOperationException(
                    "Reward base amount and multiplier must be positive.");
            if (zones.CurrentZone == int.MaxValue)
                throw new InvalidOperationException("The final supported zone has been reached.");

            int amount = checked(slice.Reward.BaseAmount * slice.Multiplier);
            checked
            {
                _ = rewards.AmountOf(slice.Reward.Type) + amount;
            }

            return amount;
        }

        private void PublishRewards()
        {
            OnInventoryChanged?.Invoke(CreateInventorySnapshot());
        }

        private IReadOnlyDictionary<RewardType, int> CreateInventorySnapshot() =>
            new ReadOnlyDictionary<RewardType, int>(
                new Dictionary<RewardType, int>(rewards.Amounts));

        private void PublishZone() => OnZoneChanged?.Invoke(zones.CurrentZone, zones.CurrentType);
        private void PublishState() => OnStateChanged?.Invoke(State);

        private void PublishRunState()
        {
            Notify(() =>
            {
                PublishRewards();
                PublishZone();
                PublishState();
            });
        }

        private void Notify(Action notification)
        {
            bool wasNotifying = isNotifying;
            isNotifying = true;
            try
            {
                notification();
            }
            finally
            {
                isNotifying = wasNotifying;
            }
        }
    }
}
