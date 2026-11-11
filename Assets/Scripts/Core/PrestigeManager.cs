using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Manages prestige ("New City") system.
    /// After restaurant 4, player can prestige: reset to restaurant 0
    /// with a permanent income multiplier (+50% per prestige).
    /// </summary>
    public class PrestigeManager : MonoBehaviour
    {
        private static PrestigeManager _instance;
        public static PrestigeManager Instance => _instance;

        private int _prestigeLevel;

        public int PrestigeLevel => _prestigeLevel;

        /// <summary>Income multiplier: 1.0 + 0.5 * prestige</summary>
        public float IncomeMultiplier => 1f + 0.5f * _prestigeLevel;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            var sm = Save.SaveManager.Instance;
            if (sm != null && sm.Data != null)
                _prestigeLevel = sm.Data.prestigeLevel;
        }

        public bool CanPrestige()
        {
            var rm = RestaurantManager.Instance;
            return rm != null && rm.RestaurantIndex >= 4;
        }

        public void DoPrestige()
        {
            if (!CanPrestige()) return;

            _prestigeLevel++;

            var sm = Save.SaveManager.Instance;
            if (sm != null)
            {
                sm.Data.prestigeLevel = _prestigeLevel;
                sm.Data.restaurantIndex = 0;
                sm.Data.restaurantsCompleted = 0;
                sm.Data.purchasedUpgradeCount = 0;
                sm.Data.activeZonePartialSpent = 0;
                sm.Data.money = 0;
                sm.Data.playerX = 0f;
                sm.Data.playerY = 0.5f;
                sm.Data.playerZ = 0f;
                sm.SaveGame();
            }

            AudioManager.Instance?.Play(SoundType.Fanfare);
            JuiceManager.Instance?.ShakeCamera(0.5f);

            var rm = RestaurantManager.Instance;
            if (rm != null)
                rm.PrestigeReset();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
