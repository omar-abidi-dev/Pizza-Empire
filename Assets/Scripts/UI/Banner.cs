using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShopIdle.UI
{
    /// <summary>Big centred announcement that pops in, holds and fades out (one at a time).</summary>
    public class Banner : MonoBehaviour
    {
        private const float PopTime = 0.2f, HoldTime = 2f, FadeTime = 0.5f;
        private static Banner _current;

        private Image _bg;
        private TextMeshProUGUI _text;
        private float _t;

        public static void Show(string text)
        {
            if (_current != null) Destroy(_current.gameObject);
            var go = new GameObject("BannerCanvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            _current = go.AddComponent<Banner>();
            _current.Build(text);
        }

        private void Build(string text)
        {
            _bg = Stretch(new GameObject("BannerBG")).AddComponent<Image>();
            _bg.color = new Color(0.08f, 0.08f, 0.15f, 0.85f);
            _text = Stretch(new GameObject("BannerText")).AddComponent<TextMeshProUGUI>();
            _text.text = text;
            _text.fontSize = 52;
            _text.enableAutoSizing = true;
            _text.fontSizeMin = 24;
            _text.fontSizeMax = 52;
            _text.alignment = TextAlignmentOptions.Center;
            _text.color = new Color(1f, 0.85f, 0.1f);
            _text.fontStyle = FontStyles.Bold;
        }

        private GameObject Stretch(GameObject go)
        {
            go.transform.SetParent(transform, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0.05f, 0.6f);
            r.anchorMax = new Vector2(0.95f, 0.7f);
            r.offsetMin = r.offsetMax = Vector2.zero;
            return go;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            float s = Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(_t / PopTime));
            _bg.transform.localScale = _text.transform.localScale = Vector3.one * s;

            float a = 1f - Mathf.Clamp01((_t - PopTime - HoldTime) / FadeTime);
            _bg.color = new Color(0.08f, 0.08f, 0.15f, 0.85f * a);
            _text.color = new Color(1f, 0.85f, 0.1f, a);
            if (a <= 0f) Destroy(gameObject);
        }

        private void OnDestroy() { if (_current == this) _current = null; }
    }
}
