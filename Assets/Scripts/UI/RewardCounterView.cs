using System;
using UnityEngine;
using TMPro;
using VertigoCase.Core;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    /// <summary>
    /// Shows one explicitly selected reward type; heterogeneous rewards are never summed.
    /// </summary>
    public class RewardCounterView : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour game;
        [SerializeField] private TextMeshProUGUI amountText;   // ui_text_reward_value
        [SerializeField] private RewardType rewardType = RewardType.Cash;

        private IInventoryPort gamePort;

        private void Awake()
        {
            gamePort = game as IInventoryPort ??
                       throw new InvalidOperationException(
                           "RewardCounterView requires a component implementing IInventoryPort.");
            if (amountText == null)
                throw new InvalidOperationException("Reward counter label is missing.");
        }

        private void OnEnable()
        {
            gamePort.OnInventoryChanged += HandleInventory;
            HandleInventory(gamePort.Inventory);
        }

        private void OnDisable()
        {
            if (gamePort != null)
                gamePort.OnInventoryChanged -= HandleInventory;
        }

        private void HandleInventory(System.Collections.Generic.IReadOnlyDictionary<RewardType, int> inventory)
        {
            amountText.text = inventory.TryGetValue(rewardType, out int amount)
                ? amount.ToString()
                : "0";
        }
    }
}
