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

            session = gameSession ?? throw new ArgumentNullException(nameof(gameSession));

            wheelSpinner.OnSpinStarted += session.StartSpin;
            wheelSpinner.OnSpinCompleted += session.CompleteSpin;
            session.OnZoneChanged += HandleZoneChanged;
        }

        public void Disconnect()
        {
            if (session == null) return;

            wheelSpinner.OnSpinStarted -= session.StartSpin;
            wheelSpinner.OnSpinCompleted -= session.CompleteSpin;
            session.OnZoneChanged -= HandleZoneChanged;
            session = null;
        }

        private void HandleZoneChanged(int _, ZoneType type)
        {
            WheelData data = wheelResolver.WheelFor(type);
            wheelPresenter.SetWheel(data);
            wheelSpinner.SetWheel(data);
        }
    }
}
