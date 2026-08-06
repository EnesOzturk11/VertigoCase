using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Data;

namespace VertigoCase.Core
{
    /// <summary>
    /// Unity adapter for a configured run. Dependencies are supplied by <see cref="GameInstaller"/>;
    /// this class connects session events to scene ports and exposes narrow view-facing contracts.
    /// </summary>
    public class GameController :
        MonoBehaviour,
        IGameOverPort,
        IInventoryPort,
        IZonePort,
        ILeavePort
    {
        private IGameSession session;
        private IWheelCoordinator wheelCoordinator;

        // UI observes this facade and remains independent from the domain services.
        public event Action<int, ZoneType> OnZoneChanged;
        public event Action<GameState> OnStateChanged;
        public event Action<IReadOnlyDictionary<RewardType, int>> OnCashedOut;
        public event Action<IReadOnlyDictionary<RewardType, int>> OnInventoryChanged;

        public GameState State => session.State;
        public IReadOnlyDictionary<RewardType, int> Inventory => session.Inventory;
        public bool CanLeave => session.CanLeave;
        public ZoneType TypeOf(int zone) => session.TypeOf(zone);

        public void Configure(IGameSession gameSession, IWheelCoordinator coordinator)
        {
            if (session != null)
                throw new InvalidOperationException("GameController is already configured.");

            session = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
            wheelCoordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        }

        private void OnEnable()
        {
            EnsureConfigured();

            wheelCoordinator.Connect(session);

            session.OnInventoryChanged += HandleInventoryChanged;
            session.OnZoneChanged += HandleZoneChanged;
            session.OnStateChanged += HandleStateChanged;
            session.OnCashedOut += HandleCashedOut;
        }

        private void OnDisable()
        {
            if (session == null) return;

            wheelCoordinator.Disconnect();

            session.OnInventoryChanged -= HandleInventoryChanged;
            session.OnZoneChanged -= HandleZoneChanged;
            session.OnStateChanged -= HandleStateChanged;
            session.OnCashedOut -= HandleCashedOut;
        }

        private void Start() => session.Initialize();

        private void HandleZoneChanged(int zone, ZoneType type) => OnZoneChanged?.Invoke(zone, type);

        private void HandleInventoryChanged(IReadOnlyDictionary<RewardType, int> inventory) =>
            OnInventoryChanged?.Invoke(inventory);
        private void HandleStateChanged(GameState state) => OnStateChanged?.Invoke(state);
        private void HandleCashedOut(IReadOnlyDictionary<RewardType, int> rewards) =>
            OnCashedOut?.Invoke(rewards);

        public void Restart() => session.Restart();
        public void Leave() => session.Leave();
        public void GiveUp() => session.GiveUp();

        private void EnsureConfigured()
        {
            if (session == null)
                throw new InvalidOperationException(
                    "GameController must be configured by GameInstaller before it is enabled.");
        }
    }
}
