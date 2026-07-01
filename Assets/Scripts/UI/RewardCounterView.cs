using UnityEngine;
using TMPro;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Shows the bank balance. A pure View: it subscribes to GameController's balance event and writes
    /// the number to a label. No game logic, no polling — it only reacts when the balance changes.
    /// </summary>
    public class RewardCounterView : MonoBehaviour
    {
        [SerializeField] private GameController game;
        [SerializeField] private TextMeshProUGUI amountText;   // ui_text_reward_value

        private void OnEnable()  => game.OnBalanceChanged += HandleBalance;
        private void OnDisable() => game.OnBalanceChanged -= HandleBalance;

        private void HandleBalance(int balance) => amountText.text = balance.ToString();
    }
}