using System.Collections.Generic;
using VertigoCase.Data;

namespace VertigoCase.Economy
{
    /// <summary>Read/write contract for rewards collected during one run.</summary>
    public interface IRewardWallet
    {
        IReadOnlyDictionary<RewardType, int> Amounts { get; }
        void Add(RewardType type, int amount);
        int AmountOf(RewardType type);
        bool CanAfford(RewardType type, int amount);
        bool Spend(RewardType type, int amount);
        void Clear();
    }
}
