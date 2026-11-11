using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using TMPro;
using Unity.AI.Navigation;

namespace ShopIdle.Upgrades
{
    public class UpgradeManager : MonoBehaviour
    {
        private struct UpgradeDef
        {
            public string Id;
            public string Name;
            public Vector3 Position;
        }

        private static readonly UpgradeDef[] AllUpgrades = new[]
        {
            new UpgradeDef { Id = "SecondOven",   Name = "Second Oven",   Position = new Vector3(-6f, 0f, 5.8f) },
            new UpgradeDef { Id = "BiggerStack",  Name = "Bigger Stack",  Position = new Vector3(3f, 0f, 2f) },
            new UpgradeDef { Id = "FasterOven",   Name = "Faster Oven",   Position = new Vector3(-4f, 0f, 4f) },
            new UpgradeDef { Id = "HireCashier",  Name = "Hire Cashier",  Position = new Vector3(3f, 0f, 8f) },
            new UpgradeDef { Id = "HireChef",     Name = "Hire Chef",     Position = new Vector3(5f, 0f, 5f) },
            new UpgradeDef { Id = "ChefSpeed",    Name = "Chef Speed",    Position = new Vector3(-2f, 0f, 2f) },
            new UpgradeDef { Id = "ChefCapacity", Name = "Chef Capacity", Position = new Vector3(5f, 0f, 8f) },
            new UpgradeDef { Id = "DiningArea",  Name = "Dining Area",  Position = new Vector3(-5f, 0f, -2f) },
            new UpgradeDef { Id = "MoreTables",  Name = "More Tables",  Position = new Vector3(-5f, 0f, -5f) },
            new UpgradeDef { Id = "HireCleaner", Name = "Hire Cleaner", Position = new Vector3(2f, 0f, -3f) },
            // ── Expansion ──
            new UpgradeDef { Id = "ExpandShop",        Name = "Expand Shop",     Position = new Vector3(6f, 0f, 0f) },
            new UpgradeDef { Id = "DriveThruWindow",   Name = "Drive-Thru",      Position = new Vector3(14f, 0f, 5f) },
            new UpgradeDef { Id = "ThirdOven",         Name = "Third Oven",      Position = new Vector3(14f, 0f, 3f) },
            new UpgradeDef { Id = "HireDTWorker",      Name = "Hire DT Worker",  Position = new Vector3(14f, 0f, 0f) },
        };

        private int _nextIndex;
        private UpgradeZone _activeZone;
        private bool _isLivePurchase;
        private UI.ProgressUI _progressUI;

        /// <summary>Number of fully purchased upgrades (sequential).</summary>
        public int PurchasedCount => _nextIndex;

        /// <summary>Amount already spent on the active upgrade zone (0 if none).</summary>
        public int ActiveZoneSpent
        {
            get
            {
                if (_activeZone == null) return 0;
                return _activeZone.TotalCost - _activeZone.Remaining;
            }
        }

        private void Start()
        {
            CreateProgressUI();
            ApplyPurchasedUpgrades();
            ShowNextZone();
            RestorePartialPayment();
        }

        private void CreateProgressUI()
        {
            var rm = Core.RestaurantManager.Instance;
            string name = rm != null ? rm.CurrentConfig.restaurantName : "Pizza Palace";

            var uiObj = new GameObject("ProgressUI");
            _progressUI = uiObj.AddComponent<UI.ProgressUI>();
            _progressUI.Init(name, AllUpgrades.Length);
        }

        private void ApplyPurchasedUpgrades()
        {
            _nextIndex = 0;

            var sm = Save.SaveManager.Instance;
            int savedCount = 0;
            if (sm != null && sm.Data != null)
                savedCount = sm.Data.purchasedUpgradeCount;

            for (int i = 0; i < savedCount && i < AllUpgrades.Length; i++)
            {
                ApplyUpgrade(AllUpgrades[i].Id);
                _nextIndex = i + 1;
            }

            if (_progressUI != null)
                _progressUI.UpdateProgress(_nextIndex);
        }

