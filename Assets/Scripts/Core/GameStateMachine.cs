using System;

namespace VertigoCase.Core
{
    /// <summary>
    /// Holds the current <see cref="GameState"/> and announces every change through an event.
    /// Pure C# (no MonoBehaviour) so the game flow can be reasoned about and tested without a scene.
    /// Listeners (UI, GameController) react to state changes instead of polling every frame.
    /// </summary>
    public class GameStateMachine
    {
        // Current state. Read-only from outside; only ChangeState() may write it.
        public GameState Current { get; private set; } = GameState.Idle;

        // Observer hook: fires with the new state whenever it actually changes.
        public event Action<GameState> OnStateChanged;

        public void ChangeState(GameState next)
        {
            if (Current == next) return;   // no-op if we're already there (avoids duplicate events)
            Current = next;
            OnStateChanged?.Invoke(next);  // notify every listener of the new state
        }
    }
}