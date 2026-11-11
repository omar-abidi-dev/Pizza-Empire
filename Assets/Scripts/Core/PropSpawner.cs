using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Instantiates imported prop models at real-world size and recolours them
    /// from <see cref="ColorPalette"/>. Each mesh node is named after a palette
    /// field (e.g. "counterTop"); trailing non-letter suffixes are ignored.
    /// A palette-coloured template is built once per model, so repeated spawns
    /// (pizzas, bills) are a single Instantiate with no per-spawn setup.
    /// </summary>
    public static class PropSpawner
    {
        private static readonly Dictionary<GameObject, GameObject> Templates = new();
        private static readonly Dictionary<string, Material> MaterialsByKey = new();
        private static Transform _templateRoot;

        /// <summary>
        /// Spawn <paramref name="model"/> under <paramref name="parent"/> with its base at
        /// <paramref name="worldPos"/>, facing <paramref name="yaw"/> degrees (world Y).
        /// Compensates for parent scale so the model keeps its authored metre size.
        /// </summary>
        public static GameObject Spawn(GameObject model, Transform parent, Vector3 worldPos, float yaw = 0f)
        {
            // Holder cancels the parent's (possibly non-uniform) scale and carries the yaw;
            // the model keeps its own import rotation (glTF Z-up → Y-up) beneath it.
            var holder = new GameObject(model.name).transform;
            holder.SetParent(parent, false);
            if (parent != null)
            {
                Vector3 s = parent.lossyScale;
                holder.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);
            }
            holder.SetPositionAndRotation(worldPos, Quaternion.Euler(0f, yaw, 0f));

            var obj = Object.Instantiate(GetTemplate(model), holder, false);
            obj.transform.localPosition = model.transform.localPosition;
            obj.transform.localRotation = model.transform.localRotation;
            obj.transform.localScale = model.transform.localScale;
            obj.SetActive(true);
            return holder.gameObject;
        }

        private static GameObject GetTemplate(GameObject model)
        {
            if (Templates.TryGetValue(model, out var template) && template != null)
                return template;

            if (_templateRoot == null)
            {
                var root = new GameObject("PropTemplates");
                root.SetActive(false);
                Object.DontDestroyOnLoad(root);
                _templateRoot = root.transform;
            }

            template = Object.Instantiate(model, _templateRoot);
            template.name = model.name;
            template.SetActive(false);
            ApplyPalette(template);
            foreach (var col in template.GetComponentsInChildren<Collider>(true))
                Object.Destroy(col);
            Templates[model] = template;
            return template;
        }

        private static void ApplyPalette(GameObject root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mat = MaterialFor(PaletteKey(r.name));
                if (mat == null) mat = MaterialFor(PaletteKey(r.transform.parent != null ? r.transform.parent.name : ""));
                if (mat != null) r.sharedMaterial = mat;
                else Debug.LogWarning($"[PropSpawner] No palette colour for part '{r.name}' in {root.name}");
            }
        }

        /// <summary>"counterTop_2" / "counterTop.001" → "counterTop".</summary>
        private static string PaletteKey(string nodeName)
        {
            int end = 0;
            while (end < nodeName.Length && char.IsLetter(nodeName[end])) end++;
            return nodeName.Substring(0, end);
        }

        private static Material MaterialFor(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (MaterialsByKey.TryGetValue(key, out var cached)) return cached;

            var field = typeof(ColorPalette).GetField(key, BindingFlags.Public | BindingFlags.Instance);
            Material mat = null;
            if (field != null && field.FieldType == typeof(Color))
            {
                mat = ColorPalette.MakeLit((Color)field.GetValue(ColorPalette.Instance));
                mat.name = "Palette_" + key;
                mat.enableInstancing = true;
            }
            MaterialsByKey[key] = mat;
            return mat;
        }
    }
}
