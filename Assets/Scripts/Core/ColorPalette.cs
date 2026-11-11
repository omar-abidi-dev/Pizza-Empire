using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Central color palette for the "Sunny Toy Shop" art style.
    /// All visuals reference this for consistent pastel look.
    /// </summary>
    public class ColorPalette : ScriptableObject
    {
        [Header("Environment")]
        public Color floorCream = new(0.96f, 0.93f, 0.87f);
        public Color floorCheckerDark = new(0.91f, 0.88f, 0.82f);
        public Color wallCoral = new(0.95f, 0.65f, 0.58f);
        public Color wallTrim = new(0.98f, 0.96f, 0.94f);
        public Color grassMint = new(0.62f, 0.88f, 0.68f);
        public Color roadGrey = new(0.55f, 0.55f, 0.58f);
        public Color roadDash = new(0.95f, 0.95f, 0.95f);

        [Header("Player")]
        public Color playerSkin = new(0.96f, 0.84f, 0.72f);
        public Color playerCapTeal = new(0.25f, 0.78f, 0.75f);
        public Color playerApron = new(0.98f, 0.98f, 0.95f);

        [Header("Customers")]
        public Color customerBase = new(0.7f, 0.75f, 0.82f);
        public Color vipGold = new(1f, 0.82f, 0.25f);

        [Header("Workers")]
        public Color chefApron = new(1f, 0.55f, 0.15f);
        public Color cashierApron = new(0.4f, 0.6f, 0.95f);
        public Color cleanerApron = new(0.5f, 0.85f, 0.5f);
        public Color dtWorkerApron = new(0.85f, 0.45f, 0.85f);

        [Header("Props")]
        public Color ovenBody = new(0.38f, 0.35f, 0.4f);
        public Color ovenDoor = new(0.28f, 0.26f, 0.3f);
        public Color ovenGlow = new(1f, 0.5f, 0.15f);
        public Color ovenShell = new(0.93f, 0.42f, 0.35f);
        public Color counterTop = new(0.92f, 0.9f, 0.85f);
        public Color counterBase = new(0.7f, 0.55f, 0.35f);
        public Color tableWood = new(0.72f, 0.55f, 0.35f);
        public Color chairSeat = new(0.6f, 0.45f, 0.28f);

        [Header("Prop Models (glTF part names map to these fields)")]
        public Color counterTrim = new(0.98f, 0.85f, 0.55f);
        public Color registerBody = new(0.45f, 0.8f, 0.82f);
        public Color registerKeys = new(0.98f, 0.96f, 0.94f);
        public Color screenDark = new(0.3f, 0.32f, 0.4f);
        public Color screenGlow = new(0.45f, 0.85f, 1f);
        public Color metalLight = new(0.78f, 0.8f, 0.84f);
        public Color metalDark = new(0.45f, 0.47f, 0.52f);
        public Color trashBody = new(0.45f, 0.75f, 0.55f);
        public Color trashLid = new(0.35f, 0.62f, 0.45f);
        public Color awningRed = new(0.95f, 0.42f, 0.38f);
        public Color awningWhite = new(0.98f, 0.96f, 0.94f);
        public Color boothBody = new(0.98f, 0.82f, 0.45f);
        public Color paperBand = new(0.98f, 0.95f, 0.85f);
        public Color grillBody = new(0.95f, 0.55f, 0.35f);
        public Color sushiWood = new(0.85f, 0.7f, 0.5f);
        public Color sushiRice = new(0.98f, 0.97f, 0.93f);
        public Color sushiFish = new(0.98f, 0.55f, 0.45f);
        public Color cartBody = new(0.4f, 0.75f, 0.85f);
        public Color wheelGrey = new(0.35f, 0.35f, 0.4f);

        [Header("Food")]
        public Color pizzaCrust = new(0.92f, 0.78f, 0.45f);
        public Color pizzaSauce = new(0.9f, 0.3f, 0.2f);
        public Color pizzaCheese = new(1f, 0.92f, 0.55f);
        public Color pizzaTopping = new(0.85f, 0.25f, 0.15f);

        [Header("Money")]
        public Color billGreen = new(0.35f, 0.82f, 0.4f);
        public Color billDark = new(0.2f, 0.6f, 0.25f);

        [Header("UI")]
        public Color uiBg = new(0.15f, 0.12f, 0.22f, 0.85f);
        public Color uiAccent = new(0.25f, 0.78f, 0.75f);
        public Color uiButton = new(0.3f, 0.72f, 0.68f);
        public Color uiButtonHover = new(0.35f, 0.82f, 0.78f);
        public Color uiTextLight = new(1f, 1f, 1f);
        public Color uiTextDark = new(0.2f, 0.18f, 0.25f);
        public Color uiGemPurple = new(0.7f, 0.35f, 0.95f);

        [Header("Upgrade Zones")]
        public Color zoneDefault = new(0.6f, 0.3f, 1f, 0.4f);
        public Color zoneGold = new(1f, 0.75f, 0.1f, 0.5f);
        public Color zoneFilled = new(0.4f, 0.8f, 0.4f, 0.5f);

        // ── Singleton Access ──
        private static ColorPalette _instance;
        public static ColorPalette Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance<ColorPalette>();
                    _instance.name = "DefaultPalette";
                }
                return _instance;
            }
        }

        // ── Material Helpers ──
        private static Material _litMatCache;

        public static Material MakeLit(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.3f);
            return mat;
        }

        public static Material MakeUnlit(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = color;
            return mat;
        }

        public static Material MakeTransparent(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = color;
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            return mat;
        }
    }
}
