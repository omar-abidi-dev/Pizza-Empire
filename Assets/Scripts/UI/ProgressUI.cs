using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShopIdle.UI
{
    /// <summary>
    /// Restaurant progress bar displayed below the money counter.
    /// Shows restaurant name, upgrade completion, and prestige info.
    /// </summary>
    public class ProgressUI : MonoBehaviour
    {
        private Image _fillBar;
        private TextMeshProUGUI _progressText;
        private TextMeshProUGUI _nameText;
        private int _total;

        public void Init(string restaurantName, int totalUpgrades)
        {
            _total = totalUpgrades;
            BuildUI(restaurantName);
            UpdateProgress(0);
        }

        public void UpdateProgress(int completed)
        {
            float fill = _total > 0 ? (float)completed / _total : 0f;
            if (_fillBar != null)
                _fillBar.fillAmount = fill;
            if (_progressText != null)
                _progressText.text = $"{completed}/{_total}";
        }

        private void BuildUI(string restaurantName)
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Container
            var container = new GameObject("ProgressUI");
            container.transform.SetParent(canvas.transform, false);
            var containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.15f, 0.935f);
            containerRect.anchorMax = new Vector2(0.85f, 0.955f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Background bar
            var bgObj = new GameObject("BarBG");
            bgObj.transform.SetParent(container.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

            // Fill bar
            var fillObj = new GameObject("BarFill");
            fillObj.transform.SetParent(container.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            _fillBar = fillObj.AddComponent<Image>();
            _fillBar.color = new Color(0.4f, 0.8f, 0.3f, 0.9f);
            _fillBar.type = Image.Type.Filled;
            _fillBar.fillMethod = Image.FillMethod.Horizontal;
            _fillBar.fillAmount = 0f;

            // Progress text (right side of bar)
            var textObj = new GameObject("ProgressText");
            textObj.transform.SetParent(container.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            _progressText = textObj.AddComponent<TextMeshProUGUI>();
            _progressText.text = "0/14";
            _progressText.fontSize = 18;
            _progressText.alignment = TextAlignmentOptions.Center;
            _progressText.color = Color.white;
            _progressText.fontStyle = FontStyles.Bold;
            _progressText.outlineWidth = 0.2f;
            _progressText.outlineColor = new Color32(0, 0, 0, 200);

            // Restaurant name + prestige info (above the bar)
            var pm = Core.PrestigeManager.Instance;
            string displayName = restaurantName;
            if (pm != null && pm.PrestigeLevel > 0)
                displayName += $"  <color=#FFD633>City {pm.PrestigeLevel + 1} x{pm.IncomeMultiplier:F1}</color>";

            var nameObj = new GameObject("RestaurantName");
            nameObj.transform.SetParent(canvas.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.15f, 0.955f);
            nameRect.anchorMax = new Vector2(0.85f, 0.975f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            _nameText = nameObj.AddComponent<TextMeshProUGUI>();
            _nameText.text = displayName;
            _nameText.fontSize = 20;
            _nameText.alignment = TextAlignmentOptions.Center;
            _nameText.color = new Color(1f, 0.9f, 0.5f);
            _nameText.fontStyle = FontStyles.Bold;
            _nameText.richText = true;
        }
    }
}
