using UnityEngine;

namespace ShopIdle.UI
{
    /// <summary>
    /// Procedurally drawn reaction faces and a rounded speech-bubble texture (cached).
    /// Original simple art: round face, outline, eyes and mouth drawn with distance tests.
    /// </summary>
    public static class EmojiTextures
    {
        private const int Size = 128;
        private static Texture2D _happy, _angry, _bubble;

        /// <summary>Beaming face with heart eyes.</summary>
        public static Texture2D Happy => _happy ??= Draw("EmojiHappy", new Color(1f, 0.82f, 0.25f), HappyFeature);

        /// <summary>Red, frowning face for customers who gave up waiting.</summary>
        public static Texture2D Angry => _angry ??= Draw("EmojiAngry", new Color(0.98f, 0.45f, 0.3f), AngryFeature);

        /// <summary>White 2:1 rounded rectangle with a small tail at the bottom centre.</summary>
        public static Texture2D Bubble => _bubble ??= DrawBubble();

        private static Texture2D Draw(string name, Color face, System.Func<float, float, Color?> feature)
        {
            var tex = NewTexture(name);
            var px = new Color32[Size * Size];
            Color outline = new(0.45f, 0.28f, 0.12f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    // Normalised coords: -1..1, +y up.
                    float u = (x + 0.5f) / Size * 2f - 1f, v = (y + 0.5f) / Size * 2f - 1f;
                    float r = Mathf.Sqrt(u * u + v * v);
                    Color c = new(0f, 0f, 0f, 0f);
                    if (r < 0.95f) c = r > 0.87f ? outline : Shade(face, u, v);
                    if (r < 0.87f && feature(u, v) is Color f) c = f;
                    // Soft edge
                    c.a *= Mathf.Clamp01((0.95f - r) * Size * 0.5f);
                    px[y * Size + x] = c;
                }
            return Finish(tex, px);
        }

        private static Color Shade(Color face, float u, float v) =>
            Color.Lerp(face, face * 0.82f, Mathf.Clamp01(-v * 0.6f + 0.3f - u * 0.2f));

        private static Color? HappyFeature(float u, float v)
        {
            Color heart = new(0.92f, 0.2f, 0.3f), mouth = new(0.55f, 0.18f, 0.12f);
            foreach (float ex in new[] { -0.36f, 0.36f })
                if (InHeart((u - ex) / 0.24f, (v - 0.22f) / 0.24f)) return heart;
            // Wide open smile: lower half-disc.
            float mx = u / 0.48f, my = (v + 0.2f) / 0.38f;
            if (my < 0f && mx * mx + my * my < 1f) return my > -0.35f && Mathf.Abs(mx) < 0.75f ? Color.white : mouth;
            return null;
        }

        private static Color? AngryFeature(float u, float v)
        {
            Color ink = new(0.35f, 0.1f, 0.08f);
            foreach (float ex in new[] { -0.32f, 0.32f })
            {
                float dx = u - ex, dy = v - 0.12f;
                if (dx * dx + dy * dy < 0.012f) return ink;                      // eye
                float slope = ex < 0 ? -0.45f : 0.45f;                            // brows slant down to the middle
                if (Mathf.Abs(dx) < 0.2f && Mathf.Abs(v - (0.36f + dx * slope)) < 0.045f) return ink;
            }
            // Frown: upper arc.
            float d = Mathf.Sqrt(u * u + (v + 0.62f) * (v + 0.62f));
            if (Mathf.Abs(d - 0.36f) < 0.05f && v > -0.38f) return ink;
            return null;
        }

        /// <summary>Classic heart curve test in local coords (~unit size).</summary>
        private static bool InHeart(float x, float y)
        {
            y = -y * 1.1f + 0.25f;
            float a = x * x + y * y - 1f;
            return a * a * a - x * x * y * y * y < 0f;
        }

        private static Texture2D DrawBubble()
        {
            const int w = Size * 2, h = Size;
            var tex = NewTexture("SpeechBubble", w, h);
            var px = new Color32[w * h];
            const float radius = 34f, tail = 22f;
            Color fill = Color.white, edge = new(0.78f, 0.78f, 0.82f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float by = y - tail; // body sits above the tail
                    float dx = Mathf.Max(radius - x, 0f, x - (w - 1 - radius));
                    float dy = Mathf.Max(radius - by, 0f, by - (h - 1 - tail - radius));
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    bool body = by >= 0 && d <= radius;
                    bool tailPx = y < tail + 2 && Mathf.Abs(x - w * 0.5f) < (y / tail) * 16f;
                    Color c = new(0f, 0f, 0f, 0f);
                    if (body || tailPx) c = body && d > radius - 4f ? edge : fill;
                    px[y * w + x] = c;
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        private static Texture2D NewTexture(string name, int w = Size, int h = Size) => new(w, h, TextureFormat.RGBA32, false)
        {
            name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
        };

        private static Texture2D Finish(Texture2D tex, Color32[] px)
        {
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }
    }
}
