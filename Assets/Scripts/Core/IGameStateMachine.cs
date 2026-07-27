using System;

namespace VertigoCase.Core
{
    /// <summary>Validated state operations observed and driven by a game session.</summary>
    public interface IGameStateMachine
    {
        GameState Current { get; }
        event Action<GameState> OnStateChanged;
        void ChangeState(GameState next);
        void Reset();
    }
}
