using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShopIdle.UI
{
    /// <summary>
    /// White rounded card under the header showing the current objective and its progress
    /// (from <see cref="Core.ObjectiveTracker"/>). Pops when the goal changes.
    /// </summary>
    public class ObjectiveCard : MonoBehaviour
    {
        public const float Top = HudTopBar.HeaderHeight + 14f;
        public const float Height = 128f;

        private static readonly Color Ink = new(0.13f, 0.2f, 0.32f);

        private TextMeshProUGUI _title, _count, _icon;
        private Image _fill;
        private RectTransform _card;
        private float _pop;
        private string _lastTitle;

        public static ObjectiveCard Build(Transform canvas)
        {
            var card = UIFactory.Fixed("ObjectiveCard", canvas, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(30f, -Top), new Vector2(600f, Height));
            var c = card.gameObject.AddComponent<ObjectiveCard>();
            c._card = card;
            c.BuildCard(card);
            return c;
        }

        private void BuildCard(RectTransform card)
        {
            UIFactory.Panel(card, new Color(1f, 1f, 1f, 0.97f));

            var badge = UIFactory.Fixed("Icon", card, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(66f, 0f), new Vector2(96f, 96f));
            UIFactory.Image(badge, UISprites.Circle, new Color(0.98f, 0.66f, 0.38f));
            _icon = UIFactory.Text(UIFactory.Rect("Glyph", badge, Vector2.zero, Vector2.one), "!", 56, Color.white,
                TextAlignmentOptions.Center, true);

            var title = UIFactory.Rect("Title", card, new Vector2(0f, 0.5f), Vector2.one, new Vector2(130f, 0f), new Vector2(-20f, -10f));
            _title = UIFactory.Text(title, "", 34, Ink, TextAlignmentOptions.Midline);
            _title.enableAutoSizing = true;
            _title.fontSizeMin = 20;
            _title.fontSizeMax = 34;

            var bar = UIFactory.Rect("Bar", card, new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(140f, 20f), new Vector2(-30f, -8f));
            _fill = UIFactory.Bar(bar, new Color(0.84f, 0.87f, 0.9f), new Color(0.32f, 0.78f, 0.4f));
            _count = UIFactory.Text(UIFactory.Rect("Count", bar, Vector2.zero, Vector2.one), "0/1", 26, Color.white,
                TextAlignmentOptions.Center, true);
        }

        private void Start()
        {
            var ot = Core.ObjectiveTracker.Instance;
            if (ot != null) ot.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (Core.ObjectiveTracker.Instance != null) Core.ObjectiveTracker.Instance.Changed -= Refresh;
        }

        private void Refresh()
        {
            var ot = Core.ObjectiveTracker.Instance;
            if (ot == null) { gameObject.SetActive(false); return; }
            _title.text = ot.Title;
            _count.text = ot.Current == Core.ObjectiveTracker.Kind.Collect
                ? $"${ot.Progress}/${ot.Target}" : $"{ot.Progress}/{ot.Target}";
            _icon.text = ot.Current switch
            {
                Core.ObjectiveTracker.Kind.Serve => "!",
                Core.ObjectiveTracker.Kind.Collect => "$",
                _ => "+"
            };
            UIFactory.SetFill(_fill, (float)ot.Progress / ot.Target);
            if (_lastTitle != null && _lastTitle != ot.Title) _pop = 0.3f;
            _lastTitle = ot.Title;
        }

        private void Update()
        {
            if (_pop <= 0f) return;
            _pop -= Time.unscaledDeltaTime;
            _card.localScale = Vector3.one * (1f + Mathf.Sin(Mathf.Clamp01(_pop / 0.3f) * Mathf.PI) * 0.08f);
        }
    }
}
