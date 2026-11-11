using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShopIdle.Core
{
    /// <summary>
    /// Manages gems — a premium currency earned from daily quests,
    /// VIP customers, and restaurant completion.
    /// </summary>
    public class GemManager : MonoBehaviour
    {
        private static GemManager _instance;
        public static GemManager Instance => _instance;

        private int _gems;
        private TextMeshProUGUI _gemText;

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

        private void Start()
        {
            CreateGemUI();
        }

        public void AddGems(int amount)
        {
            _gems += amount;
            OnGemsChanged?.Invoke(_gems);
            UpdateUI();

            JuiceManager.Instance?.SpawnFloatingText(
                $"+{amount} gem", Vector3.up * 3f, new Color(0.7f, 0.3f, 1f));
        }

        private void UpdateUI()
        {
            if (_gemText != null)
                _gemText.text = $"{_gems}";
        }

        private void CreateGemUI()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var container = new GameObject("GemUI");
            container.transform.SetParent(canvas.transform, false);
            var rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.6f, 0.97f);
            rect.anchorMax = new Vector2(0.85f, 0.995f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Background
            var bg = container.AddComponent<Image>();
            bg.color = new Color(0.25f, 0.1f, 0.4f, 0.7f);

            // Gem icon (diamond shape text)
            var iconObj = new GameObject("GemIcon");
            iconObj.transform.SetParent(container.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0f);
            iconRect.anchorMax = new Vector2(0.3f, 1f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconTmp = iconObj.AddComponent<TextMeshProUGUI>();
            iconTmp.text = "<color=#BB66FF>◆</color>";
            iconTmp.fontSize = 22;
            iconTmp.alignment = TextAlignmentOptions.Center;

            // Count text
            var textObj = new GameObject("GemCount");
            textObj.transform.SetParent(container.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.3f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            _gemText = textObj.AddComponent<TextMeshProUGUI>();
            _gemText.text = $"{_gems}";
            _gemText.fontSize = 22;
            _gemText.alignment = TextAlignmentOptions.MidlineLeft;
            _gemText.color = Color.white;
            _gemText.fontStyle = FontStyles.Bold;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
