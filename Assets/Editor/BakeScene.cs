using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Editor-only script to bake all runtime-created objects into the scene permanently,
/// remove setup scripts, adjust camera, add walls, and improve visuals.
/// Run via menu: ShopIdle > Bake Scene.
/// </summary>
public static class BakeScene
{
    [MenuItem("ShopIdle/Bake Scene")]
    public static void Bake()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Stop Play Mode before baking.");
            return;
        }

        RemoveSetupObjects();
        CreateCustomerPrefab();
        CreateCounter();
        CreateCashierZone();
        CreateCustomerQueue();
        CreateSpawnAndExitPoints();
        CreateCustomerSpawner();
        CreateMoneyManager();
        CreateMoneySpot();
        SetupMoneyUI();
        AdjustLayout();
        CreateFloorMarkers();
        AdjustCamera();
        AddWalls();
        WireReferences();
        CreateUpgradeManager();
        SetGameViewResolution();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[BakeScene] Scene baked and saved successfully.");
    }

    private static void RemoveSetupObjects()
    {
        DestroyIfExists("Step3Setup");
        DestroyIfExists("Step4Setup");
        // Also remove old markers that may have been created by setup scripts
        DestroyIfExists("CashierMarker");
        DestroyIfExists("OvenMarker");
        DestroyIfExists("MoneySpotMarker");
        DestroyIfExists("CashierMarkerOutline");
        DestroyIfExists("OvenMarkerOutline");
        DestroyIfExists("MoneySpotMarkerOutline");
    }

    private static void DestroyIfExists(string name)
    {
        var obj = GameObject.Find(name);
        if (obj != null)
        {
            Undo.DestroyObjectImmediate(obj);
            Debug.Log($"[BakeScene] Removed {name}");
        }
    }

    // ────────────────────────────────────────────
    // Customer Prefab (hidden in scene, acts as template)
    // ────────────────────────────────────────────
    private static void CreateCustomerPrefab()
    {
        if (GameObject.Find("CustomerPrefab") != null) return;

        var customer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        customer.name = "CustomerPrefab";
        customer.transform.position = new Vector3(100f, -100f, 100f);
        Undo.RegisterCreatedObjectUndo(customer, "Create CustomerPrefab");

        customer.AddComponent<ShopIdle.Customers.Customer>();

        // Speech bubble
        var speechBubble = new GameObject("SpeechBubble");
        speechBubble.transform.SetParent(customer.transform);
        speechBubble.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        speechBubble.AddComponent<ShopIdle.UI.Billboard>();

        var bubbleBg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bubbleBg.name = "BubbleBackground";
        bubbleBg.transform.SetParent(speechBubble.transform);
        bubbleBg.transform.localPosition = Vector3.zero;
        bubbleBg.transform.localScale = new Vector3(1f, 0.6f, 1f);
        var bubbleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bubbleMat.color = Color.white;
        bubbleBg.GetComponent<MeshRenderer>().sharedMaterial = bubbleMat;
        Object.DestroyImmediate(bubbleBg.GetComponent<MeshCollider>());

        var pizzaIcon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pizzaIcon.name = "PizzaIcon";
        pizzaIcon.transform.SetParent(speechBubble.transform);
        pizzaIcon.transform.localPosition = new Vector3(-0.25f, 0f, -0.01f);
        pizzaIcon.transform.localScale = new Vector3(0.2f, 0.02f, 0.2f);
        pizzaIcon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var pizzaIconMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        pizzaIconMat.color = new Color(1f, 0.85f, 0.2f);
        pizzaIcon.GetComponent<MeshRenderer>().sharedMaterial = pizzaIconMat;
        Object.DestroyImmediate(pizzaIcon.GetComponent<CapsuleCollider>());

        var textObj = new GameObject("OrderText");
        textObj.transform.SetParent(speechBubble.transform);
        textObj.transform.localPosition = new Vector3(0.15f, 0f, -0.01f);
        var tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = "x1";
        tmp.fontSize = 4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        tmp.rectTransform.sizeDelta = new Vector2(1f, 0.8f);

        var customerComp = customer.GetComponent<ShopIdle.Customers.Customer>();
        SetField(customerComp, "_speechBubble", speechBubble);
        SetField(customerComp, "_orderText", tmp);

        customer.SetActive(false);
    }

    // ────────────────────────────────────────────
    // Counter
    // ────────────────────────────────────────────
    private static void CreateCounter()
    {
        if (GameObject.Find("Counter") != null) return;

        var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        counter.name = "Counter";
        counter.transform.position = new Vector3(0f, 0.5f, 7f);
        counter.transform.localScale = new Vector3(3f, 1f, 0.8f);
        Undo.RegisterCreatedObjectUndo(counter, "Create Counter");

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.55f, 0.35f, 0.15f);
        counter.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // ────────────────────────────────────────────
    // CashierZone
    // ────────────────────────────────────────────
    private static void CreateCashierZone()
    {
        if (GameObject.Find("CashierZone") != null) return;

        var cashierZone = new GameObject("CashierZone");
        cashierZone.transform.position = new Vector3(0f, 0.5f, 8f);
        Undo.RegisterCreatedObjectUndo(cashierZone, "Create CashierZone");

        var col = cashierZone.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(2.5f, 2f, 2f);

        cashierZone.AddComponent<ShopIdle.Customers.CounterZone>();
    }

    // ────────────────────────────────────────────
    // CustomerQueue
    // ────────────────────────────────────────────
    private static void CreateCustomerQueue()
    {
        if (GameObject.Find("CustomerQueue") != null) return;

        var queueObj = new GameObject("CustomerQueue");
        queueObj.transform.position = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(queueObj, "Create CustomerQueue");

        var queue = queueObj.AddComponent<ShopIdle.Customers.CustomerQueue>();

        var queueStart = new GameObject("QueueStart");
        queueStart.transform.position = new Vector3(0f, 0f, 5f);
        queueStart.transform.SetParent(queueObj.transform);

        SetField(queue, "_queueStart", queueStart.transform);
    }

    // ────────────────────────────────────────────
    // Spawn / Exit points
    // ────────────────────────────────────────────
    private static void CreateSpawnAndExitPoints()
    {
        if (GameObject.Find("SpawnPoint") == null)
        {
            var sp = new GameObject("SpawnPoint");
            sp.transform.position = new Vector3(-7f, 0f, -9f);
            Undo.RegisterCreatedObjectUndo(sp, "Create SpawnPoint");
        }

        if (GameObject.Find("ExitPoint") == null)
        {
            var ep = new GameObject("ExitPoint");
            ep.transform.position = new Vector3(7f, 0f, -9f);
            Undo.RegisterCreatedObjectUndo(ep, "Create ExitPoint");
        }
    }

    // ────────────────────────────────────────────
    // CustomerSpawner
    // ────────────────────────────────────────────
    private static void CreateCustomerSpawner()
    {
        if (GameObject.Find("CustomerSpawner") != null) return;

        var obj = new GameObject("CustomerSpawner");
        Undo.RegisterCreatedObjectUndo(obj, "Create CustomerSpawner");
        obj.AddComponent<ShopIdle.Customers.CustomerSpawner>();
    }

    // ────────────────────────────────────────────
    // MoneyManager
    // ────────────────────────────────────────────
    private static void CreateMoneyManager()
    {
        if (GameObject.Find("MoneyManager") != null) return;

        var obj = new GameObject("MoneyManager");
        Undo.RegisterCreatedObjectUndo(obj, "Create MoneyManager");
        obj.AddComponent<ShopIdle.Money.MoneyManager>();
    }

    // ────────────────────────────────────────────
    // MoneySpot
    // ────────────────────────────────────────────
    private static void CreateMoneySpot()
    {
        if (GameObject.Find("MoneySpot") != null) return;

        var obj = new GameObject("MoneySpot");
        obj.transform.position = new Vector3(1f, 0.5f, 7.5f);
        Undo.RegisterCreatedObjectUndo(obj, "Create MoneySpot");

        var col = obj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(2f, 2f, 2.5f);

        obj.AddComponent<ShopIdle.Money.MoneySpot>();

        var billAnchor = new GameObject("BillAnchor");
        billAnchor.transform.SetParent(obj.transform);
        billAnchor.transform.position = new Vector3(1f, 1.02f, 7f);

        var moneySpot = obj.GetComponent<ShopIdle.Money.MoneySpot>();
        SetField(moneySpot, "_billAnchor", billAnchor.transform);
    }

    // ────────────────────────────────────────────
    // Money UI
    // ────────────────────────────────────────────
    private static void SetupMoneyUI()
    {
        if (GameObject.Find("MoneyDisplay") != null) return;

        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var displayObj = new GameObject("MoneyDisplay");
        displayObj.transform.SetParent(canvas.transform, false);
        Undo.RegisterCreatedObjectUndo(displayObj, "Create MoneyDisplay");

        var rect = displayObj.GetComponent<RectTransform>();
        if (rect == null) rect = displayObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -20f);
        rect.sizeDelta = new Vector2(300f, 60f);

        var tmp = displayObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "$0";
        tmp.fontSize = 36f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        var moneyUI = displayObj.AddComponent<ShopIdle.UI.MoneyUI>();
        SetField(moneyUI, "_moneyText", tmp);
    }

    // ────────────────────────────────────────────
    // Layout adjustments
    // ────────────────────────────────────────────
    private static void AdjustLayout()
    {
        var floor = GameObject.Find("Floor");
        if (floor != null)
        {
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(1.6f, 1f, 2f);

            var renderer = floor.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.96f, 0.91f, 0.82f); // warm cream
                renderer.sharedMaterial = mat;
            }
        }

        var oven = GameObject.Find("Oven");
        if (oven != null)
            oven.transform.position = new Vector3(-4f, 0.5f, 7f);

        var player = GameObject.FindWithTag("Player");
        if (player != null)
            player.transform.position = new Vector3(0f, 0.5f, 2f);
    }

    // ────────────────────────────────────────────
    // Floor markers: bright flat squares with soft outline
    // ────────────────────────────────────────────
    private static void CreateFloorMarkers()
    {
        // Cashier zone — bright cyan with outline
        CreateZoneMarkerWithOutline("CashierMarker",
            new Vector3(0f, 0.02f, 8f),
            new Vector3(2.5f, 0.01f, 2f),
            new Color(0f, 0.85f, 1f, 0.55f),
            new Color(0f, 0.85f, 1f, 0.2f));

        // Oven zone — bright orange with outline
        CreateZoneMarkerWithOutline("OvenMarker",
            new Vector3(-4f, 0.02f, 5.8f),
            new Vector3(2.5f, 0.01f, 2f),
            new Color(1f, 0.55f, 0f, 0.55f),
            new Color(1f, 0.55f, 0f, 0.2f));

        // Money spot — bright green with outline
        CreateZoneMarkerWithOutline("MoneySpotMarker",
            new Vector3(1f, 0.02f, 7f),
            new Vector3(1.2f, 0.01f, 1f),
            new Color(0.15f, 0.9f, 0.3f, 0.6f),
            new Color(0.15f, 0.9f, 0.3f, 0.2f));
    }

    private static void CreateZoneMarkerWithOutline(string name, Vector3 position, Vector3 scale, Color fillColor, Color outlineColor)
    {
        // Destroy old versions
        var old = GameObject.Find(name);
        if (old != null) Object.DestroyImmediate(old);
        var oldOutline = GameObject.Find(name + "Outline");
        if (oldOutline != null) Object.DestroyImmediate(oldOutline);

        // Soft outline (slightly larger, more transparent)
        var outline = GameObject.CreatePrimitive(PrimitiveType.Quad);
        outline.name = name + "Outline";
        outline.transform.position = position + Vector3.down * 0.001f; // just below fill
        outline.transform.localScale = scale + new Vector3(0.6f, 0f, 0.6f);
        outline.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Object.DestroyImmediate(outline.GetComponent<MeshCollider>());
        outline.GetComponent<MeshRenderer>().sharedMaterial = CreateTransparentUnlitMat(outlineColor);
        Undo.RegisterCreatedObjectUndo(outline, $"Create {name}Outline");

        // Fill quad (inner, brighter)
        var fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.name = name;
        fill.transform.position = position;
        fill.transform.localScale = scale;
        fill.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Object.DestroyImmediate(fill.GetComponent<MeshCollider>());
        fill.GetComponent<MeshRenderer>().sharedMaterial = CreateTransparentUnlitMat(fillColor);
        Undo.RegisterCreatedObjectUndo(fill, $"Create {name}");
    }

    private static Material CreateTransparentUnlitMat(Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = color;
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInteger("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInteger("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInteger("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        return mat;
    }

    // ────────────────────────────────────────────
    // Camera: 45° angle, portrait-friendly, fills phone screen
    // ────────────────────────────────────────────
    private static void AdjustCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;

        var follow = cam.GetComponent<ShopIdle.CameraSystem.CameraFollow>();
        if (follow != null)
        {
            SetField(follow, "_offset", new Vector3(0f, 12f, -9f));
            SetField(follow, "_smoothSpeed", 5f);
            SetField(follow, "_lookAheadY", 1f);
        }

        cam.fieldOfView = 50f;

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            cam.transform.position = player.transform.position + new Vector3(0f, 12f, -9f);
            cam.transform.LookAt(player.transform.position + Vector3.up * 1f);
        }
    }

    // ────────────────────────────────────────────
    // Walls: back and sides
    // ────────────────────────────────────────────
    private static void AddWalls()
    {
        var wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        wallMat.color = new Color(0.88f, 0.83f, 0.75f); // warm beige

        CreateWall("WallBack",
            new Vector3(0f, 1.5f, 10f),
            new Vector3(16f, 3f, 0.3f),
            wallMat);

        CreateWall("WallLeft",
            new Vector3(-8f, 1.5f, 0f),
            new Vector3(0.3f, 3f, 20f),
            wallMat);

        CreateWall("WallRight",
            new Vector3(8f, 1.5f, 0f),
            new Vector3(0.3f, 3f, 20f),
            wallMat);
    }

    private static void CreateWall(string name, Vector3 position, Vector3 scale, Material mat)
    {
        var existing = GameObject.Find(name);
        if (existing != null) Object.DestroyImmediate(existing);

        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<MeshRenderer>().sharedMaterial = mat;
        wall.isStatic = true;
        Undo.RegisterCreatedObjectUndo(wall, $"Create {name}");
    }

    // ────────────────────────────────────────────
    // Wire all cross-references
    // ────────────────────────────────────────────
    private static void WireReferences()
    {
        var spawnerObj = GameObject.Find("CustomerSpawner");
        if (spawnerObj != null)
        {
            var spawner = spawnerObj.GetComponent<ShopIdle.Customers.CustomerSpawner>();
            if (spawner != null)
            {
                var prefab = GameObject.Find("CustomerPrefab");
                var spawnPt = GameObject.Find("SpawnPoint");
                var exitPt = GameObject.Find("ExitPoint");
                var queueObj = GameObject.Find("CustomerQueue");

                if (prefab != null) SetField(spawner, "_customerPrefab", prefab);
                if (spawnPt != null) SetField(spawner, "_spawnPoint", spawnPt.transform);
                if (exitPt != null) SetField(spawner, "_exitPoint", exitPt.transform);
                if (queueObj != null)
                    SetField(spawner, "_queue", queueObj.GetComponent<ShopIdle.Customers.CustomerQueue>());
            }
        }

        var cashierObj = GameObject.Find("CashierZone");
        if (cashierObj != null)
        {
            var counterZone = cashierObj.GetComponent<ShopIdle.Customers.CounterZone>();
            if (counterZone != null)
            {
                var queueObj = GameObject.Find("CustomerQueue");
                if (queueObj != null)
                    SetField(counterZone, "_queue", queueObj.GetComponent<ShopIdle.Customers.CustomerQueue>());

                var spawnerObj2 = GameObject.Find("CustomerSpawner");
                if (spawnerObj2 != null)
                    SetField(counterZone, "_spawner", spawnerObj2.GetComponent<ShopIdle.Customers.CustomerSpawner>());

                var moneySpotObj = GameObject.Find("MoneySpot");
                if (moneySpotObj != null)
                    SetField(counterZone, "_moneySpot", moneySpotObj.GetComponent<ShopIdle.Money.MoneySpot>());
            }
        }
    }

    // ────────────────────────────────────────────
    // Game View resolution: 1080x1920 portrait
    // ────────────────────────────────────────────
    // ────────────────────────────────────────────
    // UpgradeManager (runtime controller for upgrade zones)
    // ────────────────────────────────────────────
    private static void CreateUpgradeManager()
    {
        if (GameObject.Find("UpgradeManager") != null) return;

        var obj = new GameObject("UpgradeManager");
        Undo.RegisterCreatedObjectUndo(obj, "Create UpgradeManager");
        obj.AddComponent<ShopIdle.Upgrades.UpgradeManager>();
    }

    private static void SetGameViewResolution()
    {
        try
        {
            var gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            var instance = instanceProp.GetValue(null);

            var getGroupMethod = sizesType.GetMethod("GetGroup");
            var group = getGroupMethod.Invoke(instance, new object[] { (int)GameViewSizeGroupType.Standalone });
            var groupType = group.GetType();

            var getTotalCount = groupType.GetMethod("GetTotalCount");
            int count = (int)getTotalCount.Invoke(group, null);

            var getGameViewSize = groupType.GetMethod("GetGameViewSize");
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                var size = getGameViewSize.Invoke(group, new object[] { i });
                var widthProp = size.GetType().GetProperty("width");
                var heightProp = size.GetType().GetProperty("height");
                int w = (int)widthProp.GetValue(size);
                int h = (int)heightProp.GetValue(size);
                if (w == 1080 && h == 1920)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
                var gameViewSizeTypeEnum = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
                var ctor = gameViewSizeType.GetConstructor(new System.Type[] {
                    gameViewSizeTypeEnum, typeof(int), typeof(int), typeof(string) });
                var newSize = ctor.Invoke(new object[] { 1, 1080, 1920, "Portrait 1080x1920" });
                var addMethod = groupType.GetMethod("AddCustomSize");
                addMethod.Invoke(group, new object[] { newSize });
                Debug.Log("[BakeScene] Added Game view resolution 1080x1920");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[BakeScene] Could not set Game view resolution: {e.Message}. Set it manually to 1080x1920.");
        }
    }

    // ────────────────────────────────────────────
    // Reflection helper
    // ────────────────────────────────────────────
    private static void SetField(object target, string fieldName, object value)
    {
        if (target == null) return;

        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            if (target is Object unityObj)
                Undo.RecordObject(unityObj, $"Set {fieldName}");

            field.SetValue(target, value);

            if (target is Object unityObj2)
                EditorUtility.SetDirty(unityObj2);
        }
        else
        {
            Debug.LogWarning($"[BakeScene] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}
