using System;
using System.Collections.Generic;
using VertigoCase.Data;

namespace VertigoCase.Economy
{
    /// <summary>
    /// The "bank": the single source of truth for the rewards collected during a run, stored per type
    /// (Cash/Gold/Chest/Weapon) in a dictionary. A winning spin calls Add(type, amount); hitting the
    /// bomb calls Clear(). Different reward types are never collapsed into a meaningless scalar.
    /// </summary>
    public sealed class RewardService : IRewardWallet
    {
        // Per-type accumulated amount. Private so only wallet operations can mutate it.
        private readonly Dictionary<RewardType, int> _amounts = new Dictionary<RewardType, int>();

        // Read-only view for the inventory UI (iterate type -> count).
        public IReadOnlyDictionary<RewardType, int> Amounts => _amounts;

        // Fires whenever the inventory changes; listeners re-read Amounts.
        public event Action OnChanged;

        public void Add(RewardType type, int amount)
        {
            if (amount <= 0) return;                  // a zero/negative reward must not corrupt the bank
            _amounts.TryGetValue(type, out int current);
            _amounts[type] = checked(current + amount);
            OnChanged?.Invoke();
        }

        // How much of one type has been collected (0 if none yet).
        public int AmountOf(RewardType type)
        {
            _amounts.TryGetValue(type, out int value);
            return value;
        }

        public bool CanAfford(RewardType type, int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount cannot be negative.");

            return AmountOf(type) >= amount;
        }

        public bool Spend(RewardType type, int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Spend amount must be positive.");
            if (!CanAfford(type, amount))
                return false;

            int remaining = AmountOf(type) - amount;
            if (remaining == 0)
                _amounts.Remove(type);
            else
                _amounts[type] = remaining;

            OnChanged?.Invoke();
            return true;
        }

        public void Clear()
        {
            if (_amounts.Count == 0) return;          // already empty -> no change, no event
            _amounts.Clear();
            OnChanged?.Invoke();
        }
    }
}
