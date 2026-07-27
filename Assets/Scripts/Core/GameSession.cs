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

        public event Action<int> OnBalanceChanged;
        public event Action<IReadOnlyDictionary<RewardType, int>> OnInventoryChanged;
        public event Action<int, ZoneType> OnZoneChanged;
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnCashedOut;

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
                stateMachine.ChangeState(GameState.GameOver);
                return;
            }

            IZoneStrategy strategy = strategyResolver.Resolve(zones.CurrentType);
            int baseAmount = slice.reward.baseAmount * slice.multiplier;
            int amount = strategy.ScaleReward(baseAmount, zones.CurrentZone);
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

            OnCashedOut?.Invoke(rewards.Total);
            Restart();
        }

        public void Revive()
        {
            if (State != GameState.GameOver) return;
            stateMachine.ChangeState(GameState.Idle);
        }

        public void GiveUp()
        {
            if (State != GameState.GameOver) return;
            Restart();
        }

        private void PublishRewards()
        {
            OnBalanceChanged?.Invoke(rewards.Total);
            OnInventoryChanged?.Invoke(rewards.Amounts);
        }

        private void PublishZone() => OnZoneChanged?.Invoke(zones.CurrentZone, zones.CurrentType);
        private void HandleStateChanged(GameState state) => OnStateChanged?.Invoke(state);
    }
}
