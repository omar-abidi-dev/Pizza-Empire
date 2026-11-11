using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>Shared building blocks for plots: floors, floor markers, dashed outlines, simple props.</summary>
    public static class PlotVisuals
    {
        private static readonly Dictionary<Color, Material> LitCache = new();

        public static Material Lit(Color c)
        {
            if (!LitCache.TryGetValue(c, out var m) || m == null) LitCache[c] = m = Core.ColorPalette.MakeLit(c);
            return m;
        }

        /// <summary>Walkable 8x8 floor: textured quad plus a thin collider (top at y = 0).</summary>
        public static GameObject Floor(Transform parent, Vector2Int cell, Core.FloorTheme.Surface surface)
        {
            var root = new GameObject("Floor");
            root.transform.SetParent(parent, false);
            root.transform.position = PlotGrid.Center(cell);
            root.isStatic = true;
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, -0.05f, 0f);
            col.size = new Vector3(PlotGrid.Size, 0.1f, PlotGrid.Size);

            var quad = Quad(root.transform, "Surface", Vector3.zero, PlotGrid.Size, PlotGrid.Size,
                Core.FloorTheme.PlotMaterial(surface));
            quad.transform.localPosition = new Vector3(0f, 0.004f, 0f);
            return root;
        }

        /// <summary>Flat quad lying on the floor (local XZ size).</summary>
        public static GameObject Quad(Transform parent, string name, Vector3 localPos, float sx, float sz, Material mat)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = name;
            Object.Destroy(q.GetComponent<Collider>());
            q.transform.SetParent(parent, false);
            q.transform.localPosition = localPos;
            q.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            q.transform.localScale = new Vector3(sx, sz, 1f);
            q.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return q;
        }

        /// <summary>Glowing interaction marker on the floor (oven pickup, cashier spot…).</summary>
        public static void Marker(Transform parent, Vector3 worldPos, float sx, float sz, Color color)
        {
            var q = Quad(parent, "Marker", Vector3.zero, sx, sz, Core.ColorPalette.MakeTransparent(color));
            q.transform.position = new Vector3(worldPos.x, 0.02f, worldPos.z);
        }

        /// <summary>Solid primitive prop without a collider.</summary>
        public static GameObject Prop(Transform parent, PrimitiveType shape, Vector3 worldPos, Vector3 scale, Color color,
            float yaw = 0f)
        {
            var g = GameObject.CreatePrimitive(shape);
            Object.Destroy(g.GetComponent<Collider>());
            g.transform.SetParent(parent, true);
            g.transform.SetPositionAndRotation(worldPos, Quaternion.Euler(0f, yaw, 0f));
            g.transform.localScale = scale;
            g.GetComponent<MeshRenderer>().sharedMaterial = Lit(color);
            return g;
        }

        /// <summary>Solid obstacle the player and NavMesh walk around (props with a footprint).</summary>
        public static GameObject Solid(Transform parent, PrimitiveType shape, Vector3 worldPos, Vector3 scale, Color color)
        {
            var g = Prop(parent, shape, worldPos, scale, color);
            g.AddComponent<BoxCollider>();
            return g;
        }

        /// <summary>One mesh of dashes around a square (the locked-plot outline).</summary>
        public static Mesh DashedSquare(float size, float dash, float gap, float width)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            float h = size * 0.5f - width;
            for (int side = 0; side < 4; side++)
            {
                Quaternion rot = Quaternion.Euler(0f, 90f * side, 0f);
                for (float s = -h; s < h; s += dash + gap)
                {
                    float e = Mathf.Min(s + dash, h);
                    int b = verts.Count;
                    verts.Add(rot * new Vector3(s, 0f, -h - width * 0.5f));
                    verts.Add(rot * new Vector3(s, 0f, -h + width * 0.5f));
                    verts.Add(rot * new Vector3(e, 0f, -h + width * 0.5f));
                    verts.Add(rot * new Vector3(e, 0f, -h - width * 0.5f));
                    tris.AddRange(new[] { b, b + 1, b + 2, b, b + 2, b + 3 });
                }
            }
            var mesh = new Mesh { name = "DashedOutline" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
