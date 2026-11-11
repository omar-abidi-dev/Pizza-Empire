using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShopIdle.UI
{
    /// <summary>
    /// Top header: level star with an XP bar on the left, cash and gem pills on the right.
    /// Values tick up smoothly and pop when they change.
    /// </summary>
    public class HudTopBar : MonoBehaviour
    {
        public const float HeaderHeight = 150f;

        private static readonly Color HeaderColor = new(0.12f, 0.52f, 0.58f, 0.96f);
        private static readonly Color PillColor = new(0.08f, 0.2f, 0.3f, 0.75f);
        private static readonly Color XpFill = new(0.5f, 0.88f, 0.32f);

        private TextMeshProUGUI _level, _xp, _cash, _gems;
        private Image _xpFill;
        private RectTransform _cashPill;
        private int _shownCash, _targetCash;
        private float _cashPop;

        public static HudTopBar Build(Transform canvas)
        {
            var root = UIFactory.Rect("HudTopBar", canvas, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -HeaderHeight), Vector2.zero);
            var bar = root.gameObject.AddComponent<HudTopBar>();
            bar.BuildHeader(root);
            return bar;
        }

        private void BuildHeader(RectTransform root)
        {
            UIFactory.Image(root, null, HeaderColor);
            var stripe = UIFactory.Rect("Stripe", root, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 8f));
            UIFactory.Image(stripe, null, new Color(0.08f, 0.38f, 0.44f));

            // XP bar first so the star overlaps its left end.
            var xp = UIFactory.Fixed("XpBar", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(95f, 0f), new Vector2(330f, 50f));
            _xpFill = UIFactory.Bar(xp, new Color(0.06f, 0.18f, 0.26f, 0.8f), XpFill);
            var xpText = UIFactory.Rect("XpText", xp, Vector2.zero, Vector2.one, new Vector2(40f, 0f), Vector2.zero);
            _xp = UIFactory.Text(xpText, "0/10", 30, Color.white, TextAlignmentOptions.Center, true);

            var star = UIFactory.Fixed("LevelStar", root, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(85f, 0f), new Vector2(124f, 124f));
            UIFactory.Image(star, UISprites.Star, new Color(1f, 0.8f, 0.18f));
            var lvl = UIFactory.Rect("Level", star, Vector2.zero, Vector2.one, new Vector2(0f, -4f), Vector2.zero);
            _level = UIFactory.Text(lvl, "1", 46, Color.white, TextAlignmentOptions.Center, true);

            _gems = Pill(root, "GemPill", -24f, 190f, null, out var gemPill);
            var gemIcon = UIFactory.Fixed("GemIcon", gemPill, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(66f, 66f));
            GemSprite.AddTo(gemIcon.gameObject);
            _cash = Pill(root, "CashPill", -262f, 240f, UISprites.Bill, out _cashPill);
        }

        /// <summary>Dark rounded pill anchored to the right, with an icon hanging over its left edge.</summary>
        private static TextMeshProUGUI Pill(RectTransform root, string name, float x, float width, Sprite icon,
            out RectTransform pill)
        {
            pill = UIFactory.Fixed(name, root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(x, 0f), new Vector2(width, 64f));
            UIFactory.Panel(pill, PillColor, 0f);
            if (icon != null)
            {
                var i = UIFactory.Fixed("Icon", pill, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76f, 52f));
                UIFactory.Image(i, icon, Color.white).transform.localRotation = Quaternion.Euler(0f, 0f, 10f);
            }
            var t = UIFactory.Rect("Amount", pill, Vector2.zero, Vector2.one, new Vector2(44f, 0f), new Vector2(-14f, 0f));
            var text = UIFactory.Text(t, "0", 36, Color.white, TextAlignmentOptions.MidlineRight, true);
            return text;
        }

        private void Start()
        {
            var mm = Money.MoneyManager.Instance;
            if (mm != null)
            {
                mm.OnMoneyChanged += OnMoney;
                _shownCash = _targetCash = mm.TotalMoney;
                _cash.text = Format(_shownCash);
            }
            var gm = Core.GemManager.Instance;
            if (gm != null)
            {
                gm.OnGemsChanged += OnGems;
                OnGems(gm.Gems);
            }
            var lm = Core.LevelManager.Instance;
            if (lm != null) lm.Changed += OnLevel;
            OnLevel();
        }

        private void OnDestroy()
        {
            if (Money.MoneyManager.Instance != null) Money.MoneyManager.Instance.OnMoneyChanged -= OnMoney;
            if (Core.GemManager.Instance != null) Core.GemManager.Instance.OnGemsChanged -= OnGems;
            if (Core.LevelManager.Instance != null) Core.LevelManager.Instance.Changed -= OnLevel;
        }

        private void OnMoney(int total)
        {
            if (total > _targetCash) _cashPop = 0.2f;
            _targetCash = total;
        }

        private void OnGems(int gems) => _gems.text = gems.ToString();

        private void OnLevel()
        {
            var lm = Core.LevelManager.Instance;
            if (lm == null) return;
            _level.text = lm.Level.ToString();
            _xp.text = $"{lm.Xp}/{lm.XpToNext}";
            UIFactory.SetFill(_xpFill, (float)lm.Xp / lm.XpToNext);
        }

        private void Update()
        {
            if (_shownCash != _targetCash)
            {
                int step = Mathf.Max(1, Mathf.Abs(_targetCash - _shownCash) / 6);
                _shownCash += _targetCash > _shownCash ? step : -step;
                if (Mathf.Abs(_targetCash - _shownCash) < step) _shownCash = _targetCash;
                _cash.text = Format(_shownCash);
            }
            if (_cashPop > 0f)
            {
                _cashPop -= Time.unscaledDeltaTime;
                _cashPill.localScale = Vector3.one * (1f + Mathf.Sin(Mathf.Clamp01(_cashPop / 0.2f) * Mathf.PI) * 0.12f);
            }
        }

        private static string Format(int amount)
        {
            if (amount >= 1000000) return $"{amount / 1000000f:0.#}M";
            if (amount >= 10000) return $"{amount / 1000f:0.#}K";
            return amount.ToString();
        }
    }
}
