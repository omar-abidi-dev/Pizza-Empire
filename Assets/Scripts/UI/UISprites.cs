using UnityEngine;

namespace ShopIdle.UI
{
    /// <summary>
    /// Procedural UI sprites (cached): a 9-sliced rounded rectangle, a soft circle,
    /// a chunky star and a cash bill icon. All white so Image.color tints them.
    /// </summary>
    public static class UISprites
    {
        private static Sprite _rounded, _circle, _star, _bill;

        /// <summary>Rounded rectangle, 9-sliced so any size keeps round corners.</summary>
        public static Sprite Rounded => _rounded ??= MakeRounded();
        public static Sprite Circle => _circle ??= Make(128, "Circle", (u, v) => 1f - Edge(Mathf.Sqrt(u * u + v * v), 0.97f));
        public static Sprite Star => _star ??= Make(128, "Star", StarAlpha);

        /// <summary>Green bill with a darker centre disc (coloured, not tinted).</summary>
        public static Sprite Bill => _bill ??= MakeBill();

        private static Sprite MakeRounded()
        {
            const int size = 64, r = 24;
            var tex = NewTex(size, size, "Rounded");
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(r - x - 0.5f, 0f, x + 0.5f - (size - r));
                    float dy = Mathf.Max(r - y - 0.5f, 0f, y + 0.5f - (size - r));
                    float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        }

        private static Sprite Make(int size, string name, System.Func<float, float, float> alpha)
        {
            var tex = NewTex(size, size, name);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
                    px[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha(u, v)));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>Five-point star with rounded-ish points (polar radius test).</summary>
        private static float StarAlpha(float u, float v)
        {
            float r = Mathf.Sqrt(u * u + v * v);
            float a = Mathf.Atan2(u, v); // 0 at the top point
            float k = Mathf.Cos(a * 5f) * 0.5f + 0.5f;           // 1 on points, 0 between
            float edge = Mathf.Lerp(0.5f, 0.96f, Mathf.Pow(k, 1.6f));
            return 1f - Edge(r, edge);
        }

        private static Sprite MakeBill()
        {
            const int w = 96, h = 64;
            var tex = NewTex(w, h, "Bill");
            var px = new Color32[w * h];
            Color light = new(0.42f, 0.86f, 0.42f), dark = new(0.22f, 0.62f, 0.27f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = Mathf.Max(10 - x, 0f, x - (w - 11)), dy = Mathf.Max(10 - y, 0f, y - (h - 11));
                    float a = Mathf.Clamp01(10f - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                    float cx = (x - w / 2f) / 16f, cy = (y - h / 2f) / 16f;
                    bool disc = cx * cx + cy * cy < 1f;
                    bool border = x < 6 || x > w - 7 || y < 6 || y > h - 7;
                    Color c = disc || border ? dark : light;
                    c.a = a;
                    px[y * w + x] = c;
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>Anti-aliased step: 0 inside <paramref name="limit"/>, 1 outside.</summary>
        private static float Edge(float r, float limit) => Mathf.Clamp01((r - limit) * 64f + 0.5f);

        private static Texture2D NewTex(int w, int h, string name) => new(w, h, TextureFormat.RGBA32, false)
        {
            name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
        };
    }
}
