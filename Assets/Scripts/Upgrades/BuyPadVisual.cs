using TMPro;
using UnityEngine;

namespace ShopIdle.Upgrades
{
    /// <summary>
    /// Floor buy pad: white rounded border, warm orange tile, title + price printed flat on it,
    /// a cash icon, and a lighter fill that grows from the front edge as money is paid in.
    /// </summary>
    public class BuyPadVisual : MonoBehaviour
    {
        public const float Size = 2.4f;
        private const float Border = 0.16f;

        private static readonly Color BorderColor = new(1f, 0.98f, 0.95f);
        private static readonly Color TileColor = new(0.98f, 0.66f, 0.38f);
        private static readonly Color FillColor = new(1f, 0.83f, 0.5f);
        private static readonly Color GoldTile = new(1f, 0.78f, 0.2f);
        private static readonly Color DimTile = new(0.82f, 0.74f, 0.68f);
        private static Mesh _outer, _inner, _fill;

        private Transform _pad, _fillT;
        private MeshRenderer _tile;
        private TextMeshPro _price;
        private Color _tileColor;
        private float _pulse;

        public void Build(string title, int price, bool gold)
        {
            float inner = Size - 2f * Border;
            _outer ??= RoundedRect(Size, Size, 0.42f, false);
            _inner ??= RoundedRect(inner, inner, 0.3f, false);
            _fill ??= RoundedRect(inner, inner, 0.3f, true);
            _tileColor = gold ? GoldTile : TileColor;

            _pad = new GameObject("Pad").transform;
            _pad.SetParent(transform, false);
            Flat(_pad, "Border", _outer, BorderColor, 0.015f);
            _tile = Flat(_pad, "Tile", _inner, _tileColor, 0.02f);
            _fillT = Flat(_pad, "Fill", _fill, FillColor, 0.025f).transform;
            _fillT.localPosition = new Vector3(0f, 0.025f, -inner * 0.5f);
            _fillT.localScale = new Vector3(1f, 1f, 0f);

            Text(_pad, title.ToUpperInvariant(), new Vector3(0f, 0.03f, 0.45f), 2.4f);
            _price = Text(_pad, price.ToString(), new Vector3(0.18f, 0.03f, -0.2f), 6f);

            // Cash icon: a little green bill left of the price.
            var bill = Flat(_pad, "CashIcon", RoundedRect(0.38f, 0.24f, 0.05f, false),
                Core.ColorPalette.Instance.billGreen, 0.03f).transform;
            bill.localPosition = new Vector3(-0.72f, 0.03f, -0.2f);
            bill.localRotation = Quaternion.Euler(0f, -12f, 0f);
        }

        /// <summary>Show what is left to pay and fill the pad with what was paid.</summary>
        public void SetProgress(int remaining, int total)
        {
            if (_price != null) _price.text = remaining.ToString();
            float paid = total > 0 ? 1f - (float)remaining / total : 0f;
            if (_fillT != null) _fillT.localScale = new Vector3(1f, 1f, Mathf.Clamp01(paid));
        }

        public void SetAffordable(bool affordable)
        {
            if (_tile != null) _tile.material.color = affordable ? _tileColor : DimTile;
            if (!affordable && _pad != null) _pad.localScale = Vector3.one;
        }

        /// <summary>Gentle breathing scale while the player can pay.</summary>
        public void Pulse(float dt)
        {
            _pulse += dt;
            if (_pad != null) _pad.localScale = Vector3.one * (1f + Mathf.Sin(_pulse * 4f) * 0.025f);
        }

        private static MeshRenderer Flat(Transform parent, string name, Mesh mesh, Color color, float y)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, y, 0f);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.material = Core.ColorPalette.MakeUnlit(color);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return r;
        }

        private static TextMeshPro Text(Transform parent, string text, Vector3 pos, float size)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            go.transform.SetLocalPositionAndRotation(pos, Quaternion.Euler(90f, 0f, 0f));
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.outlineWidth = 0.18f;
            tmp.outlineColor = new Color32(170, 90, 40, 255);
            tmp.rectTransform.sizeDelta = new Vector2(Size - 0.4f, 1f);
            return tmp;
        }

        /// <summary>Flat rounded rectangle on XZ (facing up). <paramref name="fromEdge"/> puts z=0 at its front edge.</summary>
        private static Mesh RoundedRect(float w, float d, float r, bool fromEdge)
        {
            const int cornerSteps = 6;
            var verts = new System.Collections.Generic.List<Vector3> { Vector3.zero };
            Vector2[] centers = { new(w / 2 - r, d / 2 - r), new(-w / 2 + r, d / 2 - r), new(-w / 2 + r, -d / 2 + r), new(w / 2 - r, -d / 2 + r) };
            for (int c = 0; c < 4; c++)
                for (int i = 0; i <= cornerSteps; i++)
                {
                    float a = (c * 90f + i * 90f / cornerSteps) * Mathf.Deg2Rad;
                    verts.Add(new Vector3(centers[c].x + Mathf.Cos(a) * r, 0f, centers[c].y + Mathf.Sin(a) * r));
                }
            if (fromEdge)
                for (int i = 0; i < verts.Count; i++) verts[i] += new Vector3(0f, 0f, d / 2);

            var tris = new System.Collections.Generic.List<int>();
            int n = verts.Count - 1;
            for (int i = 1; i <= n; i++)
            {
                int next = i == n ? 1 : i + 1;
                tris.Add(0); tris.Add(next); tris.Add(i); // clockwise seen from above
            }
            var mesh = new Mesh { name = "RoundedRect" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
