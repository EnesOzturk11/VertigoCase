namespace VertigoCase.Core
{
    /// <summary>Connects a game session to wheel input and presentation.</summary>
    public interface IWheelCoordinator
    {
        void Connect(IGameSession session);
        void Disconnect();
    }
}
