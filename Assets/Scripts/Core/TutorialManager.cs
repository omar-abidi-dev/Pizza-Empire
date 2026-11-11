using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// First-time guide: pick up pizzas → serve the customer → collect the cash → unlock a new area.
    /// Each step points the guide arrow (and floor pointer) at its target and shows on the
    /// objective card. Runs once; saves are marked done (older saves skip it).
    /// </summary>
    [DefaultExecutionOrder(70)] // after the scene, plots and HUD exist
    public class TutorialManager : MonoBehaviour
    {
        private enum Step { PickUp, Serve, Collect, Unlock, Done }

        public static TutorialManager Instance { get; private set; }

        public bool IsActive => _step != Step.Done;
        public int StepNumber => (int)_step + 1;
        public const int StepCount = 4;
        public string Title => _step switch
        {
            Step.PickUp => "Pick up pizzas at the oven",
            Step.Serve => "Serve the customer at the counter",
            Step.Collect => "Collect your cash",
            Step.Unlock => "Unlock a new area",
            _ => ""
        };

        /// <summary>Progress text for the card, e.g. "$40/$135" while saving up for the first area.</summary>
        public string ProgressText { get; private set; } = "0/1";
        public float Progress01 { get; private set; }

        public event System.Action Changed;

        private Step _step = Step.Done;
        private Player.PlayerStack _stack;
        private Transform _oven, _counter, _cash;
        private float _poll;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            var data = Save.SaveManager.Instance != null ? Save.SaveManager.Instance.Data : null;
            bool veteran = data == null || data.tutorialDone || data.plots.Count > 1 || data.level > 1;
            if (veteran) { Finish(false); return; }

            var player = GameObject.FindWithTag("Player");
            _stack = player != null ? player.GetComponent<Player.PlayerStack>() : null;
            _oven = Find("Oven");
            _counter = Find("CashierZone");
            _cash = Find("MoneySpot");

            GameEvents.CustomerServed += OnServed;
            GameEvents.CashCollected += OnCash;
            GameEvents.PlotBought += OnPlot;
            UI.FloorPointer.Create();
            Enter(Step.PickUp);
        }

        private void OnDestroy()
        {
            GameEvents.CustomerServed -= OnServed;
            GameEvents.CashCollected -= OnCash;
            GameEvents.PlotBought -= OnPlot;
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!IsActive) return;
            _poll -= Time.deltaTime;
            if (_poll > 0f) return;
            _poll = 0.2f;

            if (_step == Step.PickUp && _stack != null && _stack.Count > 0) Enter(Step.Serve);
            else if (_step == Step.Unlock) UpdateUnlockProgress();
        }

        private void OnServed() { if (_step == Step.Serve || _step == Step.PickUp) Enter(Step.Collect); }
        private void OnCash(int _) { if (_step == Step.Collect) Enter(Step.Unlock); }
        private void OnPlot() { if (IsActive) Finish(true); }

        private void Enter(Step step)
        {
            _step = step;
            ProgressText = "0/1";
            Progress01 = 0f;
            switch (step)
            {
                case Step.PickUp: Upgrades.GuideArrow.SetFocus(_oven, 2.6f); break;
                case Step.Serve: Upgrades.GuideArrow.SetFocus(_counter, 2.2f); break;
                case Step.Collect: Upgrades.GuideArrow.SetFocus(_cash, 1.9f); break;
                case Step.Unlock:
                    Upgrades.GuideArrow.SetFocus(null); // the arrow goes back to pads
                    UpdateUnlockProgress();
                    break;
            }
            if (step != Step.PickUp) AudioManager.Instance?.Play(SoundType.ChaChing, 0.6f);
            Changed?.Invoke();
        }

        /// <summary>Show savings towards the cheapest pad; point at it once affordable.</summary>
        private void UpdateUnlockProgress()
        {
            int money = Money.MoneyManager.Instance != null ? Money.MoneyManager.Instance.TotalMoney : 0;
            int cheapest = int.MaxValue;
            foreach (var zone in Upgrades.UpgradeZone.All) cheapest = Mathf.Min(cheapest, zone.Remaining);
            if (cheapest == int.MaxValue) return;
            string text = money >= cheapest ? "Stand on a pad!" : $"${money}/${cheapest}";
            if (text == ProgressText) return;
            ProgressText = text;
            Progress01 = Mathf.Clamp01((float)money / cheapest);
            Changed?.Invoke();
        }

        private void Finish(bool celebrate)
        {
            _step = Step.Done;
            Upgrades.GuideArrow.SetFocus(null);
            var data = Save.SaveManager.Instance != null ? Save.SaveManager.Instance.Data : null;
            if (data != null) data.tutorialDone = true;
            if (celebrate)
            {
                UI.Banner.Show("GREAT JOB! KEEP GROWING!");
                GemManager.Instance?.AddGems(3);
            }
            var pointer = FindAnyObjectByType<UI.FloorPointer>();
            if (pointer != null) Destroy(pointer.gameObject);
            Changed?.Invoke();
        }

        private static Transform Find(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.transform : null;
        }
    }
}
