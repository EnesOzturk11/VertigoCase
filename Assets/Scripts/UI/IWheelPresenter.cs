using VertigoCase.Data;

namespace VertigoCase.UI
{
    /// <summary>Minimal contract for displaying a wheel configuration.</summary>
    public interface IWheelPresenter
    {
        void SetWheel(WheelData data);
    }
}
