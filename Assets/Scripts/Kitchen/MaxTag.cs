using TMPro;
using UnityEngine;

namespace ShopIdle.Kitchen
{
    /// <summary>Tilted red "MAX" tag that pops over a station's stack while its tray is full.</summary>
    [RequireComponent(typeof(Oven))]
    public class MaxTag : MonoBehaviour
    {
        private static readonly Color TagRed = new(0.93f, 0.2f, 0.22f);

        private Oven _oven;
        private Transform _tag;
        private Transform _cam;
        private float _t;

        private void Start()
        {
            _oven = GetComponent<Oven>();
            var cam = UnityEngine.Camera.main;
            _cam = cam != null ? cam.transform : null;

            var go = new GameObject("MaxTag");
            go.transform.SetParent(transform, false);
            var tmp = go.AddComponent<TextMeshPro>();
            _tag = tmp.transform; // TMP swaps in a RectTransform: cache it after AddComponent
            tmp.text = "MAX";
            tmp.fontSize = 7f;
            tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = TagRed;
            tmp.outlineWidth = 0.3f;
            tmp.outlineColor = Color.white;
            tmp.rectTransform.sizeDelta = new Vector2(2.4f, 1f);
            _tag.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            bool full = _oven.IsFull;
            if (_tag.gameObject.activeSelf != full)
            {
                _tag.gameObject.SetActive(full);
                _t = 0f;
            }
            if (!full) return;

            _t += Time.deltaTime;
            float pop = _t < 0.2f ? Mathf.Sin(_t / 0.2f * Mathf.PI * 0.5f) * 1.2f : 1f + Mathf.Sin(_t * 5f) * 0.05f;
            _tag.localScale = Vector3.one * pop;
            // Pulled towards the camera so the stack never hides it.
            Vector3 toCam = _cam != null ? -_cam.forward : Vector3.back;
            _tag.position = _oven.StackTopWorld + Vector3.up * 0.15f + toCam * 1.2f;
            if (_cam != null) _tag.rotation = _cam.rotation * Quaternion.Euler(0f, 0f, 12f);
        }
    }
}
