using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Central factory for building all "Sunny Toy Shop" style visuals.
    /// Characters, props, and environment elements.
    /// </summary>
    public static class VisualFactory
    {
        // ═══════════════════════════════════════
        //  Characters
        // ═══════════════════════════════════════

        /// <summary>Build stylized player: rounded body, sphere head, cap, apron, small arms.</summary>
        public static void BuildPlayerVisual(Transform root)
        {
            var p = ColorPalette.Instance;
            ClearChildren(root);

            // Body (capsule)
            var body = CreateChild(root, "Body", PrimitiveType.Capsule,
                new Vector3(0f, 0.5f, 0f), new Vector3(0.7f, 0.5f, 0.7f), p.playerSkin);

            // Head (sphere)
            CreateChild(root, "Head", PrimitiveType.Sphere,
                new Vector3(0f, 1.25f, 0f), new Vector3(0.55f, 0.55f, 0.55f), p.playerSkin);

            // Cap (flattened cylinder on head)
            var cap = CreateChild(root, "Cap", PrimitiveType.Cylinder,
                new Vector3(0f, 1.55f, 0.02f), new Vector3(0.5f, 0.06f, 0.5f), p.playerCapTeal);
            // Cap brim (stretched cube)
            CreateChild(root, "CapBrim", PrimitiveType.Cube,
                new Vector3(0f, 1.48f, 0.18f), new Vector3(0.48f, 0.03f, 0.25f), p.playerCapTeal);

            // Apron (cube front body)
            CreateChild(root, "Apron", PrimitiveType.Cube,
                new Vector3(0f, 0.45f, 0.2f), new Vector3(0.55f, 0.5f, 0.08f), p.playerApron);

            // Left arm
            CreateChild(root, "ArmL", PrimitiveType.Capsule,
                new Vector3(-0.42f, 0.55f, 0f), new Vector3(0.18f, 0.25f, 0.18f), p.playerSkin);

            // Right arm
            CreateChild(root, "ArmR", PrimitiveType.Capsule,
                new Vector3(0.42f, 0.55f, 0f), new Vector3(0.18f, 0.25f, 0.18f), p.playerSkin);
        }

        /// <summary>Build a customer with random accessory.</summary>
        public static void BuildCustomerVisual(Transform root, bool isVip = false)
        {
            var p = ColorPalette.Instance;
            Color bodyColor = isVip ? p.vipGold : p.customerBase;
            ClearChildren(root);

            // Body
            CreateChild(root, "Body", PrimitiveType.Capsule,
                new Vector3(0f, 0.5f, 0f), new Vector3(0.6f, 0.45f, 0.6f), bodyColor);

            // Head
            CreateChild(root, "Head", PrimitiveType.Sphere,
                new Vector3(0f, 1.15f, 0f), new Vector3(0.48f, 0.48f, 0.48f),
                new Color(0.95f, 0.82f, 0.7f));

            // Random accessory (deterministic from instance ID)
            int acc = Mathf.Abs(root.GetInstanceID()) % 4;
            switch (acc)
            {
                case 0: // Glasses
                    CreateChild(root, "Glasses", PrimitiveType.Cube,
                        new Vector3(0f, 1.18f, 0.22f), new Vector3(0.4f, 0.06f, 0.04f),
                        new Color(0.2f, 0.2f, 0.25f));
                    break;
                case 1: // Hat
                    CreateChild(root, "Hat", PrimitiveType.Cylinder,
                        new Vector3(0f, 1.42f, 0f), new Vector3(0.4f, 0.08f, 0.4f),
                        new Color(0.65f, 0.45f, 0.3f));
                    break;
                case 2: // Hair bun
                    CreateChild(root, "HairBun", PrimitiveType.Sphere,
                        new Vector3(0f, 1.42f, -0.08f), new Vector3(0.22f, 0.22f, 0.22f),
                        new Color(0.3f, 0.2f, 0.15f));
                    break;
                case 3: // Nothing extra — just taller hair
                    CreateChild(root, "Hair", PrimitiveType.Cube,
                        new Vector3(0f, 1.4f, 0f), new Vector3(0.35f, 0.12f, 0.35f),
                        new Color(0.25f, 0.18f, 0.12f));
                    break;
            }

            // Small arms
            CreateChild(root, "ArmL", PrimitiveType.Capsule,
                new Vector3(-0.38f, 0.5f, 0f), new Vector3(0.14f, 0.2f, 0.14f),
                new Color(0.95f, 0.82f, 0.7f));
            CreateChild(root, "ArmR", PrimitiveType.Capsule,
                new Vector3(0.38f, 0.5f, 0f), new Vector3(0.14f, 0.2f, 0.14f),
                new Color(0.95f, 0.82f, 0.7f));
        }

        /// <summary>Build a worker with colored apron by type.</summary>
        public static void BuildWorkerVisual(Transform root, string workerType)
        {
            var p = ColorPalette.Instance;
            Color apronColor = workerType switch
            {
                "Chef" => p.chefApron,
                "Cashier" => p.cashierApron,
                "Cleaner" => p.cleanerApron,
                "DriveThru" => p.dtWorkerApron,
                _ => Color.grey
            };

            ClearChildren(root);

            // Body
            CreateChild(root, "Body", PrimitiveType.Capsule,
                new Vector3(0f, 0.5f, 0f), new Vector3(0.6f, 0.45f, 0.6f),
                new Color(0.92f, 0.88f, 0.82f));

            // Head
            CreateChild(root, "Head", PrimitiveType.Sphere,
                new Vector3(0f, 1.15f, 0f), new Vector3(0.45f, 0.45f, 0.45f),
                new Color(0.95f, 0.82f, 0.7f));

            // Apron
            CreateChild(root, "Apron", PrimitiveType.Cube,
                new Vector3(0f, 0.4f, 0.18f), new Vector3(0.5f, 0.45f, 0.08f), apronColor);

            // Chef hat (if chef)
            if (workerType == "Chef")
            {
                CreateChild(root, "ChefHat", PrimitiveType.Cylinder,
                    new Vector3(0f, 1.45f, 0f), new Vector3(0.35f, 0.18f, 0.35f),
                    Color.white);
            }

            // Small arms
            CreateChild(root, "ArmL", PrimitiveType.Capsule,
                new Vector3(-0.35f, 0.5f, 0f), new Vector3(0.14f, 0.2f, 0.14f),
                new Color(0.95f, 0.82f, 0.7f));
            CreateChild(root, "ArmR", PrimitiveType.Capsule,
                new Vector3(0.35f, 0.5f, 0f), new Vector3(0.14f, 0.2f, 0.14f),
                new Color(0.95f, 0.82f, 0.7f));
        }

        // ═══════════════════════════════════════
        //  Props
        // ═══════════════════════════════════════

        /// <summary>Build a chunky oven with door and glow.</summary>
        public static void BuildOvenVisual(Transform root)
        {
            var p = ColorPalette.Instance;
            ClearChildren(root);

            // Main body
            CreateChild(root, "OvenBody", PrimitiveType.Cube,
                new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.9f, 1.0f), p.ovenBody);

            // Top surface (lighter)
            CreateChild(root, "OvenTop", PrimitiveType.Cube,
                new Vector3(0f, 0.92f, 0f), new Vector3(1.5f, 0.06f, 1.1f),
                new Color(0.5f, 0.48f, 0.52f));

            // Door (front face)
            CreateChild(root, "OvenDoor", PrimitiveType.Cube,
                new Vector3(0f, 0.4f, 0.52f), new Vector3(0.9f, 0.55f, 0.05f), p.ovenDoor);

            // Glow strip (warm orange under door)
            CreateChild(root, "OvenGlow", PrimitiveType.Cube,
                new Vector3(0f, 0.1f, 0.52f), new Vector3(0.85f, 0.06f, 0.05f), p.ovenGlow);

            // Handle
            CreateChild(root, "Handle", PrimitiveType.Cylinder,
                new Vector3(0f, 0.55f, 0.56f), new Vector3(0.04f, 0.2f, 0.04f),
                new Color(0.7f, 0.68f, 0.65f));
            root.Find("Handle").localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        /// <summary>Build a counter with top surface and base.</summary>
        public static void BuildCounterVisual(Transform root)
        {
            var p = ColorPalette.Instance;
            ClearChildren(root);

            // Imported model: front (customer side) faces -Z, register screen faces the cashier.
            var model = PropModels.Instance != null ? PropModels.Instance.Counter : null;
            if (model != null)
            {
                SpawnModel(model, root, "CounterModel", 180f);
                return;
            }

            // Base
            CreateChild(root, "CounterBase", PrimitiveType.Cube,
                new Vector3(0f, 0.35f, 0f), new Vector3(2.2f, 0.7f, 0.9f), p.counterBase);

            // Top
            CreateChild(root, "CounterTop", PrimitiveType.Cube,
                new Vector3(0f, 0.72f, 0f), new Vector3(2.4f, 0.06f, 1.0f), p.counterTop);
        }

        /// <summary>Build a table with rounded top and sturdy leg.</summary>
        public static void BuildTableVisual(Transform root)
        {
            var p = ColorPalette.Instance;

            // Only add if not already built
            if (root.Find("TableTopNew") != null || root.Find("TableModel") != null) return;

            // Remove old basic table visuals
            var oldTop = root.Find("TableTop");
            if (oldTop != null) Object.Destroy(oldTop.gameObject);
            var oldLeg = root.Find("TableLeg");
            if (oldLeg != null) Object.Destroy(oldLeg.gameObject);

            var model = PropModels.Instance != null ? PropModels.Instance.DiningTable : null;
            if (model != null)
            {
                SpawnModel(model, root, "TableModel", 0f);
            }
            else
            {
                // Rounded table top (cylinder = round)
                CreateChild(root, "TableTopNew", PrimitiveType.Cylinder,
                    new Vector3(0f, 0.52f, 0f), new Vector3(1.0f, 0.04f, 1.0f), p.tableWood);

                // Center leg
                CreateChild(root, "TableLegNew", PrimitiveType.Cylinder,
                    new Vector3(0f, 0.26f, 0f), new Vector3(0.12f, 0.26f, 0.12f),
                    new Color(p.tableWood.r * 0.8f, p.tableWood.g * 0.8f, p.tableWood.b * 0.8f));
            }

            // Rebuild chairs
            var oldChairs = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in root)
                if (child.name == "Chair" || child.name == "ChairBack")
                    oldChairs.Add(child);
            foreach (var c in oldChairs) Object.Destroy(c.gameObject);

            BuildChairVisual(root, new Vector3(-0.8f, 0f, 0f));
            BuildChairVisual(root, new Vector3(0.8f, 0f, 0f));
        }

        private static void BuildChairVisual(Transform parent, Vector3 localPos)
        {
            var p = ColorPalette.Instance;

            // Seat (rounded cube)
            var seat = CreateChild(parent, "ChairNew", PrimitiveType.Cube,
                localPos + new Vector3(0f, 0.3f, 0f), new Vector3(0.35f, 0.06f, 0.35f), p.chairSeat);

            // Back rest
            float backX = localPos.x > 0 ? localPos.x + 0.14f : localPos.x - 0.14f;
            CreateChild(parent, "ChairBackNew", PrimitiveType.Cube,
                new Vector3(backX, 0.48f, 0f), new Vector3(0.05f, 0.3f, 0.3f), p.chairSeat);
        }

        // ═══════════════════════════════════════
        //  Food
        // ═══════════════════════════════════════

        /// <summary>Build a stylized pizza: flat disc with crust ring and topping dots.</summary>
        public static GameObject BuildPizzaVisual()
        {
            var p = ColorPalette.Instance;
            var pizza = new GameObject("Pizza");

            // Base disc (crust color)
            var crust = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crust.name = "Crust";
            crust.transform.SetParent(pizza.transform);
            crust.transform.localPosition = Vector3.zero;
            crust.transform.localScale = new Vector3(0.4f, 0.03f, 0.4f);
            DestroyColl(crust);
            crust.GetComponent<MeshRenderer>().material = ColorPalette.MakeLit(p.pizzaCrust);

            // Sauce center (slightly smaller, red)
            var sauce = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sauce.name = "Sauce";
            sauce.transform.SetParent(pizza.transform);
            sauce.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            sauce.transform.localScale = new Vector3(0.32f, 0.015f, 0.32f);
            DestroyColl(sauce);
            sauce.GetComponent<MeshRenderer>().material = ColorPalette.MakeLit(p.pizzaSauce);

            // Cheese layer
            var cheese = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cheese.name = "Cheese";
            cheese.transform.SetParent(pizza.transform);
            cheese.transform.localPosition = new Vector3(0f, 0.025f, 0f);
            cheese.transform.localScale = new Vector3(0.3f, 0.01f, 0.3f);
            DestroyColl(cheese);
            cheese.GetComponent<MeshRenderer>().material = ColorPalette.MakeLit(p.pizzaCheese);

            // Topping dots (small spheres)
            var toppingMat = ColorPalette.MakeLit(new Color(0.85f, 0.25f, 0.15f));
            float[] angles = { 0f, 72f, 144f, 216f, 288f };
            foreach (float angle in angles)
            {
                var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dot.name = "Topping";
                dot.transform.SetParent(pizza.transform);
                float rad = angle * Mathf.Deg2Rad;
                float r = 0.08f;
                dot.transform.localPosition = new Vector3(Mathf.Cos(rad) * r, 0.04f, Mathf.Sin(rad) * r);
                dot.transform.localScale = new Vector3(0.04f, 0.03f, 0.04f);
                DestroyColl(dot);
                dot.GetComponent<MeshRenderer>().sharedMaterial = toppingMat;
            }

            return pizza;
        }

        // ═══════════════════════════════════════
        //  Environment
        // ═══════════════════════════════════════

        /// <summary>Apply the Sunny Toy Shop environment theme to the scene.</summary>
        public static void ApplyEnvironmentTheme()
        {
            var p = ColorPalette.Instance;

            // Floor with checker pattern
            ApplyFloorChecker(p);

            // Walls — coral with white trim
            ApplyWallStyle("WallLeft", p);
            ApplyWallStyle("WallBack", p);
            ApplyWallStyle("WallRight", p);
            ApplyWallStyle("WallFront", p);

            // Lighting
            ApplyLighting();
        }

        private static void ApplyFloorChecker(ColorPalette p)
        {
            var floor = GameObject.Find("Floor");
            if (floor == null) return;

            // Create a procedural checker texture
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color c1 = p.floorCream;
            Color c2 = p.floorCheckerDark;
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    tex.SetPixel(x, y, (x + y) % 2 == 0 ? c1 : c2);
            tex.Apply();

            var mat = ColorPalette.MakeLit(Color.white);
            mat.mainTexture = tex;
            mat.mainTextureScale = new Vector2(10f, 10f);
            mat.SetFloat("_Smoothness", 0.15f);
            floor.GetComponent<MeshRenderer>().material = mat;
        }

        private static void ApplyWallStyle(string wallName, ColorPalette p)
        {
            var wall = GameObject.Find(wallName);
            if (wall == null) return;

            var renderer = wall.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material = ColorPalette.MakeLit(p.wallCoral);

            // Add white trim on top (if not already present)
            string trimName = wallName + "_Trim";
            if (wall.transform.Find(trimName) != null) return;

            var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = trimName;
            trim.transform.SetParent(wall.transform);
            trim.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            trim.transform.localScale = new Vector3(1.02f, 0.06f, 1.02f);
            Object.Destroy(trim.GetComponent<BoxCollider>());
            trim.GetComponent<MeshRenderer>().material = ColorPalette.MakeLit(p.wallTrim);
        }

        private static void ApplyLighting()
        {
            // Find or create directional light
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Light dirLight = null;
            foreach (var l in lights)
                if (l.type == LightType.Directional) { dirLight = l; break; }

            if (dirLight == null)
            {
                var lightObj = new GameObject("SunLight");
                dirLight = lightObj.AddComponent<Light>();
                dirLight.type = LightType.Directional;
            }

            dirLight.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
            dirLight.color = new Color(1f, 0.97f, 0.9f); // warm
            dirLight.intensity = 1.2f;
            dirLight.shadows = LightShadows.Soft;
            dirLight.shadowStrength = 0.4f;

            // Ambient light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.85f, 0.82f, 0.78f);
            RenderSettings.ambientIntensity = 1.0f;

            // Sky color
            if (Camera.main != null)
                Camera.main.backgroundColor = new Color(0.72f, 0.88f, 0.95f);
        }

        // ═══════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════

        private static GameObject CreateChild(Transform parent, string name, PrimitiveType shape,
            Vector3 localPos, Vector3 localScale, Color color)
        {
            var obj = GameObject.CreatePrimitive(shape);
            obj.name = name;
            obj.transform.SetParent(parent);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = localScale;
            obj.transform.localRotation = Quaternion.identity;
            DestroyColl(obj);
            obj.GetComponent<MeshRenderer>().material = ColorPalette.MakeLit(color);
            return obj;
        }

        /// <summary>
        /// Spawn an imported prop model standing on the floor (y = 0) under <paramref name="root"/>,
        /// at the root's XZ position plus <paramref name="localOffset"/> (unscaled metres).
        /// </summary>
        private static GameObject SpawnModel(GameObject model, Transform root, string name,
            float yaw, Vector3 localOffset = default)
        {
            Vector3 pos = root.position + root.rotation * localOffset;
            pos.y = FloorY + localOffset.y;
            var obj = PropSpawner.Spawn(model, root, pos, root.eulerAngles.y + yaw);
            obj.name = name;
            return obj;
        }

        private const float FloorY = 0f;

        private static void DestroyColl(GameObject obj)
        {
            var col = obj.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }

        private static void ClearChildren(Transform root)
        {
            // Remove visual children but keep components/non-visual children
            var toRemove = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in root)
            {
                string n = child.name;
                if (n == "Body" || n == "Head" || n == "Cap" || n == "CapBrim" ||
                    n == "Apron" || n == "ArmL" || n == "ArmR" || n == "Glasses" ||
                    n == "Hat" || n == "HairBun" || n == "Hair" || n == "ChefHat" ||
                    n == "OvenBody" || n == "OvenTop" || n == "OvenDoor" ||
                    n == "OvenGlow" || n == "Handle" || n == "CounterBase" ||
                    n == "CounterTop" || n == "Cabin" || n.EndsWith("Model"))
                {
                    toRemove.Add(child);
                }
            }
            foreach (var t in toRemove) Object.Destroy(t.gameObject);
        }
    }
}
