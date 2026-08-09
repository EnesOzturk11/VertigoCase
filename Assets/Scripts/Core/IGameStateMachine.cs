namespace VertigoCase.Core
{
    /// <summary>Validated state storage driven by a game session.</summary>
    public interface IGameStateMachine
    {
        GameState Current { get; }
        void ChangeState(GameState next);
        void Reset();
    }
}
