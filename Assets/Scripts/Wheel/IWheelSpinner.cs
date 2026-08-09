using System;
using VertigoCase.Data;

namespace VertigoCase.Wheel
{
    /// <summary>Spin animation lifecycle used by the wheel coordinator.</summary>
    public interface IWheelSpinner
    {
        event Action<WheelSlice> OnSpinCompleted;
        event Action OnSpinCancelled;
        void SetWheel(WheelData data);
        bool TrySpin();
        void CancelSpin();
    }
}
