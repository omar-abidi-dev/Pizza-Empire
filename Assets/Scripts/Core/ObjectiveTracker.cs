using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// One goal at a time shown on the objective card: serve customers, collect cash or
    /// open new areas, growing endlessly. Completing one pays a gem and some XP.
    /// </summary>
    public class ObjectiveTracker : MonoBehaviour
    {
        public enum Kind { Serve, Collect, BuyArea }

        public static ObjectiveTracker Instance { get; private set; }

        public Kind Current { get; private set; }
        public int Target { get; private set; }
        public int Progress { get; private set; }
        public string Title => Current switch
        {
            Kind.Serve => $"Serve {Target} customers",
            Kind.Collect => $"Collect ${Target} cash",
            _ => Target == 1 ? "Unlock a new area" : $"Unlock {Target} new areas"
        };

        public event System.Action Changed;

        private int _index;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            var data = Save.SaveManager.Instance != null ? Save.SaveManager.Instance.Data : null;
            _index = data != null ? data.objectiveIndex : 0;
            Setup(_index);
            Progress = data != null ? Mathf.Min(data.objectiveProgress, Target - 1) : 0;
        }

        private void OnEnable()
        {
            GameEvents.CustomerServed += OnServed;
            GameEvents.PlotBought += OnPlot;
            GameEvents.CashCollected += OnCash;
        }

        private void OnDisable()
        {
            GameEvents.CustomerServed -= OnServed;
            GameEvents.PlotBought -= OnPlot;
            GameEvents.CashCollected -= OnCash;
        }

        private void OnServed() { if (Current == Kind.Serve) Advance(1); }
        private void OnPlot() { if (Current == Kind.BuyArea) Advance(1); }
        private void OnCash(int amount) { if (Current == Kind.Collect) Advance(amount); }

        /// <summary>Cycle serve → collect → buy, each round a bit bigger.</summary>
        private void Setup(int index)
        {
            int round = index / 3;
            Current = (Kind)(index % 3);
            Target = Current switch
            {
                Kind.Serve => 3 + round * 4,
                Kind.Collect => Mathf.RoundToInt(100f * Mathf.Pow(1.6f, round) / 10f) * 10,
                _ => 1 + round / 3
            };
            Progress = 0;
        }

        private void Advance(int amount)
        {
            Progress = Mathf.Min(Target, Progress + amount);
            if (Progress >= Target)
            {
                GemManager.Instance?.AddGems(1);
                LevelManager.Instance?.AddXp(3);
                AudioManager.Instance?.Play(SoundType.ChaChing, 0.8f);
                Setup(++_index);
            }
            var data = Save.SaveManager.Instance != null ? Save.SaveManager.Instance.Data : null;
            if (data != null) { data.objectiveIndex = _index; data.objectiveProgress = Progress; }
            Changed?.Invoke();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
