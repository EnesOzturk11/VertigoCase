using System;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Shows the bomb panel on GameOver and forwards the restart decision. Revive is intentionally
    /// unavailable until a real currency-spend or rewarded-ad gateway can authorize it.
    /// This component must sit on an ALWAYS-ACTIVE object; it toggles the child <see cref="panelRoot"/>.
    /// </summary>
    public class GameOverPanelView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonoBehaviour game;
        [SerializeField] private GameObject panelRoot;      // the panel shown/hidden (its background blocks input)
        [SerializeField] private Button giveUpButton;

        private IGameOverPort gamePort;

        private void Awake()
        {
            gamePort = game as IGameOverPort ??
                       throw new InvalidOperationException(
                           "GameOverPanelView requires a component implementing IGameOverPort.");
        }

        private void OnEnable()
        {
            if (panelRoot == null) return;

            gamePort.OnStateChanged += HandleStateChanged;
            if (giveUpButton != null)
                giveUpButton.onClick.AddListener(HandleGiveUp);
            Refresh(gamePort.State);
        }

        private void OnDisable()
        {
            if (gamePort == null || panelRoot == null) return;

            gamePort.OnStateChanged -= HandleStateChanged;
            if (giveUpButton != null)
                giveUpButton.onClick.RemoveListener(HandleGiveUp);
        }

        private void HandleStateChanged(GameState state) => Refresh(state);

        // The panel is visible only during GameOver.
        private void Refresh(GameState state) => panelRoot.SetActive(state == GameState.GameOver);

        private void HandleGiveUp() => gamePort.GiveUp();
    }
}
