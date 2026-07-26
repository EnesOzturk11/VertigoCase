using System;
using UnityEngine;
using TMPro;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Shows the bank balance. A pure View: it subscribes through <see cref="IBalancePort"/> and
    /// writes the number to a label. No game logic or polling.
    /// </summary>
    public class RewardCounterView : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour game;
        [SerializeField] private TextMeshProUGUI amountText;   // ui_text_reward_value

        private IBalancePort gamePort;

        private void Awake()
        {
            gamePort = game as IBalancePort ??
                       throw new InvalidOperationException(
                           "RewardCounterView requires a component implementing IBalancePort.");
        }

        private void OnEnable() => gamePort.OnBalanceChanged += HandleBalance;

        private void OnDisable()
        {
            if (gamePort != null)
                gamePort.OnBalanceChanged -= HandleBalance;
        }

        private void HandleBalance(int balance) => amountText.text = balance.ToString();
    }
}
