using UnityEngine;
using TMPro;

namespace ShopIdle.Save
{
    public class OfflineEarningsUI : MonoBehaviour
    {
        [SerializeField] private float _showDelay = 0.5f;

        private int _earnings;
        private GameObject _panel;

        private void Start()
        {
            var sm = SaveManager.Instance;
            if (sm == null) return;

            _earnings = sm.CalculateOfflineEarnings();
            if (_earnings > 0)
                Invoke(nameof(ShowPopup), _showDelay);
        }

        private void ShowPopup()
        {
            // Canvas
            var canvasObj = new GameObject("OfflineEarningsCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            _panel = canvasObj;

            // Dim background
            var dimObj = new GameObject("Dim");
            dimObj.transform.SetParent(canvasObj.transform, false);
            var dimRect = dimObj.AddComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.offsetMin = Vector2.zero;
            dimRect.offsetMax = Vector2.zero;
            var dimImg = dimObj.AddComponent<UnityEngine.UI.Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.6f);

            // Panel background
            var bgObj = new GameObject("PanelBG");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.1f, 0.3f);
            bgRect.anchorMax = new Vector2(0.9f, 0.7f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0.12f, 0.12f, 0.2f, 0.95f);

            // Title
            CreateText(canvasObj.transform, "Title",
                new Vector2(0.15f, 0.58f), new Vector2(0.85f, 0.67f),
                "Welcome Back!", 42, new Color(1f, 0.85f, 0.1f));

            // Earnings text
            CreateText(canvasObj.transform, "Earnings",
                new Vector2(0.15f, 0.48f), new Vector2(0.85f, 0.57f),
                $"You earned ${_earnings} while away!", 32, Color.white);

            // Collect button
            CreateButton(canvasObj.transform, "CollectBtn",
                new Vector2(0.15f, 0.33f), new Vector2(0.48f, 0.43f),
                $"Collect ${_earnings}", new Color(0.2f, 0.7f, 0.3f),
                () => Collect(1));

            // Collect x2 button
            CreateButton(canvasObj.transform, "Collect2xBtn",
                new Vector2(0.52f, 0.33f), new Vector2(0.85f, 0.43f),
                $"Collect ${_earnings * 2} (x2)", new Color(0.8f, 0.5f, 0.1f),
                () => Collect(2));
        }

        private void Collect(int multiplier)
        {
            var mm = Money.MoneyManager.Instance;
            if (mm != null)
                mm.AddMoney(_earnings * multiplier);

            if (_panel != null)
                Destroy(_panel);
        }

        private void CreateText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            string text, float fontSize, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
        }

        private void CreateButton(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            string label, Color bgColor, System.Action onClick)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            var btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = bgColor;

            var btn = btnObj.AddComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() => onClick());

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 26;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
        }
    }
}
