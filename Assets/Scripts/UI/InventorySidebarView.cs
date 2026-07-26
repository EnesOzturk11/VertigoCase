using System;
using System.Collections.Generic;
using UnityEngine;
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
            public RewardType type;
            public Sprite icon;
        }

        [Header("References")]
        [SerializeField] private MonoBehaviour game;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private RectTransform content;
        [SerializeField] private InventoryRowView rowPrefab;

        [Header("Icons (RewardType -> Sprite)")]
        [SerializeField] private RewardIcon[] icons;

        private IInventoryPort gamePort;
        private readonly Dictionary<RewardType, InventoryRowView> rows =
            new Dictionary<RewardType, InventoryRowView>();

        private void Awake()
        {
            gamePort = game as IInventoryPort ??
                       throw new InvalidOperationException(
                           "InventorySidebarView requires a component implementing IInventoryPort.");

            if (panelRoot == null) throw new InvalidOperationException("Inventory panel is missing.");
            if (content == null) throw new InvalidOperationException("Inventory content is missing.");
            if (rowPrefab == null) throw new InvalidOperationException("Inventory row prefab is missing.");

            ValidateIconConfiguration();
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
        }

        private void HandleInventoryChanged(IReadOnlyDictionary<RewardType, int> inventory)
        {
            Synchronize(inventory, true);
        }

        private void Synchronize(
            IReadOnlyDictionary<RewardType, int> inventory,
            bool animate)
        {
            foreach (RewardIcon entry in icons)
            {
                int amount = inventory.TryGetValue(entry.type, out int value) ? value : 0;

                if (amount <= 0)
                {
                    RemoveRow(entry.type, animate);
                    continue;
                }

                if (!rows.TryGetValue(entry.type, out InventoryRowView row))
                {
                    row = Instantiate(rowPrefab, content);
                    row.Bind(entry.type, entry.icon, amount, animate);
                    rows.Add(entry.type, row);
                    continue;
                }

                row.SetAmount(amount, animate);
            }
        }

        private void RemoveRow(RewardType type, bool animate)
        {
            if (!rows.TryGetValue(type, out InventoryRowView row))
                return;

            rows.Remove(type);
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
                if (entry.type == RewardType.Bomb)
                    throw new InvalidOperationException("Bomb is a failure state, not an inventory reward.");
                if (entry.icon == null)
                    throw new InvalidOperationException(
                        "Inventory icon is missing for " + entry.type + ".");
                if (!configuredTypes.Add(entry.type))
                    throw new InvalidOperationException(
                        "Inventory contains a duplicate icon mapping for " + entry.type + ".");
            }

            RewardType[] collectibleTypes =
            {
                RewardType.Cash,
                RewardType.Gold,
                RewardType.Chest,
                RewardType.Weapon
            };

            foreach (RewardType type in collectibleTypes)
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
                Destroy(content.GetChild(i).gameObject);
        }
    }
}
