using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShopIdle.Core
{
    /// <summary>
    /// Instantiates imported prop models at real-world size and recolours them
    /// from <see cref="ColorPalette"/>. Each mesh node is named after a palette
    /// field (e.g. "counterTop"); trailing non-letter suffixes are ignored.
    /// A template is built once per model: all parts sharing a palette colour are
    /// merged into one single-submesh mesh with a URP Lit material, so a model costs
    /// one draw call per colour instead of one per CAD face (glTF primitive).
    /// </summary>
    public static class PropSpawner
    {
        private static readonly Dictionary<GameObject, GameObject> Templates = new();
        private static readonly Dictionary<string, Material> MaterialsByKey = new();
        private static Transform _templateRoot;
        private static Material _fallbackMaterial;

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
            MergeByPalette(template);
            Templates[model] = template;
            return template;
        }

        /// <summary>Replace the imported part hierarchy with one merged mesh per palette colour.</summary>
        private static void MergeByPalette(GameObject template)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            var vertexCounts = new Dictionary<Material, int>();
            Matrix4x4 toTemplate = template.transform.worldToLocalMatrix;

            foreach (var mf in template.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null || mf.GetComponent<MeshRenderer>() == null) continue;
                var mat = MaterialForNode(mf.transform, template.name);
                if (!groups.TryGetValue(mat, out var list))
                {
                    groups[mat] = list = new List<CombineInstance>();
                    vertexCounts[mat] = 0;
                }
                Matrix4x4 m = toTemplate * mf.transform.localToWorldMatrix;
                for (int s = 0; s < mf.sharedMesh.subMeshCount; s++)
                    list.Add(new CombineInstance { mesh = mf.sharedMesh, subMeshIndex = s, transform = m });
                vertexCounts[mat] += mf.sharedMesh.vertexCount;
            }

            // Drop the imported hierarchy (immediate: the template is cloned this frame).
            for (int i = template.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(template.transform.GetChild(i).gameObject);
            foreach (var c in template.GetComponents<Component>())
                if (c is Renderer || c is MeshFilter || c is Collider) Object.DestroyImmediate(c);

            foreach (var kv in groups)
            {
                var mesh = new Mesh { name = $"{template.name}_{kv.Key.name}" };
                if (vertexCounts[kv.Key] > 65000) mesh.indexFormat = IndexFormat.UInt32;
                mesh.CombineMeshes(kv.Value.ToArray(), true, true);
                mesh.RecalculateBounds();

                var part = new GameObject(kv.Key.name.Replace("Palette_", ""));
                part.transform.SetParent(template.transform, false);
                part.AddComponent<MeshFilter>().sharedMesh = mesh;
                part.AddComponent<MeshRenderer>().sharedMaterial = kv.Key;
            }
        }

        /// <summary>
        /// Recolour every renderer (all submeshes) under <paramref name="root"/> from the palette
        /// by part name. Used by prefabs that embed a model (see <see cref="PaletteApplier"/>).
        /// </summary>
        public static void ApplyPalette(GameObject root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mat = MaterialForNode(r.transform, root.name);
                int n = Mathf.Max(1, r.sharedMaterials.Length);
                var mats = new Material[n];
                for (int i = 0; i < n; i++) mats[i] = mat;
                r.sharedMaterials = mats;
            }
        }

        private static Material MaterialForNode(Transform node, string modelName)
        {
            var mat = MaterialFor(PaletteKey(node.name));
            if (mat == null && node.parent != null) mat = MaterialFor(PaletteKey(node.parent.name));
            if (mat != null) return mat;

            Debug.LogWarning($"[PropSpawner] No palette colour for part '{node.name}' in {modelName}");
            if (_fallbackMaterial == null) _fallbackMaterial = ColorPalette.MakeLit(Color.grey);
            return _fallbackMaterial;
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
