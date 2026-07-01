using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Shows the "a bomb exploded" panel when the game enters GameOver and wires its two buttons from
    /// code. Give Up starts a fresh run; Revive keeps the collected rewards and continues. A thin View:
    /// it only toggles the panel and forwards clicks; the decisions live in GameController.
    /// This component must sit on an ALWAYS-ACTIVE object; it toggles the child <see cref="panelRoot"/>.
    /// </summary>
    public class GameOverPanelView : MonoBehaviour
    {
        [SerializeField] private GameController game;
        [SerializeField] private GameObject panelRoot;    // the panel shown/hidden (its background blocks input)
        [SerializeField] private Button giveUpButton;
        [SerializeField] private Button reviveButton;

        private void OnEnable()
        {
            if (game == null || panelRoot == null) return;

            game.OnStateChanged += HandleStateChanged;
            giveUpButton.onClick.AddListener(HandleGiveUp);   // bind in code, not in the Inspector
            reviveButton.onClick.AddListener(HandleRevive);
            Refresh(game.State);                              // start hidden (Idle at launch)
        }

        private void OnDisable()
        {
            if (game == null || panelRoot == null) return;

            game.OnStateChanged -= HandleStateChanged;
            giveUpButton.onClick.RemoveListener(HandleGiveUp);
            reviveButton.onClick.RemoveListener(HandleRevive);
        }

        private void HandleStateChanged(GameState state) => Refresh(state);

        // The panel is visible only during GameOver.
        private void Refresh(GameState state) => panelRoot.SetActive(state == GameState.GameOver);

        private void HandleGiveUp() => game.GiveUp();
        private void HandleRevive() => game.Revive();
    }
}
