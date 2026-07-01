namespace VertigoCase.Core
{
    /// <summary>
    /// The high-level states of the game loop. Idle = waiting for the player to spin or leave;
    /// Spinning = the wheel is animating; Resolving = the landed slice is being applied (reward/bomb);
    /// GameOver = a bomb was hit and the run ended. Kept as a plain enum so the state is a single,
    /// readable value instead of a pile of booleans.
    /// </summary>
    public enum GameState
    {
        Idle,
        Spinning,
        Resolving,
        GameOver
    }
}