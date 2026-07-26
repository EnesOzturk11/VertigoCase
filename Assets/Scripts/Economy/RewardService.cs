using System;
using System.Collections.Generic;
using VertigoCase.Data;

namespace VertigoCase.Economy
{
    /// <summary>
    /// The "bank": the single source of truth for the rewards collected during a run, stored per type
    /// (Cash/Gold/Chest/Weapon) in a dictionary. A winning spin calls Add(type, amount); hitting the
    /// bomb (via GiveUp) calls Clear(). Pure C# (no MonoBehaviour) so it can be unit-tested without a
    /// scene. UI never stores the totals — it reads Amounts/Total and reacts to OnChanged.
    /// </summary>
    public sealed class RewardService : IRewardWallet
    {
        // Per-type accumulated amount. Private so only Add()/Clear() can mutate it.
        private readonly Dictionary<RewardType, int> _amounts = new Dictionary<RewardType, int>();

        // Read-only view for the inventory UI (iterate type -> count).
        public IReadOnlyDictionary<RewardType, int> Amounts => _amounts;

        // Combined value across every type (used by the main HUD counter).
        public int Total { get; private set; }

        // Fires whenever the inventory changes; listeners re-read Amounts/Total.
        public event Action OnChanged;

        public void Add(RewardType type, int amount)
        {
            if (amount <= 0) return;                  // a zero/negative reward must not corrupt the bank
            _amounts.TryGetValue(type, out int current);
            _amounts[type] = current + amount;        // accumulate this type
            Total += amount;
            OnChanged?.Invoke();
        }

        // How much of one type has been collected (0 if none yet).
        public int AmountOf(RewardType type)
        {
            _amounts.TryGetValue(type, out int value);
            return value;
        }

        public void Clear()
        {
            if (_amounts.Count == 0) return;          // already empty -> no change, no event
            _amounts.Clear();
            Total = 0;
            OnChanged?.Invoke();
        }
    }
}
