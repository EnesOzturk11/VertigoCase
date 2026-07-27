using System;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;
using VertigoCase.Wheel;

namespace VertigoCase.UI
{
    /// <summary>
    /// Wires the Spin button through <see cref="IWheelSpinner"/> (the case forbids binding OnClick
    /// in the Inspector). It only forwards clicks and reflects spin lifecycle events.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SpinButtonBinder : MonoBehaviour
    {
        [SerializeField] private Button spinButton;
        [SerializeField] private MonoBehaviour spinController;

        private IWheelSpinner spinner;

        // Auto-assign references in the editor so we don't hand-wire what code can find (case rule).
        private void OnValidate()
        {
            if (spinButton == null) spinButton = GetComponent<Button>();
        }

        private void Awake()
        {
            spinner = spinController as IWheelSpinner ??
                      throw new InvalidOperationException(
                          "SpinButtonBinder requires a component implementing IWheelSpinner.");
        }

        private void OnEnable()
        {
            if (spinButton == null) return;

            spinButton.onClick.AddListener(spinner.Spin);
            spinner.OnSpinStarted += HandleSpinStarted;
            spinner.OnSpinCompleted += HandleSpinCompleted;
        }

        private void OnDisable() // mirror OnEnable so we never double-subscribe or leak listeners
        {
            if (spinButton == null || spinner == null) return;

            spinButton.onClick.RemoveListener(spinner.Spin);
            spinner.OnSpinStarted -= HandleSpinStarted;
            spinner.OnSpinCompleted -= HandleSpinCompleted;
        }

        // While spinning the button is locked; when the spin ends it becomes usable again.
        private void HandleSpinStarted()                   => spinButton.interactable = false;
        private void HandleSpinCompleted(WheelSlice slice) => spinButton.interactable = true;
    }
}
