using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Config;
using VertigoCase.Core;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    /// <summary>
    /// Permanent inventory sidebar. It owns one reusable row per configured reward type and only
    /// updates the row whose amount changed; different reward types never share a counter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventorySidebarView : MonoBehaviour
    {
        [Serializable]
        private sealed class RewardIcon
        {
            [SerializeField] private RewardType type;
            [SerializeField] private Sprite icon;

            public RewardType Type => type;
            public Sprite Icon => icon;
        }

        [Header("References")]
        [SerializeField] private MonoBehaviour game;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private RectTransform content;
        [SerializeField] private InventoryRowView rowPrefab;

        [Header("Icons (RewardType -> Sprite)")]
        [SerializeField] private RewardIcon[] icons;

        [Header("Reward collect animation")]
        [SerializeField] private RectTransform rewardFlyLayer;
        [SerializeField] private RectTransform rewardFlyOrigin;
        [SerializeField, Min(1)] private int rewardFlyIconCount =
            GameConstants.Inventory.RewardFlyIconCount;

        private IInventoryPort gamePort;
        private readonly Dictionary<RewardType, InventoryRowView> rows =
            new Dictionary<RewardType, InventoryRowView>();
        private RewardFlyAnimator rewardFlyAnimator;

        private void Awake()
        {
            gamePort = game as IInventoryPort ??
                       throw new InvalidOperationException(
                           "InventorySidebarView requires a component implementing IInventoryPort.");

            if (panelRoot == null) throw new InvalidOperationException("Inventory panel is missing.");
            if (content == null) throw new InvalidOperationException("Inventory content is missing.");
            if (rowPrefab == null) throw new InvalidOperationException("Inventory row prefab is missing.");
            if (rewardFlyLayer == null)
                throw new InvalidOperationException("Reward fly animation layer is missing.");
            if (rewardFlyOrigin == null)
                throw new InvalidOperationException("Reward fly animation origin is missing.");

            ValidateIconConfiguration();
            rewardFlyAnimator = new RewardFlyAnimator(
                gameObject,
                rewardFlyLayer,
                rewardFlyOrigin,
                rewardFlyIconCount);
            ClearSceneRows();
        }

        private void OnEnable()
        {
            panelRoot.SetActive(true);
            gamePort.OnInventoryChanged += HandleInventoryChanged;
            Synchronize(gamePort.Inventory, false);
        }

        private void OnDisable()
        {
            if (gamePort != null)
                gamePort.OnInventoryChanged -= HandleInventoryChanged;

            rewardFlyAnimator?.CancelAll();
        }

        private void HandleInventoryChanged(IReadOnlyDictionary<RewardType, int> inventory)
        {
            Synchronize(inventory, true);
        }

        private void Synchronize(
            IReadOnlyDictionary<RewardType, int> inventory,
            bool animate)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));

            if (inventory.Count == 0)
                rewardFlyAnimator.CancelAll();

            foreach (RewardIcon entry in icons)
            {
                int amount = inventory.TryGetValue(entry.Type, out int value) ? value : 0;

                if (amount <= 0)
                {
                    RemoveRow(entry.Type, animate);
                    continue;
                }

                if (!rows.TryGetValue(entry.Type, out InventoryRowView row))
                {
                    row = Instantiate(rowPrefab, content);
                    row.Bind(entry.Type, entry.Icon, animate ? 0 : amount, animate);
                    rows.Add(entry.Type, row);

                    if (animate)
                        rewardFlyAnimator.Play(row, entry.Icon, amount);
                    continue;
                }

                if (animate && amount > row.Amount)
                    rewardFlyAnimator.Play(row, entry.Icon, amount);
                else
                {
                    rewardFlyAnimator.Cancel(row);
                    row.SetAmount(amount, animate);
                }
            }
        }

        private void RemoveRow(RewardType type, bool animate)
        {
            if (!rows.TryGetValue(type, out InventoryRowView row))
                return;

            rows.Remove(type);
            rewardFlyAnimator.Cancel(row);
            if (animate)
                row.HideAndDestroy();
            else
                Destroy(row.gameObject);
        }

        private void ValidateIconConfiguration()
        {
            if (icons == null || icons.Length == 0)
                throw new InvalidOperationException("At least one inventory reward icon is required.");

            HashSet<RewardType> configuredTypes = new HashSet<RewardType>();
            foreach (RewardIcon entry in icons)
            {
                if (entry == null)
                    throw new InvalidOperationException("Inventory icon entries cannot be null.");
                if (entry.Icon == null)
                    throw new InvalidOperationException(
                        "Inventory icon is missing for " + entry.Type + ".");
                if (!configuredTypes.Add(entry.Type))
                    throw new InvalidOperationException(
                        "Inventory contains a duplicate icon mapping for " + entry.Type + ".");
            }

            foreach (RewardType type in Enum.GetValues(typeof(RewardType)))
            {
                if (!configuredTypes.Contains(type))
                    throw new InvalidOperationException(
                        "Inventory icon configuration is missing " + type + ".");
            }
        }

        private void ClearSceneRows()
        {
            rows.Clear();
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                GameObject staleRow = content.GetChild(i).gameObject;
                staleRow.SetActive(false);
                Destroy(staleRow);
            }
        }
    }
}
