using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Shows the "a bomb exploded" panel on GameOver and wires its buttons from code. Give Up starts a
    /// fresh run; the two Revive buttons keep the collected rewards and continue — one by spending gold,
    /// one by watching a rewarded video. A thin View: it only toggles the panel and forwards actions;
    /// the run decisions live in GameController.
    ///
    /// Developer Mode (Inspector toggle): instead of really spending gold or playing an ad, it logs
    /// "Gold spent" / "Video watched" to the Console and revives immediately — handy for testing before
    /// the currency/ad systems exist.
    ///
    /// This component must sit on an ALWAYS-ACTIVE object; it toggles the child <see cref="panelRoot"/>.
    /// </summary>
    public class GameOverPanelView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameController game;
        [SerializeField] private GameObject panelRoot;      // the panel shown/hidden (its background blocks input)
        [SerializeField] private Button giveUpButton;
        [SerializeField] private Button reviveGoldButton;   // revive by spending gold
        [SerializeField] private Button reviveVideoButton;  // revive by watching a rewarded video

        [Header("Developer")]
        [SerializeField] private bool developerMode = true; // true: skip real gold/ad, just log + revive
        [SerializeField] private int reviveGoldCost = 25;   // gold price of a revive (used in the real path)

        private void OnEnable()
        {
            if (game == null || panelRoot == null) return;

            game.OnStateChanged += HandleStateChanged;
            // Bind each button if it exists, so a not-yet-assigned reference can't break show/hide.
            if (giveUpButton != null)      giveUpButton.onClick.AddListener(HandleGiveUp);
            if (reviveGoldButton != null)  reviveGoldButton.onClick.AddListener(HandleReviveWithGold);
            if (reviveVideoButton != null) reviveVideoButton.onClick.AddListener(HandleReviveWithVideo);
            Refresh(game.State);                                     // start hidden (Idle at launch)
        }

        private void OnDisable()
        {
            if (game == null || panelRoot == null) return;

            game.OnStateChanged -= HandleStateChanged;
            if (giveUpButton != null)      giveUpButton.onClick.RemoveListener(HandleGiveUp);
            if (reviveGoldButton != null)  reviveGoldButton.onClick.RemoveListener(HandleReviveWithGold);
            if (reviveVideoButton != null) reviveVideoButton.onClick.RemoveListener(HandleReviveWithVideo);
        }

        private void HandleStateChanged(GameState state) => Refresh(state);

        // The panel is visible only during GameOver.
        private void Refresh(GameState state) => panelRoot.SetActive(state == GameState.GameOver);

        private void HandleGiveUp() => game.GiveUp();

        private void HandleReviveWithGold()
        {
            if (developerMode)
            {
                Debug.Log($"Gold spent: {reviveGoldCost}");   // dev shortcut: pretend we paid the gold cost
                game.Revive();
                return;
            }

            // Real path: deduct reviveGoldCost through the economy, then revive. (Currency system TBD.)
            // TODO: integrate CurrencyService — verify balance, spend reviveGoldCost, then Revive() on success.
            game.Revive();
        }

        private void HandleReviveWithVideo()
        {
            if (developerMode)
            {
                Debug.Log("Video watched");   // dev shortcut: pretend the rewarded ad finished
                game.Revive();
                return;
            }

            // Real path: play a rewarded ad and revive from its completion callback. (Ad SDK TBD.)
            // TODO: integrate a rewarded-ad SDK — call Revive() only after the ad reports success.
            game.Revive();
        }
    }
}