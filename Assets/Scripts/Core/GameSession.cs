using System;
using System.Collections.Generic;
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

        public event Action<IReadOnlyDictionary<RewardType, int>> OnInventoryChanged;
        public event Action<int, ZoneType> OnZoneChanged;
        public event Action<GameState> OnStateChanged;
        public event Action<IReadOnlyDictionary<RewardType, int>> OnCashedOut;

        public GameState State => stateMachine.Current;
        public IReadOnlyDictionary<RewardType, int> Inventory => rewards.Amounts;
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

            this.rewards.OnChanged += PublishRewards;
            this.stateMachine.OnStateChanged += HandleStateChanged;
        }

        public void Initialize()
        {
            PublishZone();
            PublishRewards();
        }

        public void StartSpin()
        {
            if (State != GameState.Idle) return;
            stateMachine.ChangeState(GameState.Spinning);
        }

        public void CompleteSpin(WheelSlice slice)
        {
            if (State != GameState.Spinning) return;
            if (slice == null) throw new ArgumentNullException(nameof(slice));
            if (!slice.IsBomb && slice.reward == null)
                throw new InvalidOperationException("A non-bomb wheel slice must reference reward data.");

            stateMachine.ChangeState(GameState.Resolving);

            if (slice.IsBomb)
            {
                // A bomb ends the run immediately. The loss is a domain/economy rule, not a UI
                // choice deferred until the player presses Give Up.
                rewards.Clear();
                zones.Reset();
                PublishZone();
                stateMachine.ChangeState(GameState.GameOver);
                return;
            }

            // The value printed on a wheel slice is the final reward. Wheel callouts such as
            // "Up To x3 Rewards" describe the values already configured on that wheel; applying
            // the zone number or the callout again would award a different amount than the UI shows.
            int amount = checked(slice.reward.baseAmount * slice.multiplier);
            rewards.Add(slice.reward.type, amount);

            zones.Advance();
            PublishZone();
            stateMachine.ChangeState(GameState.Idle);
        }

        public void Restart()
        {
            zones.Reset();
            rewards.Clear();
            PublishZone();
            stateMachine.Reset();
        }

        public void Leave()
        {
            if (!CanLeave) return;

            OnCashedOut?.Invoke(new Dictionary<RewardType, int>(rewards.Amounts));
            Restart();
        }

        public void GiveUp()
        {
            if (State != GameState.GameOver) return;
            Restart();
        }

        private void PublishRewards()
        {
            OnInventoryChanged?.Invoke(rewards.Amounts);
        }

        private void PublishZone() => OnZoneChanged?.Invoke(zones.CurrentZone, zones.CurrentType);
        private void HandleStateChanged(GameState state) => OnStateChanged?.Invoke(state);
    }
}