        private void RestorePartialPayment()
        {
            var sm = Save.SaveManager.Instance;
            if (sm == null || sm.Data == null) return;

            int partial = sm.Data.activeZonePartialSpent;
            if (partial > 0 && _activeZone != null)
                _activeZone.SetPartialSpent(partial);
        }

        private int GetCostForIndex(int index)
        {
            var rm = Core.RestaurantManager.Instance;
            if (rm != null)
                return rm.GetUpgradeCost(index);
            // Fallback to base costs
            int[] baseCosts = { 50, 80, 120, 200, 250, 300, 350, 400, 600, 700, 1000, 1200, 1500, 1800 };
            return index < baseCosts.Length ? baseCosts[index] : 9999;
        }

        private void ShowNextZone()
        {
            if (_nextIndex >= AllUpgrades.Length)
            {
                // All upgrades purchased — show NEXT RESTAURANT zone
                ShowNextRestaurantZone();
                return;
            }

            var def = AllUpgrades[_nextIndex];
            int cost = GetCostForIndex(_nextIndex);

            var zoneObj = new GameObject($"UpgradeZone_{def.Id}");
            zoneObj.transform.position = def.Position;

            _activeZone = zoneObj.AddComponent<UpgradeZone>();
            _activeZone.Init(def.Name, cost);
            _activeZone.OnPurchased += OnZonePurchased;
        }

        private void ShowNextRestaurantZone()
        {
            var rm = Core.RestaurantManager.Instance;
            if (rm == null) return;

            int cost = rm.GetNextRestaurantCost();
            var zoneObj = new GameObject("UpgradeZone_NextRestaurant");
            zoneObj.transform.position = new Vector3(0f, 0f, 3f);

            _activeZone = zoneObj.AddComponent<UpgradeZone>();
            _activeZone.Init("NEXT RESTAURANT", cost, isNextRestaurant: true);
            _activeZone.OnPurchased += OnNextRestaurantPurchased;

            // Show prestige button if eligible (restaurant 4+)
            var pm = Core.PrestigeManager.Instance;
            if (pm != null && pm.CanPrestige())
                CreatePrestigeButton();
        }

        private void CreatePrestigeButton()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var btnObj = new GameObject("PrestigeButton");
            btnObj.transform.SetParent(canvas.transform, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f, 0.12f);
            rect.anchorMax = new Vector2(0.8f, 0.17f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = btnObj.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.8f, 0.6f, 0.1f, 0.9f);

