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
        private const float Thickness = 0.3f;
        private const float DoorWidth = 2f;

        private readonly Dictionary<string, GameObject> _walls = new();
        private Material _wallMat, _trimMat;

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

            var stale = new List<string>();
            foreach (var key in _walls.Keys) if (!wanted.Contains(key)) stale.Add(key);
            foreach (var key in stale)
            {
                if (animate) StartCoroutine(Sink(_walls[key]));
                else Destroy(_walls[key]);
                _walls.Remove(key);
            }
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
            var p = Core.ColorPalette.Instance;
            _wallMat ??= Core.ColorPalette.MakeLit(p.wallCoral);
            _trimMat ??= Core.ColorPalette.MakeLit(p.wallTrim);

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(parent, false);
            wall.transform.position = center + Vector3.up * height * 0.5f;
            wall.transform.localScale = alongX
                ? new Vector3(length, height, Thickness) : new Vector3(Thickness, height, length);
            wall.GetComponent<MeshRenderer>().sharedMaterial = _wallMat;

            var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "Trim";
            Destroy(trim.GetComponent<BoxCollider>());
            trim.transform.SetParent(wall.transform, false);
            trim.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            trim.transform.localScale = new Vector3(alongX ? 1.01f : 1.2f, 0.06f / height, alongX ? 1.2f : 1.01f);
            trim.GetComponent<MeshRenderer>().sharedMaterial = _trimMat;
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
