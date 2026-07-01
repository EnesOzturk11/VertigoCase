using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Wires the Leave (cash-out) button to the GameController from code, and keeps it enabled only
    /// while leaving is allowed (Idle + safe/super zone). A thin View: it forwards the click and
    /// reflects state; it holds no cash-out logic itself (that lives in GameController.Leave).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LeaveButtonBinder : MonoBehaviour
    {
        [SerializeField] private Button leaveButton;
        [SerializeField] private GameController game;

        private void OnValidate()
        {
            if (leaveButton == null) leaveButton = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (leaveButton == null || game == null) return;

            leaveButton.onClick.AddListener(game.Leave);
            game.OnStateChanged += HandleStateChanged;   // state changed -> refresh the button
            game.OnZoneChanged  += HandleZoneChanged;    // zone changed (type may change) -> refresh
            Refresh();
        }

        private void OnDisable()
        {
            if (leaveButton == null || game == null) return;

            leaveButton.onClick.RemoveListener(game.Leave);
            game.OnStateChanged -= HandleStateChanged;   // named methods so -= actually unsubscribes
            game.OnZoneChanged  -= HandleZoneChanged;
        }

        private void HandleStateChanged(GameState state)        => Refresh();
        private void HandleZoneChanged(int zone, ZoneType type) => Refresh();
        private void Refresh() => leaveButton.interactable = game.CanLeave;
    }
}