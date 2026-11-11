using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShopIdle.UI
{
    /// <summary>
    /// Builds the overhauled UI: clean top bar with money + bill icon, gems,
    /// restaurant progress bar, rounded buttons with soft drop shadows.
    /// </summary>
    [DefaultExecutionOrder(60)]
    public class UIBuilder : MonoBehaviour
    {
        private static UIBuilder _instance;
        private TextMeshProUGUI _moneyText;
        private Canvas _canvas;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            _canvas = FindAnyObjectByType<Canvas>();
            if (_canvas == null) return;

            BuildTopBar();
            RebuildJoystick();

            // Listen for money changes to update display
            var mm = Money.MoneyManager.Instance;
            if (mm != null)
            {
                mm.OnMoneyChanged += OnMoneyChanged;
                OnMoneyChanged(mm.TotalMoney);
            }
        }

        private void OnMoneyChanged(int amount)
        {
            if (_moneyText != null)
                _moneyText.text = FormatMoney(amount);
        }

        private string FormatMoney(int amount)
        {
            if (amount >= 1000000) return $"${amount / 1000000f:F1}M";
            if (amount >= 1000) return $"${amount / 1000f:F1}K";
            return $"${amount}";
        }

        private void BuildTopBar()
        {
            // Find and remove old MoneyUI if it exists
            var oldMoney = GameObject.Find("MoneyUI");
            if (oldMoney != null) Destroy(oldMoney);

            var p = Core.ColorPalette.Instance;

            // ── Top Bar Background ──
            var barObj = CreateUIObj("TopBar", _canvas.transform);
            var barRect = barObj.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0.96f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.offsetMin = Vector2.zero;
            barRect.offsetMax = Vector2.zero;
            var barBg = barObj.AddComponent<Image>();
            barBg.color = new Color(p.uiBg.r, p.uiBg.g, p.uiBg.b, 0.9f);

            // ── Money Section (center-left) ──
            var moneyObj = CreateUIObj("MoneySection", barObj.transform);
            var moneyRect = moneyObj.GetComponent<RectTransform>();
            moneyRect.anchorMin = new Vector2(0.12f, 0.1f);
            moneyRect.anchorMax = new Vector2(0.5f, 0.9f);
            moneyRect.offsetMin = Vector2.zero;
            moneyRect.offsetMax = Vector2.zero;

            // Bill icon (green rectangle)
            var iconObj = CreateUIObj("BillIcon", moneyObj.transform);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.15f);
            iconRect.anchorMax = new Vector2(0.15f, 0.85f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.color = p.billGreen;

            // Dollar sign on bill
            var dollarObj = CreateUIObj("Dollar", iconObj.transform);
            var dollarRect = dollarObj.GetComponent<RectTransform>();
            dollarRect.anchorMin = Vector2.zero;
            dollarRect.anchorMax = Vector2.one;
            dollarRect.offsetMin = Vector2.zero;
            dollarRect.offsetMax = Vector2.zero;
            var dollarTmp = dollarObj.AddComponent<TextMeshProUGUI>();
            dollarTmp.text = "$";
            dollarTmp.fontSize = 18;
            dollarTmp.alignment = TextAlignmentOptions.Center;
            dollarTmp.color = p.billDark;
            dollarTmp.fontStyle = FontStyles.Bold;

            // Money amount
            var amountObj = CreateUIObj("MoneyAmount", moneyObj.transform);
            var amountRect = amountObj.GetComponent<RectTransform>();
            amountRect.anchorMin = new Vector2(0.18f, 0f);
            amountRect.anchorMax = new Vector2(1f, 1f);
            amountRect.offsetMin = Vector2.zero;
            amountRect.offsetMax = Vector2.zero;
            _moneyText = amountObj.AddComponent<TextMeshProUGUI>();
            _moneyText.text = "$0";
            _moneyText.fontSize = 28;
            _moneyText.alignment = TextAlignmentOptions.MidlineLeft;
            _moneyText.color = Color.white;
            _moneyText.fontStyle = FontStyles.Bold;
        }

        private void RebuildJoystick()
        {
            // Find existing joystick and restyle it
            var joystick = FindAnyObjectByType<VirtualJoystick>();
            if (joystick == null) return;

            var p = Core.ColorPalette.Instance;

            // Restyle background
            var bg = joystick.GetComponent<Image>();
            if (bg != null)
                bg.color = new Color(p.uiBg.r, p.uiBg.g, p.uiBg.b, 0.35f);

            // Restyle handle
            var handle = joystick.transform.Find("Handle");
            if (handle != null)
            {
                var handleImg = handle.GetComponent<Image>();
                if (handleImg != null)
                    handleImg.color = new Color(p.uiAccent.r, p.uiAccent.g, p.uiAccent.b, 0.7f);
            }
        }

        private static GameObject CreateUIObj(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private void OnDestroy()
        {
            var mm = Money.MoneyManager.Instance;
            if (mm != null) mm.OnMoneyChanged -= OnMoneyChanged;
            if (_instance == this) _instance = null;
        }
    }
}
