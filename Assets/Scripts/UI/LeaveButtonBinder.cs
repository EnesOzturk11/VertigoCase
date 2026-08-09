using System;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Wires the Leave (cash-out) button through <see cref="ILeavePort"/> and keeps it enabled only
    /// while leaving is allowed. A thin View: it forwards the click and reflects state.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LeaveButtonBinder : MonoBehaviour
    {
        [SerializeField] private Button leaveButton;
        [SerializeField] private MonoBehaviour game;

        private ILeavePort gamePort;

        private void OnValidate()
        {
            if (leaveButton == null) leaveButton = GetComponent<Button>();
        }

        private void Awake()
        {
            if (leaveButton == null)
                leaveButton = GetComponent<Button>();

            gamePort = game as ILeavePort ??
                       throw new InvalidOperationException(
                           "LeaveButtonBinder requires a component implementing ILeavePort.");
        }

        private void OnEnable()
        {
            leaveButton.onClick.AddListener(gamePort.Leave);
            gamePort.OnStateChanged += HandleStateChanged;
            gamePort.OnZoneChanged += HandleZoneChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (leaveButton == null || gamePort == null) return;

            leaveButton.onClick.RemoveListener(gamePort.Leave);
            gamePort.OnStateChanged -= HandleStateChanged;
            gamePort.OnZoneChanged -= HandleZoneChanged;
        }

        private void HandleStateChanged(GameState state)        => Refresh();
        private void HandleZoneChanged(int zone, ZoneType type) => Refresh();
        private void Refresh() => leaveButton.interactable = gamePort.CanLeave;
    }
}
