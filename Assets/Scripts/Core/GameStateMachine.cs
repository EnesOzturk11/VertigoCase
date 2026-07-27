using System;
using System.Collections.Generic;

namespace VertigoCase.Core
{
    /// <summary>
    /// Holds the current <see cref="GameState"/>, validates normal transitions and announces every
    /// accepted change. Pure C# (no MonoBehaviour) so it can be tested without a scene.
    /// </summary>
    public sealed class GameStateMachine : IGameStateMachine
    {
        private static readonly IReadOnlyDictionary<GameState, HashSet<GameState>> AllowedTransitions =
            new Dictionary<GameState, HashSet<GameState>>
            {
                { GameState.Idle, new HashSet<GameState> { GameState.Spinning } },
                { GameState.Spinning, new HashSet<GameState> { GameState.Resolving } },
                {
                    GameState.Resolving,
                    new HashSet<GameState> { GameState.Idle, GameState.GameOver }
                },
                { GameState.GameOver, new HashSet<GameState> { GameState.Idle } }
            };

        // Current state. Read-only from outside; only validated transitions or Reset may write it.
        public GameState Current { get; private set; } = GameState.Idle;

        // Observer hook: fires with the new state whenever it actually changes.
        public event Action<GameState> OnStateChanged;

        public void ChangeState(GameState next)
        {
            if (Current == next) return;

            if (!AllowedTransitions.TryGetValue(Current, out HashSet<GameState> targets) ||
                !targets.Contains(next))
            {
                throw new InvalidOperationException(
                    $"Invalid game-state transition: {Current} -> {next}.");
            }

            SetState(next);
        }

        /// <summary>
        /// Explicit run-reset path. Unlike a normal transition, reset may return any state to Idle.
        /// </summary>
        public void Reset()
        {
            if (Current == GameState.Idle) return;
            SetState(GameState.Idle);
        }

        private void SetState(GameState next)
        {
            Current = next;
            OnStateChanged?.Invoke(next);
        }
    }
}
