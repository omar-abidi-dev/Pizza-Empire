using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Ground materials and placement: tiled kitchens, warm flat dining floors, bright grass,
    /// and the road in front of the restaurant door. Textures come from <see cref="FloorTextures"/>.
    /// </summary>
    public static class FloorTheme
    {
        // Kitchen zone of the starting plot: the strip behind the counter line.
        public const float KitchenZMin = 6.2f;
        public const float KitchenZMax = 9.85f;

        private const float RoadWidth = 5f;
        private const float RoadLength = 400f;

        public enum Surface { Dining, Tiles, Grass, Asphalt }

        private static readonly Dictionary<Surface, Material> PlotMats = new();
        private static Material _locked, _lane;
        private static Transform _road;

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
                Surface.Tiles => Textured(FloorTextures.Tiles(), size / FloorTextures.TileRepeatMetres, 0.35f),
                Surface.Grass => Textured(FloorTextures.Grass(), size / 4f, 0.05f),
                Surface.Asphalt => Textured(FloorTextures.Asphalt(), 2f, 0.1f),
                _ => Textured(FloorTextures.Dining(), 1f, 0.2f)
            };
            return PlotMats[surface] = m;
        }

        /// <summary>See-through floor for plots that are not bought yet.</summary>
        public static Material LockedMaterial()
        {
            if (_locked != null) return _locked;
            _locked = ColorPalette.MakeTransparent(new Color(1f, 0.93f, 0.85f, 0.45f));
            return _locked;
        }

        /// <summary>8x3 m drive-thru lane (road runs along the quad's X axis).</summary>
        public static Material LaneMaterial()
        {
            if (_lane != null) return _lane;
            _lane = Textured(FloorTextures.Road(), 1f, 0.1f);
            _lane.mainTextureScale = new Vector2(2.6f, 1f);
            return _lane;
        }

        /// <summary>Tile strip over a kitchen zone between <paramref name="xMin"/> and <paramref name="xMax"/>.</summary>
        public static void AddKitchenTiles(string name, float xMin, float xMax)
        {
            if (GameObject.Find(name) != null) return;
            float w = xMax - xMin, d = KitchenZMax - KitchenZMin;
            var quad = GroundQuad(name, new Vector3((xMin + xMax) * 0.5f, 0.01f, (KitchenZMin + KitchenZMax) * 0.5f), w, d);
            var mat = Textured(FloorTextures.Tiles(), 1f, 0.35f);
            mat.mainTextureScale = new Vector2(w / FloorTextures.TileRepeatMetres, d / FloorTextures.TileRepeatMetres);
            quad.GetComponent<MeshRenderer>().material = mat;
        }

        /// <summary>Bright grass on the big outdoor plane.</summary>
        public static void TextureGrass(GameObject grass)
        {
            var r = grass.GetComponent<MeshRenderer>();
            Vector3 size = r.bounds.size;
            var mat = Textured(FloorTextures.Grass(), 1f, 0.05f);
            mat.mainTextureScale = new Vector2(size.x / 4f, size.z / 4f);
            r.material = mat;
        }

        /// <summary>Lay (or move) the street that runs past the front door at <paramref name="centerZ"/>.</summary>
        public static void PlaceFrontRoad(float centerZ)
        {
            if (_road == null)
            {
                var road = GroundQuad("FrontRoad", Vector3.zero, RoadLength, RoadWidth);
                var mat = Textured(FloorTextures.Road(), 1f, 0.1f);
                mat.mainTextureScale = new Vector2(RoadLength / RoadWidth / 1.5f, 1f);
                road.GetComponent<MeshRenderer>().material = mat;
                _road = road.transform;
            }
            _road.position = new Vector3(0f, -0.015f, centerZ);
        }

        private static GameObject GroundQuad(string name, Vector3 center, float sizeX, float sizeZ)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetPositionAndRotation(center, Quaternion.Euler(90f, 0f, 0f));
            quad.transform.localScale = new Vector3(sizeX, sizeZ, 1f);
            Object.Destroy(quad.GetComponent<Collider>());
            return quad;
        }

        private static Material Textured(Texture2D tex, float tiling, float smoothness)
        {
            var mat = ColorPalette.MakeLit(Color.white);
            mat.mainTexture = tex;
            mat.mainTextureScale = new Vector2(tiling, tiling);
            mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }
    }
}
