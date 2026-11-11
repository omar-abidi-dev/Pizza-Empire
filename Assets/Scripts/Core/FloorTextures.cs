using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Procedural, cached floor textures in a bright, flat toy style:
    /// kitchen tiles with small diamond inlays, a warm dining floor, grass, asphalt and road.
    /// </summary>
    public static class FloorTextures
    {
        /// <summary>Metres covered by one repeat of <see cref="Tiles"/> (4 x 4 tiles of 0.5 m).</summary>
        public const float TileRepeatMetres = 2f;

        private static Texture2D _tiles, _dining, _grass, _road, _asphalt;

        public static Texture2D Tiles()
        {
            if (_tiles != null) return _tiles;
            const int size = 256, tilePx = size / 4;
            var px = new Color32[size * size];
            Color tile = new(0.95f, 0.93f, 0.92f), grout = new(0.88f, 0.85f, 0.84f), inlay = new(0.88f, 0.62f, 0.6f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int lx = x % tilePx, ly = y % tilePx;
                    // Distance to the nearest tile corner, in a diamond (L1) metric.
                    int cx = Mathf.Min(lx, tilePx - lx), cy = Mathf.Min(ly, tilePx - ly);
                    Color c = tile;
                    if (lx < 2 || ly < 2) c = grout;
                    if (cx + cy < 7) c = inlay;
                    // Soft centre diamond for a subtle pattern.
                    int dx = Mathf.Abs(lx - tilePx / 2), dy = Mathf.Abs(ly - tilePx / 2);
                    if (dx + dy < 4) c = Color.Lerp(tile, inlay, 0.55f);
                    px[y * size + x] = c;
                }
            return _tiles = Finish(New(size, "KitchenTiles"), px);
        }

        /// <summary>Warm flat dining floor with a barely visible grain so it doesn't look plastic.</summary>
        public static Texture2D Dining()
        {
            if (_dining != null) return _dining;
            const int size = 64;
            var rng = new System.Random(3);
            var px = new Color32[size * size];
            Color a = new(0.99f, 0.86f, 0.73f);
            for (int i = 0; i < px.Length; i++)
                px[i] = a * (0.985f + (float)rng.NextDouble() * 0.015f);
            return _dining = Finish(New(size, "DiningFloor"), px);
        }

        public static Texture2D Grass()
        {
            if (_grass != null) return _grass;
            const int size = 64;
            var rng = new System.Random(77);
            var px = new Color32[size * size];
            Color a = new(0.52f, 0.80f, 0.36f), b = new(0.49f, 0.77f, 0.34f);
            for (int i = 0; i < px.Length; i++)
                px[i] = Color.Lerp(a, b, (float)rng.NextDouble() * 0.5f);
            return _grass = Finish(New(size, "Grass"), px);
        }

        /// <summary>Road running along U: kerbs on both V edges and a dashed centre line.</summary>
        public static Texture2D Road()
        {
            if (_road != null) return _road;
            const int size = 64;
            var px = new Color32[size * size];
            Color asphalt = new(0.24f, 0.24f, 0.27f), dash = new(0.97f, 0.97f, 0.95f), kerb = new(0.82f, 0.82f, 0.84f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Color c = asphalt;
                    if (y < 4 || y >= size - 4) c = kerb;
                    else if (Mathf.Abs(y - size / 2) < 2 && x < size / 2) c = dash;
                    px[y * size + x] = c;
                }
            return _road = Finish(New(size, "Road"), px);
        }

        /// <summary>Car-park asphalt with a stall line on one edge.</summary>
        public static Texture2D Asphalt()
        {
            if (_asphalt != null) return _asphalt;
            const int size = 64;
            var rng = new System.Random(5);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = 0.30f + (float)rng.NextDouble() * 0.02f;
                    px[y * size + x] = x < 2 ? new Color(0.95f, 0.95f, 0.92f) : new Color(n, n, n + 0.03f);
                }
            return _asphalt = Finish(New(size, "Asphalt"), px);
        }

        private static Texture2D New(int size, string name) => new(size, size, TextureFormat.RGBA32, true)
        {
            name = name,
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 4
        };

        private static Texture2D Finish(Texture2D tex, Color32[] px)
        {
            tex.SetPixels32(px);
            tex.Apply(true, true);
            return tex;
        }
    }
}
