using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
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

        [Header("Reward collect animation")]
        [SerializeField] private RectTransform rewardFlyLayer;
        [SerializeField] private RectTransform rewardFlyOrigin;
        [SerializeField, Min(1)] private int rewardFlyIconCount =
            GameConstants.Inventory.RewardFlyIconCount;

        private IInventoryPort gamePort;
        private readonly Dictionary<RewardType, InventoryRowView> rows =
            new Dictionary<RewardType, InventoryRowView>();
        private readonly List<Sequence> rewardFlySequences = new List<Sequence>();
        private readonly List<GameObject> rewardFlyIcons = new List<GameObject>();

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

            CancelRewardFlyAnimations();
        }

        private void HandleInventoryChanged(IReadOnlyDictionary<RewardType, int> inventory)
        {
            Synchronize(inventory, true);
        }

        private void Synchronize(
            IReadOnlyDictionary<RewardType, int> inventory,
            bool animate)
        {
            if (inventory.Count == 0)
                CancelRewardFlyAnimations();

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
                    row.Bind(entry.type, entry.icon, animate ? 0 : amount, animate);
                    rows.Add(entry.type, row);

                    if (animate)
                        PlayRewardFlyAnimation(row, entry.icon, amount);
                    continue;
                }

                if (animate && amount > row.Amount)
                    PlayRewardFlyAnimation(row, entry.icon, amount);
                else
                    row.SetAmount(amount, animate);
            }
        }

        private void PlayRewardFlyAnimation(
            InventoryRowView row,
            Sprite rewardIcon,
            int finalAmount)
        {
            Canvas.ForceUpdateCanvases();

            var spawnedIcons = new List<GameObject>(rewardFlyIconCount);
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            rewardFlySequences.Add(sequence);

            Vector3 origin = rewardFlyOrigin.position;
            Vector3 target = row.IconTarget.position;

            for (int i = 0; i < rewardFlyIconCount; i++)
            {
                Image flyIcon = CreateRewardFlyIcon(rewardIcon, origin);
                spawnedIcons.Add(flyIcon.gameObject);

                float delay = i * GameConstants.Inventory.RewardFlyStaggerSeconds;
                float angle = i * Mathf.PI * 2f / rewardFlyIconCount;
                float radius = GameConstants.Inventory.RewardFlyScatterRadius *
                               (i % 2 == 0 ? 1f : 0.72f);
                Vector3 scatterTarget = origin +
                                        new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

                RectTransform iconTransform = flyIcon.rectTransform;
                sequence.Insert(
                    delay,
                    iconTransform.DOScale(Vector3.one, GameConstants.Inventory.RowAnimationSeconds)
                        .SetEase(Ease.OutBack));
                sequence.Insert(
                    delay,
                    iconTransform.DOMove(
                            scatterTarget,
                            GameConstants.Inventory.RewardFlyScatterSeconds)
                        .SetEase(Ease.OutQuad));
                sequence.Insert(
                    delay + GameConstants.Inventory.RewardFlyScatterSeconds,
                    iconTransform.DOMove(
                            target,
                            GameConstants.Inventory.RewardFlyTravelSeconds)
                        .SetEase(Ease.InQuad));
                sequence.Insert(
                    delay + GameConstants.Inventory.RewardFlyScatterSeconds +
                    GameConstants.Inventory.RewardFlyTravelSeconds -
                    GameConstants.Inventory.RewardFlyFadeSeconds,
                    flyIcon.DOFade(0f, GameConstants.Inventory.RewardFlyFadeSeconds));
            }

            sequence.OnComplete(() =>
            {
                if (row != null)
                    row.SetAmount(finalAmount, true);

                DestroyRewardFlyIcons(spawnedIcons);
                rewardFlySequences.Remove(sequence);
            });
        }

        private Image CreateRewardFlyIcon(Sprite sprite, Vector3 origin)
        {
            var iconObject = new GameObject(
                "ui_image_reward_fly_value",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            iconObject.layer = gameObject.layer;

            RectTransform iconTransform = iconObject.GetComponent<RectTransform>();
            iconTransform.SetParent(rewardFlyLayer, false);
            iconTransform.SetAsLastSibling();
            iconTransform.position = origin;
            iconTransform.sizeDelta = Vector2.one * GameConstants.Inventory.RewardFlyIconSize;
            iconTransform.localScale = Vector3.zero;

            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.maskable = false;

            rewardFlyIcons.Add(iconObject);
            return icon;
        }

        private void CancelRewardFlyAnimations()
        {
            for (int i = rewardFlySequences.Count - 1; i >= 0; i--)
                rewardFlySequences[i]?.Kill(false);

            rewardFlySequences.Clear();
            DestroyRewardFlyIcons(rewardFlyIcons);
            rewardFlyIcons.Clear();
        }

        private void DestroyRewardFlyIcons(IReadOnlyList<GameObject> iconsToDestroy)
        {
            for (int i = iconsToDestroy.Count - 1; i >= 0; i--)
            {
                GameObject icon = iconsToDestroy[i];
                rewardFlyIcons.Remove(icon);
                if (icon != null)
                    Destroy(icon);
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
