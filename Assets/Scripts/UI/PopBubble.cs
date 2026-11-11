using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ShopIdle.UI
{
    /// <summary>
    /// Short-lived world-space reaction above a character: an emoji face or a text bubble
    /// ("No seat!"). Pops in, floats, then shrinks away. One per owner at a time.
    /// </summary>
    public class PopBubble : MonoBehaviour
    {
        private const float PopTime = 0.18f, OutTime = 0.2f;
        private static readonly Dictionary<Texture2D, Material> Mats = new();

        private float _life, _t;
        private Vector3 _basePos;
        private Transform _cam;

        public static void Emoji(Transform owner, Texture2D face, float height = 2.7f, float duration = 1.6f)
        {
            var b = Create(owner, height, duration);
            Quad(b.transform, face, 1.15f, 1.15f, Vector3.zero);
        }

        public static void Text(Transform owner, string text, float height = 2.4f, float duration = 1.8f)
        {
            var b = Create(owner, height, duration);
            Quad(b.transform, EmojiTextures.Bubble, 2f, 1f, new Vector3(0f, 0.5f, 0f));

            var label = new GameObject("Text");
            label.transform.SetParent(b.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.6f, -0.01f);
            var tmp = label.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 4.2f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.2f, 0.18f, 0.25f);
            tmp.rectTransform.sizeDelta = new Vector2(1.8f, 0.62f);
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 1.5f;
            tmp.fontSizeMax = 4.2f;
        }

        private static PopBubble Create(Transform owner, float height, float duration)
        {
            var old = owner.Find("PopBubble");
            if (old != null) Destroy(old.gameObject);

            var go = new GameObject("PopBubble");
            go.transform.SetParent(owner, false);
            go.transform.localPosition = Vector3.up * height;
            go.transform.localScale = Vector3.zero;
            var b = go.AddComponent<PopBubble>();
            b._life = duration;
            b._basePos = go.transform.localPosition;
            return b;
        }

        private static void Quad(Transform parent, Texture2D tex, float w, float h, Vector3 offset)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(q.GetComponent<Collider>());
            q.transform.SetParent(parent, false);
            q.transform.localPosition = offset;
            q.transform.localScale = new Vector3(w, h, 1f);
            if (!Mats.TryGetValue(tex, out var mat) || mat == null)
            {
                mat = Core.ColorPalette.MakeTransparent(Color.white);
                mat.SetTexture("_BaseMap", tex);
                mat.renderQueue = 2999; // above floor decals and pads, below TMP text (3000)
                Mats[tex] = mat;
            }
            var r = q.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private void Start()
        {
            var cam = UnityEngine.Camera.main;
            _cam = cam != null ? cam.transform : null;
        }

        private void LateUpdate()
        {
            _t += Time.deltaTime;
            if (_cam != null) transform.rotation = _cam.rotation;

            float s;
            if (_t < PopTime) s = Mathf.Sin(_t / PopTime * Mathf.PI * 0.5f) * 1.15f;
            else if (_t > _life - OutTime) s = Mathf.Clamp01((_life - _t) / OutTime);
            else s = Mathf.Lerp(1.15f, 1f, Mathf.Clamp01((_t - PopTime) / 0.1f));
            transform.localScale = Vector3.one * s;
            transform.localPosition = _basePos + Vector3.up * (_t * 0.15f);

            if (_t >= _life) Destroy(gameObject);
        }
    }
}
