using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Wires the Spin button through the application-facing spin port. The view never starts a
    /// tween directly, so the session authorizes the state transition before animation begins.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SpinButtonBinder : MonoBehaviour
    {
        [SerializeField] private Button spinButton;
        [FormerlySerializedAs("spinController")]
        [SerializeField] private MonoBehaviour game;

        private ISpinPort gamePort;

        // Auto-assign references in the editor so we don't hand-wire what code can find (case rule).
        private void OnValidate()
        {
            if (spinButton == null) spinButton = GetComponent<Button>();
        }

        private void Awake()
        {
            if (spinButton == null)
                spinButton = GetComponent<Button>();

            gamePort = game as ISpinPort ??
                       throw new InvalidOperationException(
                           "SpinButtonBinder requires a component implementing ISpinPort.");
        }

        private void OnEnable()
        {
            spinButton.onClick.AddListener(gamePort.Spin);
            gamePort.OnStateChanged += HandleStateChanged;
            Refresh();
        }

        private void OnDisable() // mirror OnEnable so we never double-subscribe or leak listeners
        {
            if (spinButton == null || gamePort == null) return;

            spinButton.onClick.RemoveListener(gamePort.Spin);
            gamePort.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState _) => Refresh();

        private void Refresh() => spinButton.interactable = gamePort.State == GameState.Idle;
    }
}
