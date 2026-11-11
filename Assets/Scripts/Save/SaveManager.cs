using UnityEngine;
using System;
using System.IO;

namespace ShopIdle.Save
{
    [DefaultExecutionOrder(-100)]
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance => _instance;

        [SerializeField] private float _autoSaveInterval = 10f;

        private GameData _data;
        private float _saveTimer;
        private string _filePath;
        private bool _initialized;

        public GameData Data => _data;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            _filePath = Path.Combine(Application.persistentDataPath, "save.json");
            Load();

            // Ensure RestaurantManager exists (persists across scene reloads)
            if (Core.RestaurantManager.Instance == null)
            {
                var rmObj = new GameObject("RestaurantManager");
                rmObj.AddComponent<Core.RestaurantManager>();
            }
        }

        private void Start()
        {
            // Title screen first
            EnsureManager<UI.TitleScreen>("TitleScreen");

            // Bootstrap new managers
            EnsureManager<Core.PrestigeManager>("PrestigeManager");
            EnsureManager<Core.GemManager>("GemManager");
            EnsureManager<Core.DailyQuestManager>("DailyQuestManager");
            EnsureManager<Core.RushHourManager>("RushHourManager");
            EnsureManager<Core.SceneThemer>("SceneThemer");
            EnsureManager<Core.LevelManager>("LevelManager");
            EnsureManager<Core.ObjectiveTracker>("ObjectiveTracker");
            EnsureManager<UI.UIBuilder>("UIBuilder");

            // Delay initialization flag to next frame so all managers finish Start()
            StartCoroutine(SetInitializedNextFrame());
        }

        private void EnsureManager<T>(string name) where T : MonoBehaviour
        {
            if (UnityEngine.Object.FindAnyObjectByType<T>() == null)
            {
                var obj = new GameObject(name);
                obj.AddComponent<T>();
            }
        }

        private System.Collections.IEnumerator SetInitializedNextFrame()
        {
            yield return null;
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;
            _saveTimer += Time.deltaTime;
            if (_saveTimer >= _autoSaveInterval)
            {
                _saveTimer = 0f;
                SaveGame();
            }
        }

        public void SaveGame()
        {
            GatherState();
            _data.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(_filePath, json);
        }

        public void Load()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                _data = JsonUtility.FromJson<GameData>(json);
            }
            else
            {
                _data = new GameData();
            }
        }

        /// <summary>
        /// Calculates offline earnings based on hired workers and time away.
        /// Returns 0 if less than 60 seconds passed.
        /// </summary>
        public int CalculateOfflineEarnings()
        {
            if (_data.lastSaveTimestamp <= 0) return 0;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsed = now - _data.lastSaveTimestamp;

            if (elapsed < 60) return 0;

            // Cap at 2 hours
            const long maxSeconds = 2 * 60 * 60;
            if (elapsed > maxSeconds) elapsed = maxSeconds;

            float minutes = elapsed / 60f;

            // Base rate: every owned plot and every hired worker earns a little per minute
            int workers = 0;
            foreach (var plot in _data.plots)
                if (plot.Kind == Plots.PlotKind.StaffRoom) workers++;
            int incomePerMinute = 3 * _data.plots.Count + 6 * workers;

            if (incomePerMinute <= 0) return 0;

            // Scale by restaurant economy (higher restaurants earn more)
            float economyScale = 1f + 0.3f * (Kitchen.Menu.SizeFor(_data.plots.Count) - 1); // pricier menu items

            // Apply prestige multiplier
            var pm = Core.PrestigeManager.Instance;
            float prestigeMult = pm != null ? pm.IncomeMultiplier : 1f;

            return Mathf.RoundToInt(incomePerMinute * minutes * economyScale * prestigeMult);
        }

        private void GatherState()
        {
            // Money
            var mm = Money.MoneyManager.Instance;
            if (mm != null)
                _data.money = mm.TotalMoney;

            // Plots (PlotManager edits _data.plots directly)
            _data.purchasedUpgradeCount = _data.plots.Count;

            // Player position
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector3 pos = player.transform.position;
                _data.playerX = pos.x;
                _data.playerY = pos.y;
                _data.playerZ = pos.z;
            }

            // Restaurant index
            var rm = Core.RestaurantManager.Instance;
            if (rm != null)
                _data.restaurantIndex = rm.RestaurantIndex;

            // Prestige
            var pm = Core.PrestigeManager.Instance;
            if (pm != null)
                _data.prestigeLevel = pm.PrestigeLevel;

            // Gems
            var gm = Core.GemManager.Instance;
            if (gm != null)
                _data.gems = gm.Gems;
        }

        public void DeleteSave()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            _data = new GameData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _initialized)
                SaveGame();
        }

        private void OnApplicationQuit()
        {
            if (_initialized)
                SaveGame();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
