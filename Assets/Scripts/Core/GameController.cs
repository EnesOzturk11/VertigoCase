using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Data;
using VertigoCase.Economy;
using VertigoCase.UI;
using VertigoCase.Wheel;

namespace VertigoCase.Core
{
    /// <summary>
    /// The brain that wires the Day 3 pieces into one game loop. SpinController announces a result;
    /// this controller interprets it (bomb vs reward), updates the bank, advances the zone, swaps the
    /// wheel and drives the state machine. It owns the plain-C# services and re-broadcasts their
    /// events as a single hub the UI listens to (Observer + Dependency Inversion).
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene refs")]
        [SerializeField] private SpinController spinController;
        [SerializeField] private WheelView wheelView;

        [Header("Wheel data (ZoneType mapping)")]
        [SerializeField] private WheelData bronzeWheel;   // Normal
        [SerializeField] private WheelData silverWheel;   // Safe
        [SerializeField] private WheelData goldenWheel;   // Super

        // Plain-C# services: no scene presence, owned by this controller.
        private ZoneService zones = new ZoneService();
        private readonly RewardService bank = new RewardService();
        private readonly GameStateMachine fsm = new GameStateMachine();

        // Events the UI listens to (hub). GameController is the single publisher.
        public event Action<int> OnBalanceChanged;
        public event Action<int, ZoneType> OnZoneChanged;
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnCashedOut;   // amount taken when the player leaves (UI can celebrate)
        public event Action<IReadOnlyDictionary<RewardType, int>> OnInventoryChanged; // per-type inventory for the inventory UI

        public GameState State => fsm.Current;
        public IReadOnlyDictionary<RewardType, int> Inventory => bank.Amounts; // read-only, for the inventory panel to show
        public bool CanLeave => fsm.Current == GameState.Idle &&
                                (zones.CurrentType == ZoneType.Safe || zones.CurrentType == ZoneType.Super);

        private void OnEnable()
        {
            spinController.OnSpinStarted   += HandleSpinStarted;
            spinController.OnSpinCompleted += HandleSpinCompleted;
            bank.OnChanged                 += HandleBankChanged;      // bridge service -> UI hub
            fsm.OnStateChanged             += HandleStateChanged;
        }

        private void OnDisable()
        {
            spinController.OnSpinStarted   -= HandleSpinStarted;
            spinController.OnSpinCompleted -= HandleSpinCompleted;
            bank.OnChanged                 -= HandleBankChanged;
            fsm.OnStateChanged             -= HandleStateChanged;
        }

        private void Start()
        {
            ApplyCurrentZone();                     // initial wheel + zone label
            HandleBankChanged();                    // initial counter + inventory (empty)
        }

        private void HandleSpinStarted() => fsm.ChangeState(GameState.Spinning);

        private void HandleSpinCompleted(WheelSlice slice)
        {
            fsm.ChangeState(GameState.Resolving);

            if (slice.IsBomb)                       // BOMB -> danger! keep the bank until the player decides
            {
                fsm.ChangeState(GameState.GameOver);
                return;
            }

            // REWARD -> scale by zone type, then bank it
            IZoneStrategy strategy = ZoneStrategyFactory.For(zones.CurrentType);
            int amount = strategy.ScaleReward(slice.reward.baseAmount * slice.multiplier, zones.CurrentZone);
            bank.Add(slice.reward.type, amount);    // bank it under its reward type

            zones.Advance();                        // move to the next zone
            ApplyCurrentZone();                     // new wheel + label
            fsm.ChangeState(GameState.Idle);        // spinnable again
        }

        // Rebuild the run from scratch (after a cash-out or a game over).
        public void Restart()
        {
            zones = new ZoneService();
            bank.Clear();
            ApplyCurrentZone();
            HandleBankChanged();
            fsm.ChangeState(GameState.Idle);
        }

        // Pick the wheel for the current zone type and apply it to both the view and the controller.
        private void ApplyCurrentZone()
        {
            WheelData data = WheelForType(zones.CurrentType);
            wheelView.SetWheel(data);
            spinController.SetWheel(data);
            OnZoneChanged?.Invoke(zones.CurrentZone, zones.CurrentType);
        }

        private WheelData WheelForType(ZoneType type)
        {
            switch (type)
            {
                case ZoneType.Super: return goldenWheel;
                case ZoneType.Safe:  return silverWheel;
                default:             return bronzeWheel;
            }
        }

        // Forward service/machine events to the UI hub.
        private void HandleBankChanged()
        {
            OnBalanceChanged?.Invoke(bank.Total);     // combined total for the HUD counter
            OnInventoryChanged?.Invoke(bank.Amounts); // per-type breakdown for the inventory panel
        }
        private void HandleStateChanged(GameState s) => OnStateChanged?.Invoke(s);

        // Cash out: take the bank and end the run. Only valid while Idle in a safe/super zone.
        public void Leave()
        {
            if (!CanLeave) return;        // guard: mirrors CanLeave so a stray call can't cheat
            int taken = bank.Total;
            OnCashedOut?.Invoke(taken);   // announce how much was collected
            Restart();                    // pocket the reward, reset the run
        }

        // From GameOver: keep the collected rewards and continue on the same zone (revive).
        public void Revive()
        {
            if (fsm.Current != GameState.GameOver) return;  // only meaningful after a bomb
            fsm.ChangeState(GameState.Idle);                // bank untouched -> rewards kept
        }

        // From GameOver: accept the loss and start a fresh run (give up).
        public void GiveUp()
        {
            if (fsm.Current != GameState.GameOver) return;
            Restart();                                      // wipes the bank, back to zone 1
        }
    }
}