using System;
using System.Collections.Generic;
using VertigoCase.Data;

namespace VertigoCase.Core
{
    /// <summary>Application-facing contract for one game run.</summary>
    public interface IGameSession
    {
        event Action<int> OnBalanceChanged;
        event Action<IReadOnlyDictionary<RewardType, int>> OnInventoryChanged;
        event Action<int, ZoneType> OnZoneChanged;
        event Action<GameState> OnStateChanged;
        event Action<int> OnCashedOut;

        GameState State { get; }
        IReadOnlyDictionary<RewardType, int> Inventory { get; }
        bool CanLeave { get; }

        ZoneType TypeOf(int zone);
        void Initialize();
        void StartSpin();
        void CompleteSpin(WheelSlice slice);
        void Restart();
        void Leave();
        void Revive();
        void GiveUp();
    }
}
