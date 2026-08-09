using System.Collections.Generic;
using System;

namespace VertigoCase.Core
{
    /// <summary>
    /// Holds the current <see cref="GameState"/> and validates transitions. Event publication stays
    /// in GameSession so compound state, wallet and zone mutations are committed before observers run.
    /// </summary>
    public sealed class GameStateMachine : IGameStateMachine
    {
        private static readonly IReadOnlyDictionary<GameState, HashSet<GameState>> AllowedTransitions =
            new Dictionary<GameState, HashSet<GameState>>
            {
                { GameState.Idle, new HashSet<GameState> { GameState.Spinning } },
                {
                    GameState.Spinning,
                    new HashSet<GameState> { GameState.Idle, GameState.Resolving }
                },
                {
                    GameState.Resolving,
                    new HashSet<GameState> { GameState.Idle, GameState.GameOver }
                },
                // GameOver is terminal for a run. Restart uses Reset(); no unguarded transition
                // may bypass the bomb loss rule.
                { GameState.GameOver, new HashSet<GameState>() }
            };

        // Current state. Read-only from outside; only validated transitions or Reset may write it.
        public GameState Current { get; private set; } = GameState.Idle;

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
        }
    }
}
