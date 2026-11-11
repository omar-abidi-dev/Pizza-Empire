using UnityEngine;

namespace ShopIdle.UI
{
    /// <summary>
    /// Chevron painted on the floor next to the player, pointing at whatever the guide arrow
    /// targets. Hidden once the player is close.
    /// </summary>
    public class FloorPointer : MonoBehaviour
    {
        private const float Offset = 1.4f;
        private const float HideDistance = 2.6f;

        private Transform _player;
        private GameObject _quad;
        private float _t;

        public static FloorPointer Create() => new GameObject("FloorPointer").AddComponent<FloorPointer>();

        private void Start()
        {
            var p = GameObject.FindWithTag("Player");
            _player = p != null ? p.transform : null;

            _quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _quad.name = "Chevron";
            Destroy(_quad.GetComponent<Collider>());
            _quad.transform.SetParent(transform, false);
            _quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            _quad.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            var mat = Core.ColorPalette.MakeTransparent(new Color(1f, 0.85f, 0.25f, 0.9f));
            mat.SetTexture("_BaseMap", ChevronTexture());
            var r = _quad.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _quad.SetActive(false);
        }

        private void LateUpdate()
        {
            var target = Upgrades.GuideArrow.CurrentTarget;
            Vector3 to = target != null && _player != null ? target.position - _player.position : Vector3.zero;
            to.y = 0f;
            bool show = to.magnitude > HideDistance;
            if (_quad.activeSelf != show) _quad.SetActive(show);
            if (!show) return;

            _t += Time.deltaTime;
            Vector3 dir = to.normalized;
            float slide = Mathf.Repeat(_t * 1.5f, 1f) * 0.35f; // creeps forward like it's leading the way
            transform.SetPositionAndRotation(
                new Vector3(_player.position.x, 0.03f, _player.position.z) + dir * (Offset + slide),
                Quaternion.LookRotation(dir, Vector3.up));
        }

        /// <summary>White double chevron pointing to +V (the quad's forward after laying flat).</summary>
        private static Texture2D ChevronTexture()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
                    float a = 0f;
                    foreach (float off in new[] { -0.35f, 0.25f })
                    {
                        // V shape: distance from the line v = off + 0.8 - |u|
                        float d = Mathf.Abs((v - off) - (0.55f - Mathf.Abs(u) * 0.9f));
                        if (Mathf.Abs(u) < 0.75f) a = Mathf.Max(a, Mathf.Clamp01((0.13f - d) * 40f));
                    }
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }
    }
}
