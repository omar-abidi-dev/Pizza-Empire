using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShopIdle.UI
{
    /// <summary>Small helpers for building HUD pieces in code (rects, rounded panels, text, bars).</summary>
    public static class UIFactory
    {
        /// <summary>The scene's gameplay canvas (not the title screen or popups).</summary>
        public static Canvas MainCanvas()
        {
            var go = GameObject.Find("GameCanvas");
            return go != null ? go.GetComponent<Canvas>() : Object.FindAnyObjectByType<Canvas>();
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = offsetMin;
            r.offsetMax = offsetMax;
            return r;
        }

        /// <summary>Rect anchored at a point with a fixed pixel size.</summary>
        public static RectTransform Fixed(string name, Transform parent, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size)
        {
            var r = Rect(name, parent, anchor, anchor);
            r.pivot = pivot;
            r.anchoredPosition = position;
            r.sizeDelta = size;
            return r;
        }

        public static Image Image(RectTransform r, Sprite sprite, Color color, bool sliced = false)
        {
            var img = r.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = sliced ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Rounded panel with a soft drop shadow underneath.</summary>
        public static Image Panel(RectTransform r, Color color, float shadow = 6f)
        {
            if (shadow > 0f)
            {
                var s = Rect("Shadow", r, Vector2.zero, Vector2.one, new Vector2(0f, -shadow), new Vector2(0f, -shadow));
                Image(s, UISprites.Rounded, new Color(0f, 0f, 0f, 0.22f), true);
                s.SetAsFirstSibling();
            }
            var body = Rect("Body", r, Vector2.zero, Vector2.one);
            return Image(body, UISprites.Rounded, color, true);
        }

        public static TextMeshProUGUI Text(RectTransform r, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, bool outline = false)
        {
            var t = r.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = FontStyles.Bold;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            if (outline)
            {
                t.outlineWidth = 0.22f;
                t.outlineColor = new Color32(30, 40, 60, 255);
            }
            return t;
        }

        /// <summary>Rounded progress bar; returns the fill image (Filled, horizontal).</summary>
        public static Image Bar(RectTransform r, Color back, Color fill)
        {
            Image(r, UISprites.Rounded, back, true);
            var f = Rect("Fill", r, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            var img = Image(f, UISprites.Rounded, fill, true);
            return img;
        }

        /// <summary>Set a sliced bar fill to a 0..1 fraction by shrinking its right edge.</summary>
        public static void SetFill(Image fill, float t)
        {
            var r = fill.rectTransform;
            r.anchorMax = new Vector2(Mathf.Clamp01(t), 1f);
            fill.enabled = t > 0.001f;
        }
    }
}
