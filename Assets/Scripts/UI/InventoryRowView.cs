using System;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Config;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    /// <summary>
    /// One inventory reward group. The row keeps the reward identity and visual amount together,
    /// so each collected type owns an independent UI object and counter.
    /// </summary>
    public sealed class InventoryRowView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI amountText;

        private int currentAmount;

        public RewardType Type { get; private set; }

        public void Bind(RewardType type, Sprite icon, int amount, bool animate)
        {
            if (type == RewardType.Bomb)
                throw new ArgumentException("Bomb cannot be bound to an inventory row.", nameof(type));
            if (icon == null)
                throw new ArgumentNullException(nameof(icon));

            Type = type;
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            SetAmount(amount, false);

            if (!animate) return;

            transform.DOKill();
            transform.localScale = Vector3.one * 0.72f;
            transform
                .DOScale(Vector3.one, GameConstants.Inventory.RowAnimationSeconds)
                .SetEase(Ease.OutBack);
        }

        public void SetAmount(int amount, bool animate)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount cannot be negative.");

            bool changed = amount != currentAmount;
            currentAmount = amount;
            amountText.text = amount.ToString("N0", CultureInfo.InvariantCulture);

            if (!animate || !changed) return;

            amountText.transform.DOKill();
            amountText.transform.localScale = Vector3.one;
            amountText.transform.DOPunchScale(
                Vector3.one * 0.18f,
                GameConstants.Inventory.RowAnimationSeconds,
                5,
                0.4f);
        }

        public void HideAndDestroy()
        {
            transform.DOKill();
            transform
                .DOScale(Vector3.zero, GameConstants.Inventory.RowAnimationSeconds)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject));
        }

        private void OnDisable()
        {
            transform.DOKill();
            if (amountText != null)
                amountText.transform.DOKill();
        }
    }
}
