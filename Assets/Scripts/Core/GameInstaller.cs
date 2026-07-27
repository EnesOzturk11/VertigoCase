using System;
using UnityEngine;
using VertigoCase.Config;
using VertigoCase.Economy;
using VertigoCase.UI;
using VertigoCase.Wheel;

namespace VertigoCase.Core
{
    /// <summary>
    /// Scene composition root. This is the only class that selects concrete application services;
    /// all runtime consumers receive the resulting abstractions through explicit injection.
    /// </summary>
    [DefaultExecutionOrder(GameConstants.ExecutionOrder.GameInstaller)]
    public sealed class GameInstaller : MonoBehaviour
    {
        [SerializeField] private GameController gameController;
        [SerializeField] private SpinController spinController;
        [SerializeField] private WheelView wheelView;
        [SerializeField] private ZoneCatalog zoneCatalog;

        private void Awake()
        {
            if (gameController == null)
                throw new InvalidOperationException("GameInstaller requires a GameController.");
            if (spinController == null)
                throw new InvalidOperationException("GameInstaller requires a SpinController.");
            if (wheelView == null)
                throw new InvalidOperationException("GameInstaller requires a WheelView.");
            if (zoneCatalog == null)
                throw new InvalidOperationException("GameInstaller requires a ZoneCatalog.");

            zoneCatalog.ValidateConfiguration();

            IGameSession session = new GameSession(
                new ZoneService(zoneCatalog),
                new RewardService(),
                new GameStateMachine(),
                zoneCatalog);

            IWheelCoordinator wheelCoordinator =
                new WheelCoordinator(zoneCatalog, spinController, wheelView);

            gameController.Configure(session, wheelCoordinator);
        }
    }
}
