using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Restaurant level: XP from serving customers (+1) and buying plots (+5).
    /// Level n needs 10 + 5n XP. Each level-up pays a few gems.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private const int XpPerCustomer = 1;
        private const int XpPerPlot = 5;

        public static LevelManager Instance { get; private set; }

        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }
        public int XpToNext => 10 + 5 * Level;

        public event System.Action Changed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            var data = Save.SaveManager.Instance != null ? Save.SaveManager.Instance.Data : null;
            if (data != null)
            {
                Level = Mathf.Max(1, data.level);
                Xp = data.xp;
            }
        }

        private void OnEnable()
        {
            GameEvents.CustomerServed += OnServed;
            GameEvents.PlotBought += OnPlot;
        }

        private void OnDisable()
        {
            GameEvents.CustomerServed -= OnServed;
            GameEvents.PlotBought -= OnPlot;
        }

        private void OnServed() => AddXp(XpPerCustomer);
        private void OnPlot() => AddXp(XpPerPlot);

        public void AddXp(int amount)
        {
            Xp += amount;
            while (Xp >= XpToNext)
            {
                Xp -= XpToNext;
                Level++;
                UI.Banner.Show($"LEVEL {Level}!");
                AudioManager.Instance?.Play(SoundType.Fanfare, 0.8f);
                GemManager.Instance?.AddGems(2);
            }
            var data = Save.SaveManager.Instance != null ? Save.SaveManager.Instance.Data : null;
            if (data != null) { data.level = Level; data.xp = Xp; }
            Changed?.Invoke();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
