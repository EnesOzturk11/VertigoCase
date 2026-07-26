using System;
using VertigoCase.Data;

namespace VertigoCase.Wheel
{
    /// <summary>Spin lifecycle and wheel-input contract used by controllers and button adapters.</summary>
    public interface IWheelSpinner
    {
        event Action OnSpinStarted;
        event Action<WheelSlice> OnSpinCompleted;
        void SetWheel(WheelData data);
        void Spin();
    }
}
