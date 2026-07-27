using System;
using System.Collections.Generic;
using VertigoCase.Data;

namespace VertigoCase.Core
{
    public interface IGameOverPort
    {
        GameState State { get; }
        event Action<GameState> OnStateChanged;
        void Revive();
        void GiveUp();
    }

    public interface IInventoryPort
    {
        IReadOnlyDictionary<RewardType, int> Inventory { get; }
        event Action<IReadOnlyDictionary<RewardType, int>> OnInventoryChanged;
    }

    public interface IBalancePort
    {
        event Action<int> OnBalanceChanged;
    }

    public interface IZonePort
    {
        event Action<int, ZoneType> OnZoneChanged;
        ZoneType TypeOf(int zone);
    }

    public interface ILeavePort
    {
        bool CanLeave { get; }
        event Action<GameState> OnStateChanged;
        event Action<int, ZoneType> OnZoneChanged;
        void Leave();
    }
}
