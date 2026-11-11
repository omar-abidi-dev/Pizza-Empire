using UnityEngine;
using UnityEditor;
using System.IO;

namespace ShopIdleEditor
{
    /// <summary>
    /// Generates a 1024x1024 app icon: mascot holding pizza on teal background.
    /// Menu: Tools > Pizza Empire > Generate App Icon
    /// </summary>
    public static class AppIconGenerator
    {
        [MenuItem("Tools/Pizza Empire/Generate App Icon")]
        public static void GenerateIcon()
        {
            int size = 1024;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            // Fill background with teal gradient
            Color tealTop = new Color(0.20f, 0.72f, 0.68f);
            Color tealBot = new Color(0.28f, 0.82f, 0.78f);
            for (int y = 0; y < size; y++)
            {
                float t = y / (float)size;
                Color bg = Color.Lerp(tealBot, tealTop, t);
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, bg);
            }

            // Soft glow circle behind mascot
            DrawFilledCircle(tex, 512, 420, 320, new Color(1f, 1f, 1f, 0.12f));

            // --- Mascot ---
            Color skin = new Color(0.96f, 0.84f, 0.72f);
            Color capTeal = new Color(0.15f, 0.60f, 0.58f);
            Color apron = new Color(0.96f, 0.96f, 0.92f);
            Color eyeColor = new Color(0.15f, 0.12f, 0.18f);
            Color crustColor = new Color(0.92f, 0.78f, 0.45f);
            Color sauceColor = new Color(0.9f, 0.3f, 0.2f);
            Color cheeseColor = new Color(1f, 0.92f, 0.55f);

            // Body (rounded rect)
            DrawRoundedRect(tex, 360, 100, 304, 380, 60, skin);

            // Apron (front)
            DrawRoundedRect(tex, 390, 120, 244, 300, 30, apron);

            // Head (circle)
            DrawFilledCircle(tex, 512, 530, 135, skin);

            // Cap (rounded rect on top of head)
            DrawRoundedRect(tex, 340, 620, 344, 90, 30, capTeal);

            // Cap brim
            DrawRoundedRect(tex, 320, 600, 384, 35, 15, capTeal);

            // Eyes
            DrawRoundedRect(tex, 440, 520, 45, 55, 12, eyeColor);
            DrawRoundedRect(tex, 540, 520, 45, 55, 12, eyeColor);

            // Eye highlights (white dots)
            DrawFilledCircle(tex, 455, 545, 8, Color.white);
            DrawFilledCircle(tex, 555, 545, 8, Color.white);

            // Smile
            DrawRoundedRect(tex, 470, 470, 84, 20, 10, new Color(0.85f, 0.4f, 0.35f));

            // Left arm
            DrawRoundedRect(tex, 250, 200, 110, 230, 40, skin);

            // Right arm (raised, holding pizza)
            DrawRoundedRect(tex, 664, 320, 110, 260, 40, skin);

            // --- Pizza stack on right hand ---
            // Pizza 1 (bottom)
            DrawRoundedRect(tex, 620, 560, 200, 50, 20, crustColor);
            DrawRoundedRect(tex, 640, 570, 160, 30, 12, sauceColor);
            DrawRoundedRect(tex, 650, 575, 140, 20, 10, cheeseColor);

            // Pizza 2 (top)
            DrawRoundedRect(tex, 630, 610, 190, 48, 18, crustColor);
            DrawRoundedRect(tex, 648, 618, 154, 30, 12, sauceColor);
            DrawRoundedRect(tex, 658, 623, 134, 20, 10, cheeseColor);

            // Pizza 3 (highest)
            DrawRoundedRect(tex, 640, 658, 180, 46, 16, crustColor);
            DrawRoundedRect(tex, 656, 665, 148, 28, 12, sauceColor);
            DrawRoundedRect(tex, 666, 670, 128, 18, 8, cheeseColor);

            // Topping dots on top pizza
            Color pepperoni = new Color(0.85f, 0.25f, 0.15f);
            DrawFilledCircle(tex, 700, 682, 8, pepperoni);
            DrawFilledCircle(tex, 740, 680, 7, pepperoni);
            DrawFilledCircle(tex, 770, 683, 8, pepperoni);

            // --- Title text at bottom ---
            // Simple "PE" monogram badge
            DrawFilledCircle(tex, 512, 80, 60, new Color(1f, 1f, 1f, 0.3f));

            tex.Apply();

            // Save to Assets/Branding/AppIcon.png
            string dir = Path.Combine(Application.dataPath, "Branding");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, "AppIcon.png");
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.Refresh();
            Debug.Log($"[AppIconGenerator] App icon saved to: Assets/Branding/AppIcon.png");

            // Set as app icon for iOS and Android
            string assetPath = "Assets/Branding/AppIcon.png";
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.isReadable = true;
                importer.maxTextureSize = 1024;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            // Load and assign as default icon
            var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (iconTex != null)
            {
                var icons = new Texture2D[] { iconTex };
                // Default icons
                #pragma warning disable CS0618
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, icons);
                #pragma warning restore CS0618
                Debug.Log("[AppIconGenerator] Set as default app icon.");
            }
        }

        private static void DrawFilledCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            int r2 = radius * radius;
            float feather = 2f;
            for (int y = cy - radius - 2; y <= cy + radius + 2; y++)
            {
                if (y < 0 || y >= tex.height) continue;
                for (int x = cx - radius - 2; x <= cx + radius + 2; x++)
                {
                    if (x < 0 || x >= tex.width) continue;
                    float dist2 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                    float dist = Mathf.Sqrt(dist2);
                    if (dist <= radius + feather)
                    {
                        float a = Mathf.Clamp01((radius + feather - dist) / feather) * color.a;
                        Color existing = tex.GetPixel(x, y);
                        Color blended = Color.Lerp(existing, new Color(color.r, color.g, color.b, 1f), a);
                        tex.SetPixel(x, y, blended);
                    }
                }
            }
        }

        private static void DrawRoundedRect(Texture2D tex, int left, int bottom, int width, int height,
            int cornerRadius, Color color)
        {
            int right = left + width;
            int top = bottom + height;
            float feather = 1.5f;

            for (int y = bottom - 2; y <= top + 2; y++)
            {
                if (y < 0 || y >= tex.height) continue;
                for (int x = left - 2; x <= right + 2; x++)
                {
                    if (x < 0 || x >= tex.width) continue;

                    float dist = SdfRoundedRect(x, y, left, bottom, width, height, cornerRadius);

                    if (dist <= feather)
                    {
                        float a = Mathf.Clamp01((feather - dist) / feather) * color.a;
                        Color existing = tex.GetPixel(x, y);
                        Color blended = Color.Lerp(existing, new Color(color.r, color.g, color.b, 1f), a);
                        tex.SetPixel(x, y, blended);
                    }
                }
            }
        }

        private static float SdfRoundedRect(int px, int py, int left, int bottom, int width, int height, int r)
        {
            float cx = left + width * 0.5f;
            float cy = bottom + height * 0.5f;
            float hw = width * 0.5f - r;
            float hh = height * 0.5f - r;
            float dx = Mathf.Max(Mathf.Abs(px - cx) - hw, 0f);
            float dy = Mathf.Max(Mathf.Abs(py - cy) - hh, 0f);
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }
    }
}
