using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly IReadOnlyDictionary<RewardType, int> _readOnlyAmounts;

        // The wrapper prevents consumers from casting the public view back to Dictionary and
        // bypassing wallet invariants. Values remain live while mutation stays private.
        public IReadOnlyDictionary<RewardType, int> Amounts => _readOnlyAmounts;

        public RewardService()
        {
            _readOnlyAmounts = new ReadOnlyDictionary<RewardType, int>(_amounts);
        }

        public void Add(RewardType type, int amount)
        {
            ValidateType(type);
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(amount), amount, "Reward amount must be positive.");

            _amounts.TryGetValue(type, out int current);
            _amounts[type] = checked(current + amount);
        }

        // How much of one type has been collected (0 if none yet).
        public int AmountOf(RewardType type)
        {
            ValidateType(type);
            _amounts.TryGetValue(type, out int value);
            return value;
        }

        public bool CanAfford(RewardType type, int amount)
        {
            ValidateType(type);
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

            return true;
        }

        public void Clear()
        {
            _amounts.Clear();
        }

        private static void ValidateType(RewardType type)
        {
            if (!Enum.IsDefined(typeof(RewardType), type))
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown reward type.");
        }
    }
}
