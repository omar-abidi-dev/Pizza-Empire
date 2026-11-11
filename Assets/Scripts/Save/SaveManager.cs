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
            // Delay initialization flag to next frame so all managers finish Start()
            StartCoroutine(SetInitializedNextFrame());
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

            // Base rate: each worker type adds income per minute
            int incomePerMinute = 0;
            int upgrades = _data.purchasedUpgradeCount;

            // Worker-based income (check which upgrades are purchased)
            // Index 3 = HireCashier, 4 = HireChef, 9 = HireCleaner, 13 = HireDTWorker
            if (upgrades > 3) incomePerMinute += 5;   // Cashier
            if (upgrades > 4) incomePerMinute += 8;   // Chef
            if (upgrades > 9) incomePerMinute += 4;   // Cleaner
            if (upgrades > 13) incomePerMinute += 10;  // DT Worker

            // Bonus for oven upgrades
            if (upgrades > 0) incomePerMinute += 2;    // Second oven
            if (upgrades > 12) incomePerMinute += 3;   // Third oven

            if (incomePerMinute <= 0) return 0;

            // Scale by restaurant economy (higher restaurants earn more)
            var rm = Core.RestaurantManager.Instance;
            float economyScale = rm != null && rm.CurrentConfig != null
                ? rm.CurrentConfig.moneyPerItem / 10f
                : 1f;

            return Mathf.RoundToInt(incomePerMinute * minutes * economyScale);
        }

        private void GatherState()
        {
            // Money
            var mm = Money.MoneyManager.Instance;
            if (mm != null)
                _data.money = mm.TotalMoney;

            // Upgrades
            var um = FindAnyObjectByType<Upgrades.UpgradeManager>();
            if (um != null)
            {
                _data.purchasedUpgradeCount = um.PurchasedCount;
                _data.activeZonePartialSpent = um.ActiveZoneSpent;
            }

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
