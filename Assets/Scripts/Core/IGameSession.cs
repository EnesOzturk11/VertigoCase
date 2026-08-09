using System;
using System.Collections.Generic;
using VertigoCase.Data;

namespace VertigoCase.Core
{
    /// <summary>Application-facing contract for one game run.</summary>
    public interface IGameSession
    {
        event Action<IReadOnlyDictionary<RewardType, int>> OnInventoryChanged;
        event Action<int, ZoneType> OnZoneChanged;
        event Action<GameState> OnStateChanged;
        event Action<IReadOnlyDictionary<RewardType, int>> OnCashedOut;

        GameState State { get; }
        IReadOnlyDictionary<RewardType, int> Inventory { get; }
        int CurrentZone { get; }
        ZoneType CurrentZoneType { get; }
        bool CanLeave { get; }

        ZoneType TypeOf(int zone);
        void Initialize();
        void StartSpin();
        void CancelSpin();
        void CompleteSpin(WheelSlice slice);
        void Leave();
        void GiveUp();
    }
}
