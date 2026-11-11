using UnityEngine;

namespace ShopIdle.Upgrades
{
    /// <summary>
    /// Big bouncing arrow over the nearest buy pad the player can fully pay for,
    /// so there is always an obvious next goal.
    /// </summary>
    public class GuideArrow : MonoBehaviour
    {
        private const float Height = 0.9f;
        private const float Bounce = 0.3f;
        private const float RetargetInterval = 0.5f;

        private static readonly Color ArrowColor = new(1f, 0.8f, 0.15f);

        private Transform _visual;
        private Transform _player;
        private UpgradeZone _target;
        private float _retarget;
        private float _t;

        public static GuideArrow Create()
        {
            var go = new GameObject("GuideArrow");
            return go.AddComponent<GuideArrow>();
        }

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            _player = player != null ? player.transform : null;
            _visual = BuildVisual(transform);
            _visual.gameObject.SetActive(false);
        }

        private void Update()
        {
            _retarget -= Time.deltaTime;
            if (_retarget <= 0f)
            {
                _retarget = RetargetInterval;
                _target = PickTarget();
            }

            bool show = _target != null;
            if (_visual.gameObject.activeSelf != show) _visual.gameObject.SetActive(show);
            if (!show) return;

            _t += Time.deltaTime;
            float y = Height + Mathf.Abs(Mathf.Sin(_t * 4f)) * Bounce;
            transform.SetPositionAndRotation(_target.transform.position + Vector3.up * y,
                Quaternion.Euler(0f, _t * 90f, 0f));
        }

        private UpgradeZone PickTarget()
        {
            var mm = Money.MoneyManager.Instance;
            if (_player == null || mm == null) return null;
            UpgradeZone best = null;
            float bestDist = float.MaxValue;
            foreach (var zone in UpgradeZone.All)
            {
                if (zone.Remaining > mm.TotalMoney) continue;
                float d = (zone.transform.position - _player.position).sqrMagnitude;
                if (d < bestDist) { best = zone; bestDist = d; }
            }
            return best;
        }

        private static Transform BuildVisual(Transform parent)
        {
            var root = new GameObject("Arrow").transform;
            root.SetParent(parent, false);
            root.localScale = Vector3.one * 2f;
            var mat = Core.ColorPalette.MakeLit(ArrowColor);
            mat.SetFloat("_Cull", 0f); // two-sided: the procedural cone needs no winding care

            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(shaft.GetComponent<Collider>());
            shaft.transform.SetParent(root, false);
            shaft.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            shaft.transform.localScale = new Vector3(0.22f, 0.3f, 0.22f);
            shaft.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var head = new GameObject("Head");
            head.transform.SetParent(root, false);
            head.AddComponent<MeshFilter>().sharedMesh = Cone(0.36f, 0.45f, 16);
            head.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return root;
        }

        /// <summary>Cone with its tip at the origin pointing down (-Y), base at <paramref name="height"/>.</summary>
        private static Mesh Cone(float radius, float height, int sides)
        {
            var verts = new Vector3[sides + 2];
            var tris = new int[sides * 6];
            verts[0] = Vector3.zero;                    // tip
            verts[sides + 1] = Vector3.up * height;     // base centre
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides;
                verts[i + 1] = new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius);
            }
            for (int i = 0; i < sides; i++)
            {
                int a = i + 1, b = i == sides - 1 ? 1 : i + 2;
                tris[i * 6 + 0] = 0; tris[i * 6 + 1] = b; tris[i * 6 + 2] = a;               // side
                tris[i * 6 + 3] = sides + 1; tris[i * 6 + 4] = a; tris[i * 6 + 5] = b;       // base
            }
            var mesh = new Mesh { name = "Cone", vertices = verts, triangles = tris };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
