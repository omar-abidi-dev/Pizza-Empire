using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Manages gems — a premium currency earned from daily quests, objectives
    /// and level-ups. Shown by the HUD header (UI.HudTopBar).
    /// </summary>
    public class GemManager : MonoBehaviour
    {
        private static GemManager _instance;
        public static GemManager Instance => _instance;

        private int _gems;

        public int Gems => _gems;
        public event System.Action<int> OnGemsChanged;

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
                _gems = sm.Data.gems;
        }


        public void AddGems(int amount)
        {
            _gems += amount;
            OnGemsChanged?.Invoke(_gems);

            JuiceManager.Instance?.SpawnFloatingText(
                $"+{amount} gem", Vector3.up * 3f, new Color(0.7f, 0.3f, 1f));
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
