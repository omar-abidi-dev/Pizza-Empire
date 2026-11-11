using UnityEngine;

namespace ShopIdle.Kitchen
{
    /// <summary>
    /// Every food type on the (endless) menu: 0 Pizza, 1 Burger, 2 Sushi, 3 Taco, then
    /// deterministic procedural foods. Creates stackable food visuals tagged with <see cref="FoodItem"/>.
    /// </summary>
    public static class FoodCatalog
    {
        private struct Def
        {
            public string Name;
            public Color Color;
            public PrimitiveType Shape;
            public Vector3 Scale;
        }

        private static readonly Def[] Handmade =
        {
            new Def { Name = "Pizza",  Color = new Color(1f, 0.82f, 0.3f),    Shape = PrimitiveType.Cylinder, Scale = new Vector3(0.4f, 0.04f, 0.4f) },
            new Def { Name = "Burger", Color = new Color(0.72f, 0.42f, 0.18f), Shape = PrimitiveType.Sphere,   Scale = new Vector3(0.38f, 0.2f, 0.38f) },
            new Def { Name = "Sushi",  Color = new Color(0.98f, 0.45f, 0.4f),  Shape = PrimitiveType.Cube,     Scale = new Vector3(0.3f, 0.14f, 0.2f) },
            new Def { Name = "Taco",   Color = new Color(0.98f, 0.8f, 0.3f),   Shape = PrimitiveType.Capsule,  Scale = new Vector3(0.22f, 0.11f, 0.36f) },
        };

        private static readonly string[] ProceduralNames =
        {
            "Noodle", "Waffle", "Dumpling", "Kebab", "Crepe", "Pretzel", "Ramen", "Donut", "Pancake", "Cookie"
        };

        private static readonly System.Collections.Generic.Dictionary<int, Material> Materials = new();

        /// <summary>Tallest a station stack may grow (metres).</summary>
        public const float MaxStackMetres = 1.6f;

        public static string Name(int type) => Get(type).Name;
        public static Color Color(int type) => Get(type).Color;

        /// <summary>Money paid per item; later menu items are worth more.</summary>
        public static int Price(int type) => Mathf.RoundToInt(10f * Mathf.Pow(1.45f, type));

        /// <summary>Vertical step between two stacked items of this type.</summary>
        public static float StackHeight(int type)
        {
            if (type == 0) return Core.RestaurantManager.PizzaStackHeight;
            var d = Get(type);
            bool tall = d.Shape == PrimitiveType.Cylinder || d.Shape == PrimitiveType.Capsule;
            return tall ? d.Scale.y * 2f : d.Scale.y;
        }

        /// <summary>How many items of this type a station tray holds (stack capped at ~1.6 m).</summary>
        public static int TrayCapacity(int type, int max) =>
            Mathf.Clamp(Mathf.FloorToInt(MaxStackMetres / StackHeight(type)), 3, max);

        /// <summary>Food visual with its pivot at the base, tagged with its type.</summary>
        public static GameObject Create(int type)
        {
            GameObject root = type == 0 ? Core.VisualFactory.BuildPizzaVisual() : CreatePrimitiveFood(type);
            if (root == null) root = CreatePrimitiveFood(type);
            var item = root.GetComponent<FoodItem>();
            if (item == null) item = root.AddComponent<FoodItem>();
            item.SetType(type);
            return root;
        }

        private static GameObject CreatePrimitiveFood(int type)
        {
            var d = Get(type);
            var obj = GameObject.CreatePrimitive(d.Shape);
            obj.name = "FoodMesh";
            obj.transform.localScale = d.Scale;
            Object.Destroy(obj.GetComponent<Collider>());

            if (!Materials.TryGetValue(type, out var mat) || mat == null)
            {
                mat = Core.ColorPalette.MakeLit(d.Color);
                mat.SetFloat("_Smoothness", 0.3f);
                Materials[type] = mat;
            }
            obj.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var root = new GameObject(d.Name);
            obj.transform.SetParent(root.transform, false);
            obj.transform.localPosition = new Vector3(0f, StackHeight(type) * 0.5f, 0f);
            return root;
        }

        private static Def Get(int type)
        {
            if (type >= 0 && type < Handmade.Length) return Handmade[type];

            var rng = new System.Random(type * 7919);
            var shapes = new[] { PrimitiveType.Cylinder, PrimitiveType.Sphere, PrimitiveType.Cube, PrimitiveType.Capsule };
            var shape = shapes[rng.Next(shapes.Length)];
            return new Def
            {
                Name = ProceduralNames[(type - Handmade.Length) % ProceduralNames.Length],
                Color = UnityEngine.Color.HSVToRGB((float)rng.NextDouble(), 0.45f, 0.95f),
                Shape = shape,
                Scale = shape switch
                {
                    PrimitiveType.Sphere => new Vector3(0.34f, 0.22f, 0.34f),
                    PrimitiveType.Cube => new Vector3(0.3f, 0.14f, 0.22f),
                    PrimitiveType.Capsule => new Vector3(0.22f, 0.11f, 0.36f),
                    _ => new Vector3(0.38f, 0.05f, 0.38f)
                }
            };
        }
    }
}
