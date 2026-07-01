using System;

namespace VertigoCase.Economy
{
    /// <summary>
    /// The "bank": the single source of truth for the total reward value collected during a run.
    /// A winning spin calls <see cref="Add"/>; hitting the bomb calls <see cref="ResetBank"/>.
    /// Pure C# (no MonoBehaviour) so it can be unit-tested without a scene. UI never stores the total —
    /// it listens to <see cref="OnBalanceChanged"/> and displays whatever the bank reports.
    /// </summary>
    public class RewardService
    {
        // Total accumulated value. Read-only from outside; only Add()/ResetBank() may change it.
        public int Balance { get; private set; }

        // Observer hook: fires with the new balance whenever it actually changes.
        public event Action<int> OnBalanceChanged;

        public void Add(int amount)
        {
            if (amount <= 0) return;           // a zero/negative reward must not corrupt the bank
            Balance += amount;
            OnBalanceChanged?.Invoke(Balance); // notify listeners (e.g. the counter UI)
        }

        public void ResetBank()
        {
            if (Balance == 0) return;          // already empty -> no change, no event
            Balance = 0;
            OnBalanceChanged?.Invoke(Balance);
        }
    }
}