using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;
using VertigoCase.Wheel;

namespace VertigoCase.UI
{
    /// <summary>
    /// Wires the Spin button to the SpinController entirely from code (the case forbids binding
    /// OnClick in the Inspector). It is a thin View: it only forwards the click to the logic and
    /// listens to spin events to enable/disable the button. It contains no spin math itself.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SpinButtonBinder : MonoBehaviour
    {
        [SerializeField] private Button spinButton;
        [SerializeField] private SpinController spinController;

        // Auto-assign references in the editor so we don't hand-wire what code can find (case rule).
        private void OnValidate()
        {
            if (spinButton == null) spinButton = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (spinButton == null || spinController == null) return; // nothing to bind yet

            spinButton.onClick.AddListener(spinController.Spin);      // bind the click in code, not in the Inspector
            spinController.OnSpinStarted   += HandleSpinStarted;
            spinController.OnSpinCompleted += HandleSpinCompleted;
        }

        private void OnDisable() // mirror OnEnable so we never double-subscribe or leak listeners
        {
            if (spinButton == null || spinController == null) return;

            spinButton.onClick.RemoveListener(spinController.Spin);
            spinController.OnSpinStarted   -= HandleSpinStarted;
            spinController.OnSpinCompleted -= HandleSpinCompleted;
        }

        // While spinning the button is locked; when the spin ends it becomes usable again.
        private void HandleSpinStarted()                   => spinButton.interactable = false;
        private void HandleSpinCompleted(WheelSlice slice) => spinButton.interactable = true;
    }
}