            var btn = btnObj.AddComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() =>
            {
                var pm = Core.PrestigeManager.Instance;
                if (pm != null) pm.DoPrestige();
            });

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var pm = Core.PrestigeManager.Instance;
            float nextMult = pm != null ? 1f + 0.5f * (pm.PrestigeLevel + 1) : 1.5f;
            var tmp = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = $"NEW CITY (x{nextMult:F1} income)";
            tmp.fontSize = 24;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = TMPro.FontStyles.Bold;
        }

        private void OnZonePurchased()
        {
            if (_nextIndex >= AllUpgrades.Length) return;

            _isLivePurchase = true;
            ApplyUpgrade(AllUpgrades[_nextIndex].Id);
            _isLivePurchase = false;

            _nextIndex++;
            _activeZone = null;

            if (_progressUI != null)
                _progressUI.UpdateProgress(_nextIndex);

            Invoke(nameof(ShowNextZone), 0.5f);
        }

        private void OnNextRestaurantPurchased()
        {
            _activeZone = null;
            var rm = Core.RestaurantManager.Instance;
            if (rm != null)
                rm.StartNextRestaurant();
        }

        private void ApplyUpgrade(string id)
        {
            switch (id)
            {
                case "SecondOven":      ApplySecondOven();      break;
                case "BiggerStack":     ApplyBiggerStack();     break;
                case "FasterOven":      ApplyFasterOven();      break;
                case "HireCashier":     ApplyHireCashier();     break;
                case "HireChef":        ApplyHireChef();        break;
                case "ChefSpeed":       ApplyChefSpeed();       break;
                case "ChefCapacity":    ApplyChefCapacity();    break;
                case "DiningArea":      ApplyDiningArea();      break;
                case "MoreTables":      ApplyMoreTables();      break;
                case "HireCleaner":     ApplyHireCleaner();     break;
                case "ExpandShop":      ApplyExpandShop();      break;
                case "DriveThruWindow": ApplyDriveThruWindow(); break;
                case "ThirdOven":       ApplyThirdOven();       break;
                case "HireDTWorker":    ApplyHireDTWorker();    break;
            }
        }

        // ════════════════════════════════════════
        //  Original upgrades
        // ════════════════════════════════════════

        private void ApplySecondOven()
        {
            if (GameObject.Find("Oven2") != null) return;
            var firstOven = GameObject.Find("Oven");
            if (firstOven == null) return;

            var firstComp = firstOven.GetComponent<Kitchen.Oven>();

            var oven = GameObject.CreatePrimitive(PrimitiveType.Cube);
            oven.name = "Oven2";
            oven.transform.position = new Vector3(-6f, 0f, 7f);
            oven.transform.localScale = Vector3.one;
            oven.GetComponent<MeshRenderer>().enabled = false;

            oven.GetComponent<BoxCollider>().isTrigger = true;
            oven.GetComponent<BoxCollider>().size = new Vector3(1.5f, 1.2f, 1.2f);
            oven.GetComponent<BoxCollider>().center = new Vector3(0f, 0.5f, 0f);

            var obstacle = oven.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;

            var ovenComp = oven.AddComponent<Kitchen.Oven>();

            var tray = new GameObject("TrayAnchor");
            tray.transform.SetParent(oven.transform);
            tray.transform.localPosition = new Vector3(0f, 1.0f, 0f);

            ovenComp.SetTrayAnchor(tray.transform);
            if (firstComp != null)
                ovenComp.SetPizzaPrefab(firstComp.PizzaPrefab);

            // Visual after the Oven + tray exist so the tray can align to the station top.
            Core.VisualFactory.BuildOvenVisual(oven.transform);

            CreateOvenMarker(oven.transform.position);
        }

        private void CreateOvenMarker(Vector3 ovenPos)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = "OvenMarker_New";
            marker.transform.position = new Vector3(ovenPos.x, 0.02f, ovenPos.z - 1.2f);
            marker.transform.localScale = new Vector3(2.5f, 0.01f, 2f);
            marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Object.Destroy(marker.GetComponent<MeshCollider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            var color = new Color(1f, 0.55f, 0f, 0.55f);
            mat.color = color;
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            marker.GetComponent<MeshRenderer>().material = mat;
        }

        private void ApplyBiggerStack()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            var stack = player.GetComponent<Player.PlayerStack>();
            if (stack == null) stack = player.GetComponentInChildren<Player.PlayerStack>();
            if (stack != null)
                stack.SetMaxCapacity(8);
        }

        private void ApplyFasterOven()
        {
            const float fastInterval = 1.4f;
            var ovens = Object.FindObjectsByType<Kitchen.Oven>(FindObjectsSortMode.None);
            foreach (var oven in ovens)
                oven.SetProduceInterval(fastInterval);
        }

        private void ApplyHireCashier()
        {
            var cashierZone = GameObject.Find("CashierZone");
            if (cashierZone == null) return;

            var counterZone = cashierZone.GetComponent<Customers.CounterZone>();
            if (counterZone != null)
                counterZone.EnableCashierWorker();

            if (GameObject.Find("CashierWorker") != null) return;

            var worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            worker.name = "CashierWorker";
            worker.transform.position = new Vector3(0f, 0.5f, 8.5f);
            Object.Destroy(worker.GetComponent<CapsuleCollider>());
            worker.GetComponent<MeshRenderer>().enabled = false;

            Core.VisualFactory.BuildWorkerVisual(worker.transform, "Cashier");
            worker.AddComponent<Workers.CashierWorker>();
        }

        private void ApplyHireChef()
        {
            if (GameObject.Find("ChefWorker") != null) return;

            var chef = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            chef.name = "ChefWorker";
            chef.transform.position = new Vector3(-2f, 0.5f, 5f);
            Object.Destroy(chef.GetComponent<CapsuleCollider>());
            chef.GetComponent<MeshRenderer>().enabled = false;

            Core.VisualFactory.BuildWorkerVisual(chef.transform, "Chef");

            var agent = chef.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.radius = 0.4f;
            agent.height = 2f;
            agent.stoppingDistance = 1.8f;

            chef.AddComponent<Workers.ChefWorker>();
        }

        private void ApplyChefSpeed()
        {
            const float fastSpeed = 5.5f;
            var chefs = Object.FindObjectsByType<Workers.ChefWorker>(FindObjectsSortMode.None);
            foreach (var chef in chefs)
                chef.SetMoveSpeed(fastSpeed);
        }

        private void ApplyChefCapacity()
        {
            const int bigCapacity = 6;
            var chefs = Object.FindObjectsByType<Workers.ChefWorker>(FindObjectsSortMode.None);
            foreach (var chef in chefs)
                chef.SetMaxCarry(bigCapacity);
        }

        // ════════════════════════════════════════
        //  Tables
        // ════════════════════════════════════════

        private void EnsureTableManager()
        {
            if (Tables.TableManager.Instance != null) return;
            var obj = new GameObject("TableManager");
            obj.AddComponent<Tables.TableManager>();
        }

        private void ApplyDiningArea()
        {
            EnsureTableManager();
            CreateTable("DineTable1", new Vector3(4.5f, 0f, 1f));
            CreateTable("DineTable2", new Vector3(4.5f, 0f, -2f));
        }

        private void ApplyMoreTables()
        {
            EnsureTableManager();
            CreateTable("DineTable3", new Vector3(4.5f, 0f, -5f));
            CreateTable("DineTable4", new Vector3(2f, 0f, -5f));
        }

        private void CreateTable(string name, Vector3 position)
        {
            if (GameObject.Find(name) != null) return;

            var tableObj = new GameObject(name);
            tableObj.transform.position = position;

            // Table + chairs (imported models, primitive fallback)
            Core.VisualFactory.BuildTableVisual(tableObj.transform);

            var col = tableObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 2f, 2f);
            col.center = new Vector3(0f, 1f, 0f);

            var table = tableObj.AddComponent<Tables.DineInTable>();

            var tm = Tables.TableManager.Instance;
            if (tm != null)
                tm.RegisterTable(table);
        }

        private void ApplyHireCleaner()
        {
            if (GameObject.Find("CleanerWorker") != null) return;

            var cleaner = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cleaner.name = "CleanerWorker";
            cleaner.transform.position = new Vector3(3f, 0.5f, -1f);
            Object.Destroy(cleaner.GetComponent<CapsuleCollider>());
            cleaner.GetComponent<MeshRenderer>().enabled = false;

            Core.VisualFactory.BuildWorkerVisual(cleaner.transform, "Cleaner");

            var agent = cleaner.AddComponent<NavMeshAgent>();
            agent.speed = 3f;
            agent.radius = 0.4f;
            agent.height = 2f;
            agent.stoppingDistance = 1.5f;

            cleaner.AddComponent<Workers.CleanerWorker>();
        }

        // ════════════════════════════════════════
        //  Expansion + Drive-Thru
        // ════════════════════════════════════════

        private void ApplyExpandShop()
        {
            if (_isLivePurchase)
            {
                var wall = GameObject.Find("WallRight");
                if (wall != null)
                    StartCoroutine(ExpandShopSequence(wall));
                else
                    CreateExpansionArea();
            }
            else
            {
                var wall = GameObject.Find("WallRight");
                if (wall != null) Object.Destroy(wall);
                CreateExpansionArea();
                RebakeNavMesh();
            }
        }

        private IEnumerator ExpandShopSequence(GameObject wall)
        {
            float duration = 0.4f;
            float t = 0f;
            Vector3 startScale = wall.transform.localScale;
            Vector3 startPos = wall.transform.position;

            wall.transform.localScale = startScale * 1.15f;
            yield return new WaitForSeconds(0.1f);

            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                float ease = p * p;
                wall.transform.localScale = new Vector3(
                    startScale.x, startScale.y * (1f - ease), startScale.z);
                wall.transform.position = new Vector3(
                    startPos.x, startPos.y * (1f - ease), startPos.z);
                yield return null;
            }
            Object.Destroy(wall);

            CreateExpansionArea();
            RebakeNavMesh();

            Core.JuiceManager.Instance?.ShakeCamera(0.5f);
            Core.AudioManager.Instance?.Play(Core.SoundType.Fanfare);

            // Restyle expansion area with current theme
            var themer = Object.FindAnyObjectByType<Core.SceneThemer>();
            if (themer != null) themer.OnExpansionUnlocked();

            StartCoroutine(ShowBanner("NEW AREA UNLOCKED!"));
        }

        private void CreateExpansionArea()
        {
            if (GameObject.Find("ExpansionFloor") != null) return;

            var rm = Core.RestaurantManager.Instance;
            var wallColor = rm != null ? rm.CurrentConfig.wallColor : new Color(0.85f, 0.82f, 0.75f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "ExpansionFloor";
            floor.transform.position = new Vector3(14f, 0f, 0f);
            floor.transform.localScale = new Vector3(1.2f, 1f, 2f);

            var origFloor = GameObject.Find("Floor");
            if (origFloor != null)
            {
                var origMat = origFloor.GetComponent<MeshRenderer>().material;
                floor.GetComponent<MeshRenderer>().material = origMat;
            }

            CreateExpansionWall("ExpWallRight_Upper", new Vector3(20f, 1.5f, 8.5f),
                new Vector3(0.3f, 3f, 3f), wallColor);
            CreateExpansionWall("ExpWallRight_Lower", new Vector3(20f, 1.5f, -3f),
                new Vector3(0.3f, 3f, 14f), wallColor);
            CreateExpansionWall("ExpWallBack", new Vector3(14f, 1.5f, 10f),
                new Vector3(12f, 3f, 0.3f), wallColor);

            var road = GameObject.CreatePrimitive(PrimitiveType.Quad);
            road.name = "Road";
            road.transform.position = new Vector3(21.5f, 0.02f, 0f);
            road.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            road.transform.localScale = new Vector3(4f, 24f, 1f);
            Object.Destroy(road.GetComponent<MeshCollider>());

            var roadMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roadMat.color = new Color(0.25f, 0.25f, 0.25f);
            road.GetComponent<MeshRenderer>().material = roadMat;

            var line = GameObject.CreatePrimitive(PrimitiveType.Quad);
            line.name = "RoadLine";
            line.transform.position = new Vector3(21.5f, 0.025f, 0f);
            line.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            line.transform.localScale = new Vector3(0.15f, 24f, 1f);
            Object.Destroy(line.GetComponent<MeshCollider>());

            var lineMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            lineMat.color = new Color(0.95f, 0.85f, 0.15f);
            line.GetComponent<MeshRenderer>().material = lineMat;

            // Booth model fills the wall gap (z 4–7); primitive shelf as fallback.
            if (!Core.VisualFactory.BuildDriveThruBooth(new Vector3(20f, 0f, 5.5f)))
            {
                var shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.name = "DriveThruShelf";
                shelf.transform.position = new Vector3(19.8f, 0.7f, 5.5f);
                shelf.transform.localScale = new Vector3(0.6f, 0.08f, 1.5f);
                Object.Destroy(shelf.GetComponent<BoxCollider>());

                var shelfMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                shelfMat.color = new Color(0.6f, 0.4f, 0.2f);
                shelf.GetComponent<MeshRenderer>().material = shelfMat;
            }
        }

        private void CreateExpansionWall(string name, Vector3 position, Vector3 scale, Color color)
        {
            if (GameObject.Find(name) != null) return;

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.isStatic = true;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            wall.GetComponent<MeshRenderer>().material = mat;
        }

        private void RebakeNavMesh()
        {
            var surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
            foreach (var s in surfaces)
                s.BuildNavMesh();
        }

        private void ApplyDriveThruWindow()
        {
            if (GameObject.Find("DriveThruWindow") != null) return;

            var windowObj = new GameObject("DriveThruWindow");
            windowObj.transform.position = new Vector3(19f, 0f, 5.5f);
            windowObj.AddComponent<DriveThru.DriveThruWindow>();

            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = "DTWindowMarker";
            marker.transform.position = new Vector3(19f, 0.02f, 5.5f);
            marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            marker.transform.localScale = new Vector3(2.5f, 0.01f, 2.5f);
            Object.Destroy(marker.GetComponent<MeshCollider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = new Color(0.2f, 0.8f, 1f, 0.5f);
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            marker.GetComponent<MeshRenderer>().material = mat;
        }

        private void ApplyThirdOven()
        {
            if (GameObject.Find("Oven3") != null) return;
            var firstOven = GameObject.Find("Oven");
            if (firstOven == null) return;

            var firstComp = firstOven.GetComponent<Kitchen.Oven>();

            var oven = GameObject.CreatePrimitive(PrimitiveType.Cube);
            oven.name = "Oven3";
            oven.transform.position = new Vector3(14f, 0f, 7f);
            oven.transform.localScale = Vector3.one;
            oven.GetComponent<MeshRenderer>().enabled = false;

            oven.GetComponent<BoxCollider>().isTrigger = true;
            oven.GetComponent<BoxCollider>().size = new Vector3(1.5f, 1.2f, 1.2f);
            oven.GetComponent<BoxCollider>().center = new Vector3(0f, 0.5f, 0f);

            var obstacle = oven.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;

            var ovenComp = oven.AddComponent<Kitchen.Oven>();

            var tray = new GameObject("TrayAnchor");
            tray.transform.SetParent(oven.transform);
            tray.transform.localPosition = new Vector3(0f, 1.0f, 0f);

            ovenComp.SetTrayAnchor(tray.transform);
            if (firstComp != null)
                ovenComp.SetPizzaPrefab(firstComp.PizzaPrefab);

            // Visual after the Oven + tray exist so the tray can align to the station top.
            Core.VisualFactory.BuildOvenVisual(oven.transform);

            CreateOvenMarker(oven.transform.position);
            RebakeNavMesh();
        }

        private void ApplyHireDTWorker()
        {
            if (GameObject.Find("DriveThruWorker") != null) return;

            var worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            worker.name = "DriveThruWorker";
            worker.transform.position = new Vector3(14f, 0.5f, 5f);
            Object.Destroy(worker.GetComponent<CapsuleCollider>());
            worker.GetComponent<MeshRenderer>().enabled = false;

            Core.VisualFactory.BuildWorkerVisual(worker.transform, "DriveThru");

            var agent = worker.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.radius = 0.4f;
            agent.height = 2f;
            agent.stoppingDistance = 1.8f;

            worker.AddComponent<Workers.DriveThruWorker>();
        }

        // ════════════════════════════════════════
        //  UI Banner
        // ════════════════════════════════════════

        private IEnumerator ShowBanner(string text)
        {
            var canvasObj = new GameObject("BannerCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var bgObj = new GameObject("BannerBG");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.05f, 0.42f);
            bgRect.anchorMax = new Vector2(0.95f, 0.58f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.15f, 0.85f);

            var textObj = new GameObject("BannerText");
            textObj.transform.SetParent(canvasObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.42f);
            textRect.anchorMax = new Vector2(0.95f, 0.58f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 52;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.85f, 0.1f);
            tmp.fontStyle = FontStyles.Bold;

            canvasObj.transform.localScale = Vector3.one * 0.5f;
            float punch = 0f;
            while (punch < 0.2f)
            {
                punch += Time.deltaTime;
                float s = Mathf.Lerp(0.5f, 1f, punch / 0.2f);
                canvasObj.transform.localScale = Vector3.one * s;
                yield return null;
            }
            canvasObj.transform.localScale = Vector3.one;

            yield return new WaitForSeconds(2f);

            float fade = 0f;
            while (fade < 0.5f)
            {
                fade += Time.deltaTime;
                float a = 1f - (fade / 0.5f);
                bgImage.color = new Color(0.08f, 0.08f, 0.15f, 0.85f * a);
                tmp.color = new Color(1f, 0.85f, 0.1f, a);
                yield return null;
            }

            Object.Destroy(canvasObj);
        }
    }
}
