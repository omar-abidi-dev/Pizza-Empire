using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Ground surfaces: warm wooden planks inside, light tiles in kitchen zones,
    /// textured grass and a road outside. Textures are generated once and cached.
    /// </summary>
    public static class FloorTheme
    {
        // Kitchen zone: the strip behind the counter line (ovens, counter, cashier).
        public const float KitchenZMin = 6.2f;
        public const float KitchenZMax = 9.85f;

        private const float PlankTextureMetres = 2f;  // 8 planks of 0.25 m
        private const float TileTextureMetres = 2f;   // 4 tiles of 0.5 m

        private static Texture2D _wood, _tiles, _grass, _road;

        public enum Surface { Wood, Tiles, Grass, Asphalt }

        private static readonly System.Collections.Generic.Dictionary<Surface, Material> PlotMats = new();
        private static Material _locked, _lane;

        /// <summary>Kitchen tiles along the back of the starting plot (behind the counter line).</summary>
        public static void ApplyMainArea()
        {
            AddKitchenTiles("KitchenTiles", -3.9f, 3.9f);
        }

        /// <summary>Shared material for an 8x8 m plot floor.</summary>
        public static Material PlotMaterial(Surface surface)
        {
            if (PlotMats.TryGetValue(surface, out var m) && m != null) return m;
            const float size = 8f;
            m = surface switch
            {
                Surface.Tiles => TexturedMaterial(TileTexture(), size / TileTextureMetres, size / TileTextureMetres, 0.4f),
                Surface.Grass => TexturedMaterial(GrassTexture(), size / 4f, size / 4f, 0.05f),
                Surface.Asphalt => TexturedMaterial(AsphaltTexture(), 2f, 2f, 0.1f),
                _ => TexturedMaterial(WoodTexture(), size / PlankTextureMetres, size / PlankTextureMetres, 0.25f)
            };
            PlotMats[surface] = m;
            return m;
        }

        /// <summary>Faded see-through planks for plots that are not bought yet.</summary>
        public static Material LockedMaterial()
        {
            if (_locked != null) return _locked;
            _locked = ColorPalette.MakeTransparent(new Color(1f, 0.97f, 0.92f, 0.38f));
            _locked.SetTexture("_BaseMap", WoodTexture());
            _locked.SetTextureScale("_BaseMap", new Vector2(4f, 4f));
            return _locked;
        }

        /// <summary>Road texture for an 8x3 m drive-thru lane (road runs along the quad's X axis).</summary>
        public static Material LaneMaterial()
        {
            if (_lane != null) return _lane;
            _lane = TexturedMaterial(RoadTexture(), 2.6f, 1f, 0.1f);
            return _lane;
        }

        /// <summary>Tile strip over a kitchen zone between <paramref name="xMin"/> and <paramref name="xMax"/>.</summary>
        public static void AddKitchenTiles(string name, float xMin, float xMax)
        {
            if (GameObject.Find(name) != null) return;
            float w = xMax - xMin, d = KitchenZMax - KitchenZMin;
            var quad = CreateGroundQuad(name, new Vector3((xMin + xMax) * 0.5f, 0.01f,
                (KitchenZMin + KitchenZMax) * 0.5f), w, d);
            quad.GetComponent<MeshRenderer>().material =
                TexturedMaterial(TileTexture(), w / TileTextureMetres, d / TileTextureMetres, 0.4f);
        }

        /// <summary>Give the outdoor grass plane a soft two-tone texture.</summary>
        public static void TextureGrass(GameObject grass)
        {
            var r = grass.GetComponent<MeshRenderer>();
            Vector3 size = r.bounds.size;
            r.material = TexturedMaterial(GrassTexture(), size.x / 4f, size.z / 4f, 0.05f);
        }

        // ── Helpers ──

        private static GameObject CreateGroundQuad(string name, Vector3 center, float sizeX, float sizeZ)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetPositionAndRotation(center, Quaternion.Euler(90f, 0f, 0f));
            quad.transform.localScale = new Vector3(sizeX, sizeZ, 1f);
            Object.Destroy(quad.GetComponent<Collider>());
            quad.isStatic = true;
            return quad;
        }

        private static Material TexturedMaterial(Texture2D tex, float tileU, float tileV, float smoothness)
        {
            var mat = ColorPalette.MakeLit(Color.white);
            mat.mainTexture = tex;
            mat.mainTextureScale = new Vector2(tileU, tileV);
            mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        private static Texture2D NewTexture(int w, int h, string name)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, true)
            {
                name = name,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 2
            };
        }

        // ── Procedural textures ──

        private static Texture2D WoodTexture()
        {
            if (_wood != null) return _wood;
            const int size = 256, planks = 8, plankPx = size / planks;
            var rng = new System.Random(1234);
            var px = new Color32[size * size];
            Color[] tones =
            {
                new(0.82f, 0.60f, 0.40f), new(0.76f, 0.54f, 0.35f),
                new(0.86f, 0.66f, 0.45f), new(0.79f, 0.57f, 0.37f)
            };

            for (int p = 0; p < planks; p++)
            {
                // Each plank row is split into boards at a random offset (staggered joints).
                int joint = rng.Next(40, size - 40);
                Color toneA = tones[rng.Next(tones.Length)], toneB = tones[rng.Next(tones.Length)];
                float phase = (float)rng.NextDouble() * 10f;
                for (int y = p * plankPx; y < (p + 1) * plankPx; y++)
                {
                    int local = y - p * plankPx;
                    for (int x = 0; x < size; x++)
                    {
                        Color c = x < joint ? toneA : toneB;
                        float grain = Mathf.Sin(x * 0.09f + local * 0.8f + phase) * 0.025f
                                      + Mathf.Sin(x * 0.31f + phase * 2f) * 0.015f;
                        c *= 1f + grain;
                        bool seam = local < 2 || Mathf.Abs(x - joint) < 1 || x < 1;
                        if (seam) c *= 0.72f;
                        c.a = 1f;
                        px[y * size + x] = c;
                    }
                }
            }
            _wood = Finish(NewTexture(size, size, "WoodPlanks"), px);
            return _wood;
        }

        private static Texture2D TileTexture()
        {
            if (_tiles != null) return _tiles;
            const int size = 128, tiles = 4, tilePx = size / tiles;
            var px = new Color32[size * size];
            Color tileA = new(0.97f, 0.95f, 0.91f), tileB = new(0.93f, 0.92f, 0.89f);
            Color grout = new(0.80f, 0.78f, 0.74f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int tx = x / tilePx, ty = y / tilePx;
                    bool edge = x % tilePx < 2 || y % tilePx < 2;
                    px[y * size + x] = edge ? grout : ((tx + ty) % 2 == 0 ? tileA : tileB);
                }
            _tiles = Finish(NewTexture(size, size, "KitchenTiles"), px);
            return _tiles;
        }

        private static Texture2D GrassTexture()
        {
            if (_grass != null) return _grass;
            const int size = 64;
            var rng = new System.Random(77);
            var px = new Color32[size * size];
            Color a = new(0.60f, 0.86f, 0.64f), b = new(0.53f, 0.80f, 0.57f);
            for (int i = 0; i < px.Length; i++)
                px[i] = Color.Lerp(a, b, (float)rng.NextDouble() * 0.8f);
            _grass = Finish(NewTexture(size, size, "Grass"), px);
            return _grass;
        }

        private static Texture2D RoadTexture()
        {
            if (_road != null) return _road;
            const int size = 64;
            var px = new Color32[size * size];
            Color asphalt = new(0.42f, 0.42f, 0.46f), dash = new(0.97f, 0.95f, 0.85f), kerb = new(0.85f, 0.84f, 0.82f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Color c = asphalt;
                    if (y < 3 || y >= size - 3) c = kerb;                              // kerbs
                    else if (Mathf.Abs(y - size / 2) < 2 && x < size / 2) c = dash;    // centre dash
                    px[y * size + x] = c;
                }
            _road = Finish(NewTexture(size, size, "Road"), px);
            return _road;
        }

        private static Texture2D _asphalt;

        private static Texture2D AsphaltTexture()
        {
            if (_asphalt != null) return _asphalt;
            const int size = 64;
            var rng = new System.Random(5);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = 0.44f + (float)rng.NextDouble() * 0.04f;
                    Color c = new(n, n, n + 0.03f);
                    if (x < 2) c = new Color(0.95f, 0.95f, 0.92f); // parking stall line
                    px[y * size + x] = c;
                }
            _asphalt = Finish(NewTexture(size, size, "Asphalt"), px);
            return _asphalt;
        }

        private static Texture2D Finish(Texture2D tex, Color32[] px)
        {
            tex.SetPixels32(px);
            tex.Apply(true, true);
            return tex;
        }
    }
}
