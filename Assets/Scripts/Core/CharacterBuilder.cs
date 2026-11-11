using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>What a model character wears.</summary>
    public struct CharacterLook
    {
        public enum Headwear { None, HairShort, HairBun, Cap, ChefHat }

        public Color Shirt;
        public Color Skin;
        public Color Hair;
        public Headwear Head;
        public bool Apron;
        public Color ApronColor;
    }

    /// <summary>
    /// Assembles a toy character from the rig-part models (torso, head, arms, legs, accessories)
    /// under pivots at the hips, neck, shoulders and hip joints, then adds a
    /// <see cref="CharacterAnimator"/> for walking and carrying. Natural height ≈ 1.62 m.
    /// </summary>
    public static class CharacterBuilder
    {
        public const float NaturalHeight = 1.62f;   // cap / hair top in model metres
        public const float HeadTop = 1.58f;         // where hats and crowns sit

        private const float HipY = 0.52f, NeckY = 1.08f, ShoulderY = 1.01f;
        private const float ShoulderX = 0.255f, HipX = 0.12f;

        private static readonly Color[] SkinTones = { new(0.98f, 0.82f, 0.68f), new(0.86f, 0.64f, 0.48f), new(0.6f, 0.42f, 0.3f) };
        private static readonly Color[] HairColors = { new(0.45f, 0.28f, 0.18f), new(0.2f, 0.16f, 0.15f), new(0.92f, 0.72f, 0.38f), new(0.75f, 0.35f, 0.2f) };

        public static Color RandomSkin() => SkinTones[Random.Range(0, SkinTones.Length)];
        public static Color RandomHair() => HairColors[Random.Range(0, HairColors.Length)];

        /// <summary>Build the rig under <paramref name="visual"/> (a grounded, scaled container).</summary>
        public static void Build(Transform visual, CharacterLook look)
        {
            var m = PropModels.Instance;
            var hips = Pivot(visual, "Hips", new Vector3(0f, HipY, 0f));
            Place(m.CharTorso, hips, look);
            if (look.Apron && m.CharApron != null) Tint(Place(m.CharApron, hips, look), "playerApron", look.ApronColor);

            // Legs hang from the visual (not the bobbing hips) so the feet stay on the floor.
            var legL = Pivot(visual, "LegL", new Vector3(-HipX, HipY, 0f));
            var legR = Pivot(visual, "LegR", new Vector3(HipX, HipY, 0f));
            Place(m.CharLeg, legL, look);
            Place(m.CharLeg, legR, look);

            var neck = Pivot(hips, "Neck", new Vector3(0f, NeckY - HipY, 0f));
            Place(m.CharHead, neck, look);
            var hat = look.Head switch
            {
                CharacterLook.Headwear.HairShort => m.CharHairShort,
                CharacterLook.Headwear.HairBun => m.CharHairBun,
                CharacterLook.Headwear.Cap => m.CharCap,
                CharacterLook.Headwear.ChefHat => m.CharChefHat,
                _ => null
            };
            if (hat != null) Place(hat, neck, look);

            var armL = Pivot(hips, "ArmL", new Vector3(-ShoulderX, ShoulderY - HipY, 0f));
            var armR = Pivot(hips, "ArmR", new Vector3(ShoulderX, ShoulderY - HipY, 0f));
            armL.localRotation = Quaternion.Euler(0f, 0f, -7f);
            armR.localRotation = Quaternion.Euler(0f, 0f, 7f);
            Place(m.CharArm, armL, look);
            Place(m.CharArm, armR, look);

            var anim = visual.parent.GetComponent<CharacterAnimator>();
            if (anim == null) anim = visual.parent.gameObject.AddComponent<CharacterAnimator>();
            anim.Bind(hips, legL, legR, armL, armR);
        }

        private static Transform Pivot(Transform parent, string name, Vector3 localPos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            return t;
        }

        /// <summary>Spawn a part at the pivot, then recolour shirt / skin / hair for this character.</summary>
        private static GameObject Place(GameObject model, Transform pivot, CharacterLook look)
        {
            var holder = PropSpawner.Spawn(model, pivot, pivot.position, pivot.eulerAngles.y);
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;
            holder.transform.localScale = Vector3.one;
            Tint(holder, "customerBase", look.Shirt);
            Tint(holder, "playerSkin", look.Skin);
            Tint(holder, "hairBrown", look.Hair);
            return holder;
        }

        private static readonly System.Collections.Generic.Dictionary<(string, Color), Material> TintCache = new();

        /// <summary>Per-character colour: a cached copy of the palette material per (part, colour).</summary>
        private static void Tint(GameObject holder, string paletteKey, Color color)
        {
            if (color.a <= 0f) return; // default(Color): keep the palette colour
            foreach (var r in holder.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.gameObject.name != paletteKey) continue;
                if (!TintCache.TryGetValue((paletteKey, color), out var mat) || mat == null)
                {
                    mat = new Material(r.sharedMaterial) { name = $"{paletteKey}_{ColorUtility.ToHtmlStringRGB(color)}", color = color };
                    TintCache[(paletteKey, color)] = mat;
                }
                r.sharedMaterial = mat;
            }
        }
    }
}
