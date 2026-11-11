using UnityEngine;
using UnityEngine.UI;

namespace ShopIdle.UI
{
    /// <summary>
    /// Procedurally generated, cached gem icon sprite (faceted purple diamond).
    /// Avoids depending on a font glyph that LiberationSans SDF doesn't contain.
    /// </summary>
    public static class GemSprite
    {
        private const int Size = 64;
        private static Sprite _sprite;

        private static readonly Color Light = new Color(0.86f, 0.62f, 1f);
        private static readonly Color Mid = new Color(0.73f, 0.40f, 1f);
        private static readonly Color Dark = new Color(0.50f, 0.22f, 0.80f);
        private static readonly Color Shine = new Color(1f, 1f, 1f);

        public static Sprite Get()
        {
            if (_sprite != null) return _sprite;

            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = "GemSpriteTex",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[Size * Size];
            float half = Size * 0.5f;
            float radius = half - 2f;

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    // Normalized coords centered on the texture, y up.
                    float px = (x + 0.5f - half) / radius;
                    float py = (y + 0.5f - half) / radius;
                    float d = Mathf.Abs(px) + Mathf.Abs(py); // diamond distance

                    // 1-pixel anti-aliased edge.
                    float alpha = Mathf.Clamp01((1f - d) * radius * 0.7f);
                    if (alpha <= 0f) { pixels[y * Size + x] = new Color32(0, 0, 0, 0); continue; }

                    Color c;
                    if (py > 0f) c = px < 0f ? Light : Mid;   // top facets
                    else c = px < 0f ? Mid : Dark;            // bottom facets

                    // Inner table facet + small highlight for a cut-gem look.
                    if (d < 0.45f) c = Color.Lerp(c, Light, 0.5f);
                    float sx = px + 0.3f, sy = py - 0.3f;
                    if (sx * sx + sy * sy < 0.012f) c = Shine;

                    c.a = alpha;
                    pixels[y * Size + x] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            _sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
            _sprite.name = "GemSprite";
            return _sprite;
        }

        /// <summary>Adds a gem Image to the given RectTransform object.</summary>
        public static Image AddTo(GameObject go)
        {
            var img = go.AddComponent<Image>();
            img.sprite = Get();
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }
    }
}
