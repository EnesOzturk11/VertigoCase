using System;
using VertigoCase.Data;
using VertigoCase.UI;
using VertigoCase.Wheel;

namespace VertigoCase.Core
{
    /// <summary>
    /// Owns all wheel-related orchestration: spin events enter the session, while zone changes update
    /// both the wheel presenter and spinner data. GameController remains a session-to-view facade.
    /// </summary>
    public sealed class WheelCoordinator : IWheelCoordinator
    {
        private readonly IZoneWheelResolver wheelResolver;
        private readonly IWheelSpinner wheelSpinner;
        private readonly IWheelPresenter wheelPresenter;

        private IGameSession session;

        public WheelCoordinator(
            IZoneWheelResolver wheelResolver,
            IWheelSpinner wheelSpinner,
            IWheelPresenter wheelPresenter)
        {
            this.wheelResolver = wheelResolver ?? throw new ArgumentNullException(nameof(wheelResolver));
            this.wheelSpinner = wheelSpinner ?? throw new ArgumentNullException(nameof(wheelSpinner));
            this.wheelPresenter = wheelPresenter ?? throw new ArgumentNullException(nameof(wheelPresenter));
        }

        public void Connect(IGameSession gameSession)
        {
            if (session != null)
                throw new InvalidOperationException("WheelCoordinator is already connected.");

            if (gameSession == null) throw new ArgumentNullException(nameof(gameSession));

            // Complete all failure-prone setup before publishing the connected state.
            ApplyWheel(gameSession.CurrentZoneType);
            session = gameSession;

            wheelSpinner.OnSpinCompleted += session.CompleteSpin;
            wheelSpinner.OnSpinCancelled += session.CancelSpin;
            session.OnZoneChanged += HandleZoneChanged;
        }

        public void Disconnect()
        {
            if (session == null) return;

            IGameSession disconnectedSession = session;
            try
            {
                wheelSpinner.CancelSpin();
            }
            finally
            {
                wheelSpinner.OnSpinCompleted -= disconnectedSession.CompleteSpin;
                wheelSpinner.OnSpinCancelled -= disconnectedSession.CancelSpin;
                disconnectedSession.OnZoneChanged -= HandleZoneChanged;
                session = null;
            }
        }

        public void Spin()
        {
            if (session == null)
                return;
            if (session.State != GameState.Idle)
                return;

            IGameSession activeSession = session;
            activeSession.StartSpin();
            if (session != activeSession || activeSession.State != GameState.Spinning)
                return;

            try
            {
                if (!wheelSpinner.TrySpin())
                    activeSession.CancelSpin();
            }
            catch
            {
                activeSession.CancelSpin();
                throw;
            }
        }

        private void HandleZoneChanged(int _, ZoneType type)
        {
            ApplyWheel(type);
        }

        private void ApplyWheel(ZoneType type)
        {
            WheelData data = wheelResolver.WheelFor(type);
            wheelPresenter.SetWheel(data);
            wheelSpinner.SetWheel(data);
        }
    }
}
