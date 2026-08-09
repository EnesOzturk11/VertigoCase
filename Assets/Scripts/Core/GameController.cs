using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Data;
using VertigoCase.Utils;

namespace VertigoCase.Core
{
    /// <summary>
    /// Unity adapter for a configured run. Dependencies are supplied by <see cref="GameInstaller"/>;
    /// this class connects session events to scene ports and exposes narrow view-facing contracts.
    /// </summary>
    public class GameController :
        MonoBehaviour,
        ISpinPort,
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
        public int CurrentZone => session.CurrentZone;
        public ZoneType CurrentZoneType => session.CurrentZoneType;
        public bool CanLeave => session.CanLeave;
        public ZoneType TypeOf(int zone) => session.TypeOf(zone);

        public void Configure(IGameSession gameSession, IWheelCoordinator coordinator)
        {
            if (session != null)
                throw new InvalidOperationException("GameController is already configured.");

            if (gameSession == null) throw new ArgumentNullException(nameof(gameSession));
            if (coordinator == null) throw new ArgumentNullException(nameof(coordinator));

            // Validate the complete configuration before committing either dependency.
            session = gameSession;
            wheelCoordinator = coordinator;
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

        private void HandleZoneChanged(int zone, ZoneType type) =>
            ObserverDispatcher.Notify(OnZoneChanged, zone, type, this);

        private void HandleInventoryChanged(IReadOnlyDictionary<RewardType, int> inventory) =>
            ObserverDispatcher.Notify(OnInventoryChanged, inventory, this);
        private void HandleStateChanged(GameState state) =>
            ObserverDispatcher.Notify(OnStateChanged, state, this);
        private void HandleCashedOut(IReadOnlyDictionary<RewardType, int> rewards) =>
            ObserverDispatcher.Notify(OnCashedOut, rewards, this);

        public void Spin() => wheelCoordinator.Spin();
        public void Leave() => session.Leave();
        public void GiveUp() => session.GiveUp();

        private void EnsureConfigured()
        {
            if (session == null || wheelCoordinator == null)
                throw new InvalidOperationException(
                    "GameController must be configured by GameInstaller before it is enabled.");
        }
    }
}
