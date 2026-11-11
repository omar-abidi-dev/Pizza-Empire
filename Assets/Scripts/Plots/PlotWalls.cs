using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>
    /// Walls along the outside of the owned area. Edges shared by two owned plots have no wall;
    /// when a plot is bought the walls it absorbs sink into the floor. The south edge of the
    /// door plot keeps a 2 m opening (customers only — an invisible blocker stops the player).
    /// </summary>
    public class PlotWalls : MonoBehaviour
    {
        private const float TallHeight = 2.4f;   // north/east: behind the action
        private const float LowHeight = 0.9f;    // south/west: cut away towards the camera
        private const float Thickness = 0.35f;
        private const float DoorWidth = 2f;

        private readonly Dictionary<string, GameObject> _walls = new();
        private readonly Dictionary<string, GameObject> _posts = new();
        private static readonly Color WallBody = new(0.88f, 0.9f, 0.91f);
        private static readonly Color WallStripe = new(0.1f, 0.6f, 0.64f);
        private static readonly Color WallCap = new(0.99f, 0.99f, 0.97f);

        private Material _wallMat, _trimMat, _capMat;

        public void Rebuild(HashSet<Vector2Int> owned, Vector2Int doorPlot, bool animate)
        {
            var wanted = new HashSet<string>();
            foreach (var cell in owned)
                for (int d = 0; d < 4; d++)
                {
                    if (owned.Contains(cell + PlotGrid.Dirs[d])) continue;
                    bool door = d == 0 && cell == doorPlot;
                    string key = $"{cell.x},{cell.y},{d}{(door ? "D" : "")}";
                    wanted.Add(key);
                    if (!_walls.ContainsKey(key)) _walls[key] = BuildEdge(cell, d, door);
                }

            RemoveStale(_walls, wanted, animate);
            RebuildPosts(wanted, animate);
        }

        private void RemoveStale(Dictionary<string, GameObject> items, HashSet<string> wanted, bool animate)
        {
            var stale = new List<string>();
            foreach (var key in items.Keys) if (!wanted.Contains(key)) stale.Add(key);
            foreach (var key in stale)
            {
                if (items[key] != null)
                {
                    if (animate) StartCoroutine(Sink(items[key]));
                    else Destroy(items[key]);
                }
                items.Remove(key);
            }
        }

        /// <summary>A post at every wall end / corner; tall where a tall (north or east) wall meets it.</summary>
        private void RebuildPosts(HashSet<string> wallKeys, bool animate)
        {
            var tallAt = new Dictionary<string, (Vector3 pos, bool tall)>();
            foreach (var key in wallKeys)
            {
                var parts = key.TrimEnd('D').Split(',');
                var cell = new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
                int dir = int.Parse(parts[2]);
                Vector2Int d = PlotGrid.Dirs[dir];
                Vector3 mid = PlotGrid.Center(cell) + new Vector3(d.x, 0f, d.y) * PlotGrid.Half;
                Vector3 axis = d.y != 0 ? Vector3.right : Vector3.forward;
                foreach (float s in new[] { -1f, 1f })
                {
                    Vector3 p = mid + axis * (s * PlotGrid.Half);
                    string pk = $"{p.x:F1},{p.z:F1}";
                    bool tall = dir >= 2 || (tallAt.TryGetValue(pk, out var prev) && prev.tall);
                    tallAt[pk] = (p, tall);
                }
            }

            var wanted = new HashSet<string>();
            foreach (var kv in tallAt)
            {
                string key = kv.Key + (kv.Value.tall ? "T" : "L");
                wanted.Add(key);
                if (!_posts.ContainsKey(key)) _posts[key] = WallKit.Post(transform, kv.Value.pos, kv.Value.tall);
            }
            RemoveStale(_posts, wanted, animate);
        }

        private GameObject BuildEdge(Vector2Int cell, int dir, bool door)
        {
            var edge = new GameObject($"Wall_{cell.x}_{cell.y}_{dir}");
            edge.transform.SetParent(transform, false);
            Vector2Int d = PlotGrid.Dirs[dir];
            Vector3 mid = PlotGrid.Center(cell) + new Vector3(d.x, 0f, d.y) * PlotGrid.Half;
            bool alongX = d.y != 0;
            float height = dir >= 2 ? TallHeight : LowHeight;
            float length = PlotGrid.Size + Thickness;

            if (!door)
            {
                Segment(edge.transform, mid, alongX, length, height);
                return edge;
            }

            float side = (length - DoorWidth) * 0.5f;
            Vector3 axis = alongX ? Vector3.right : Vector3.forward;
            Segment(edge.transform, mid - axis * (DoorWidth + side) * 0.5f, alongX, side, height);
            Segment(edge.transform, mid + axis * (DoorWidth + side) * 0.5f, alongX, side, height);

            WallKit.Entrance(edge.transform, mid);

            // Invisible door blocker: the CharacterController can't leave, customers walk through (no physics).
            var blocker = new GameObject("DoorBlocker");
            blocker.transform.SetParent(edge.transform, false);
            blocker.transform.position = mid + Vector3.up;
            var col = blocker.AddComponent<BoxCollider>();
            col.size = alongX ? new Vector3(DoorWidth, 2f, Thickness) : new Vector3(Thickness, 2f, DoorWidth);
            return edge;
        }

        private void Segment(Transform parent, Vector3 center, bool alongX, float length, float height)
        {
            _wallMat ??= Core.ColorPalette.MakeLit(WallBody);
            _trimMat ??= Core.ColorPalette.MakeLit(WallStripe);
            _capMat ??= Core.ColorPalette.MakeLit(WallCap);

            var wall = Block(parent, "Wall", center + Vector3.up * height * 0.5f, alongX, length, height, Thickness, _wallMat);
            wall.AddComponent<BoxCollider>();

            // Coloured stripe just under a white cap: the wall reads clearly from the iso camera.
            Block(parent, "Stripe", center + Vector3.up * (height - 0.16f), alongX, length + 0.01f, 0.12f, Thickness + 0.04f, _trimMat);
            Block(parent, "Cap", center + Vector3.up * (height + 0.03f), alongX, length + 0.02f, 0.06f, Thickness + 0.08f, _capMat);

            if (height >= TallHeight && length > PlotGrid.Size) WallKit.Windows(parent, center, alongX, Thickness);
        }

        private static GameObject Block(Transform parent, string name, Vector3 pos, bool alongX,
            float length, float height, float depth, Material mat)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            Destroy(g.GetComponent<BoxCollider>());
            g.transform.SetParent(parent, false);
            g.transform.position = pos;
            g.transform.localScale = alongX ? new Vector3(length, height, depth) : new Vector3(depth, height, length);
            g.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return g;
        }

        private static IEnumerator Sink(GameObject edge)
        {
            foreach (var c in edge.GetComponentsInChildren<Collider>()) c.enabled = false;
            Vector3 start = edge.transform.position;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                float k = t / 0.5f;
                edge.transform.position = start + Vector3.down * (k * k * 2.6f);
                yield return null;
            }
            Destroy(edge);
        }
    }
}
