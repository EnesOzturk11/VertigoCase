using System;
using System.Collections.Generic;
using VertigoCase.Data;

namespace VertigoCase.Economy
{
    /// <summary>Read/write contract for rewards collected during one run.</summary>
    public interface IRewardWallet
    {
        IReadOnlyDictionary<RewardType, int> Amounts { get; }
        int Total { get; }
        event Action OnChanged;
        void Add(RewardType type, int amount);
        void Clear();
    }
}
