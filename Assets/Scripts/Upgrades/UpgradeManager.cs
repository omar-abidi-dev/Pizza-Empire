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
            public int Cost;
            public Vector3 Position;
        }

        private static readonly UpgradeDef[] AllUpgrades = new[]
        {
            new UpgradeDef { Id = "SecondOven",   Name = "Second Oven",   Cost = 50,  Position = new Vector3(-6f, 0f, 5.8f) },
            new UpgradeDef { Id = "BiggerStack",  Name = "Bigger Stack",  Cost = 80,  Position = new Vector3(3f, 0f, 2f) },
            new UpgradeDef { Id = "FasterOven",   Name = "Faster Oven",   Cost = 120, Position = new Vector3(-4f, 0f, 4f) },
            new UpgradeDef { Id = "HireCashier",  Name = "Hire Cashier",  Cost = 200, Position = new Vector3(3f, 0f, 8f) },
            new UpgradeDef { Id = "HireChef",     Name = "Hire Chef",     Cost = 250, Position = new Vector3(5f, 0f, 5f) },
            new UpgradeDef { Id = "ChefSpeed",    Name = "Chef Speed",    Cost = 300, Position = new Vector3(-2f, 0f, 2f) },
            new UpgradeDef { Id = "ChefCapacity", Name = "Chef Capacity", Cost = 350, Position = new Vector3(5f, 0f, 8f) },
            new UpgradeDef { Id = "DiningArea",  Name = "Dining Area",  Cost = 400, Position = new Vector3(-5f, 0f, -2f) },
            new UpgradeDef { Id = "MoreTables",  Name = "More Tables",  Cost = 600, Position = new Vector3(-5f, 0f, -5f) },
            new UpgradeDef { Id = "HireCleaner", Name = "Hire Cleaner", Cost = 700, Position = new Vector3(2f, 0f, -3f) },
            // ── Expansion ──
            new UpgradeDef { Id = "ExpandShop",        Name = "Expand Shop",     Cost = 1000, Position = new Vector3(6f, 0f, 0f) },
            new UpgradeDef { Id = "DriveThruWindow",   Name = "Drive-Thru",      Cost = 1200, Position = new Vector3(14f, 0f, 5f) },
            new UpgradeDef { Id = "ThirdOven",         Name = "Third Oven",      Cost = 1500, Position = new Vector3(14f, 0f, 3f) },
            new UpgradeDef { Id = "HireDTWorker",      Name = "Hire DT Worker",  Cost = 1800, Position = new Vector3(14f, 0f, 0f) },
        };

        private int _nextIndex;
        private UpgradeZone _activeZone;
        private bool _isLivePurchase;

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
            ApplyPurchasedUpgrades();
            ShowNextZone();
            RestorePartialPayment();
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
        }

        private void RestorePartialPayment()
        {
            var sm = Save.SaveManager.Instance;
            if (sm == null || sm.Data == null) return;

            int partial = sm.Data.activeZonePartialSpent;
            if (partial > 0 && _activeZone != null)
                _activeZone.SetPartialSpent(partial);
        }

        private void ShowNextZone()
        {
            if (_nextIndex >= AllUpgrades.Length) return;

            var def = AllUpgrades[_nextIndex];
            var zoneObj = new GameObject($"UpgradeZone_{def.Id}");
            zoneObj.transform.position = def.Position;

            _activeZone = zoneObj.AddComponent<UpgradeZone>();
            _activeZone.Init(def.Name, def.Cost);
            _activeZone.OnPurchased += OnZonePurchased;
        }

        private void OnZonePurchased()
        {
            if (_nextIndex >= AllUpgrades.Length) return;

            _isLivePurchase = true;
            ApplyUpgrade(AllUpgrades[_nextIndex].Id);
            _isLivePurchase = false;

            _nextIndex++;
            _activeZone = null;

            Invoke(nameof(ShowNextZone), 0.5f);
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
            oven.transform.position = new Vector3(-6f, 0.5f, 7f);
            oven.transform.localScale = firstOven.transform.localScale;

            var renderer = firstOven.GetComponent<MeshRenderer>();
            if (renderer != null)
                oven.GetComponent<MeshRenderer>().material = renderer.material;

            oven.GetComponent<BoxCollider>().isTrigger = true;

            var obstacle = oven.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;

            var ovenComp = oven.AddComponent<Kitchen.Oven>();

            var tray = new GameObject("TrayAnchor");
            tray.transform.SetParent(oven.transform);
            tray.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            ovenComp.SetTrayAnchor(tray.transform);
            if (firstComp != null)
                ovenComp.SetPizzaPrefab(firstComp.PizzaPrefab);

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

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.3f, 0.5f, 0.9f);
            worker.GetComponent<MeshRenderer>().material = mat;

            worker.AddComponent<Workers.CashierWorker>();
        }

        private void ApplyHireChef()
        {
            if (GameObject.Find("ChefWorker") != null) return;

            var chef = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            chef.name = "ChefWorker";
            chef.transform.position = new Vector3(-2f, 0.5f, 5f);
            Object.Destroy(chef.GetComponent<CapsuleCollider>());

            var bodyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bodyMat.color = new Color(1f, 0.55f, 0.15f);
            chef.GetComponent<MeshRenderer>().material = bodyMat;

            var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.name = "ChefHat";
            hat.transform.SetParent(chef.transform);
            hat.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            hat.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f);
            Object.Destroy(hat.GetComponent<CapsuleCollider>());

            var hatMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            hatMat.color = Color.white;
            hat.GetComponent<MeshRenderer>().material = hatMat;

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

            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "TableTop";
            top.transform.SetParent(tableObj.transform);
            top.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            top.transform.localScale = new Vector3(1.2f, 0.08f, 0.8f);
            Object.Destroy(top.GetComponent<BoxCollider>());

            var topMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            topMat.color = new Color(0.6f, 0.4f, 0.2f);
            top.GetComponent<MeshRenderer>().material = topMat;

            var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "TableLeg";
            leg.transform.SetParent(tableObj.transform);
            leg.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            leg.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);
            Object.Destroy(leg.GetComponent<CapsuleCollider>());

            var legMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            legMat.color = new Color(0.45f, 0.3f, 0.15f);
            leg.GetComponent<MeshRenderer>().material = legMat;

            CreateChair(tableObj.transform, new Vector3(-0.9f, 0f, 0f));
            CreateChair(tableObj.transform, new Vector3(0.9f, 0f, 0f));

            var col = tableObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 2f, 2f);
            col.center = new Vector3(0f, 1f, 0f);

            var table = tableObj.AddComponent<Tables.DineInTable>();

            var tm = Tables.TableManager.Instance;
            if (tm != null)
                tm.RegisterTable(table);
        }

        private void CreateChair(Transform parent, Vector3 localPos)
        {
            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Chair";
            seat.transform.SetParent(parent);
            seat.transform.localPosition = localPos + new Vector3(0f, 0.3f, 0f);
            seat.transform.localScale = new Vector3(0.4f, 0.06f, 0.4f);
            Object.Destroy(seat.GetComponent<BoxCollider>());

            var seatMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            seatMat.color = new Color(0.5f, 0.35f, 0.18f);
            seat.GetComponent<MeshRenderer>().material = seatMat;

            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "ChairBack";
            back.transform.SetParent(parent);
            float backX = localPos.x > 0 ? localPos.x + 0.17f : localPos.x - 0.17f;
            back.transform.localPosition = new Vector3(backX, 0.5f, 0f);
            back.transform.localScale = new Vector3(0.06f, 0.35f, 0.35f);
            Object.Destroy(back.GetComponent<BoxCollider>());
            back.GetComponent<MeshRenderer>().material = seatMat;
        }

        private void ApplyHireCleaner()
        {
            if (GameObject.Find("CleanerWorker") != null) return;

            var cleaner = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cleaner.name = "CleanerWorker";
            cleaner.transform.position = new Vector3(3f, 0.5f, -1f);
            Object.Destroy(cleaner.GetComponent<CapsuleCollider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.45f, 0.85f, 0.45f);
            cleaner.GetComponent<MeshRenderer>().material = mat;

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
            // Wall sinks with a pop
            float duration = 0.4f;
            float t = 0f;
            Vector3 startScale = wall.transform.localScale;
            Vector3 startPos = wall.transform.position;

            // Quick scale-up pop
            wall.transform.localScale = startScale * 1.15f;
            yield return new WaitForSeconds(0.1f);

            // Sink down
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

            // Big camera shake for expansion
            Core.JuiceManager.Instance?.ShakeCamera(0.5f);
            Core.AudioManager.Instance?.Play(Core.SoundType.Fanfare);

            StartCoroutine(ShowBanner("NEW AREA UNLOCKED!"));
        }

        private void CreateExpansionArea()
        {
            if (GameObject.Find("ExpansionFloor") != null) return;

            // New floor (plane default is 10x10)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "ExpansionFloor";
            floor.transform.position = new Vector3(14f, 0f, 0f);
            floor.transform.localScale = new Vector3(1.2f, 1f, 2f);

            // Match existing floor material
            var origFloor = GameObject.Find("Floor");
            if (origFloor != null)
            {
                var origMat = origFloor.GetComponent<MeshRenderer>().material;
                floor.GetComponent<MeshRenderer>().material = origMat;
            }

            // New right wall (with gap for drive-thru at Z=4 to Z=7)
            CreateExpansionWall("ExpWallRight_Upper", new Vector3(20f, 1.5f, 8.5f),
                new Vector3(0.3f, 3f, 3f));
            CreateExpansionWall("ExpWallRight_Lower", new Vector3(20f, 1.5f, -3f),
                new Vector3(0.3f, 3f, 14f));

            // Back wall extension
            CreateExpansionWall("ExpWallBack", new Vector3(14f, 1.5f, 10f),
                new Vector3(12f, 3f, 0.3f));

            // Road outside (dark gray quad)
            var road = GameObject.CreatePrimitive(PrimitiveType.Quad);
            road.name = "Road";
            road.transform.position = new Vector3(21.5f, 0.02f, 0f);
            road.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            road.transform.localScale = new Vector3(4f, 24f, 1f);
            Object.Destroy(road.GetComponent<MeshCollider>());

            var roadMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roadMat.color = new Color(0.25f, 0.25f, 0.25f);
            road.GetComponent<MeshRenderer>().material = roadMat;

            // Road center line (yellow dashed)
            var line = GameObject.CreatePrimitive(PrimitiveType.Quad);
            line.name = "RoadLine";
            line.transform.position = new Vector3(21.5f, 0.025f, 0f);
            line.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            line.transform.localScale = new Vector3(0.15f, 24f, 1f);
            Object.Destroy(line.GetComponent<MeshCollider>());

            var lineMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            lineMat.color = new Color(0.95f, 0.85f, 0.15f);
            line.GetComponent<MeshRenderer>().material = lineMat;

            // Window counter visual (small shelf at the gap)
            var shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = "DriveThruShelf";
            shelf.transform.position = new Vector3(19.8f, 0.7f, 5.5f);
            shelf.transform.localScale = new Vector3(0.6f, 0.08f, 1.5f);
            Object.Destroy(shelf.GetComponent<BoxCollider>());

            var shelfMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            shelfMat.color = new Color(0.6f, 0.4f, 0.2f);
            shelf.GetComponent<MeshRenderer>().material = shelfMat;
        }

        private void CreateExpansionWall(string name, Vector3 position, Vector3 scale)
        {
            if (GameObject.Find(name) != null) return;

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.isStatic = true;

            var origWall = GameObject.Find("WallLeft");
            if (origWall != null)
            {
                var mat = origWall.GetComponent<MeshRenderer>().material;
                wall.GetComponent<MeshRenderer>().material = mat;
            }
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

            // Floor marker for serve point
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
            oven.transform.position = new Vector3(14f, 0.5f, 7f);
            oven.transform.localScale = firstOven.transform.localScale;

            var renderer = firstOven.GetComponent<MeshRenderer>();
            if (renderer != null)
                oven.GetComponent<MeshRenderer>().material = renderer.material;

            oven.GetComponent<BoxCollider>().isTrigger = true;

            var obstacle = oven.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;

            var ovenComp = oven.AddComponent<Kitchen.Oven>();

            var tray = new GameObject("TrayAnchor");
            tray.transform.SetParent(oven.transform);
            tray.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            ovenComp.SetTrayAnchor(tray.transform);
            if (firstComp != null)
                ovenComp.SetPizzaPrefab(firstComp.PizzaPrefab);

            CreateOvenMarker(oven.transform.position);

            // Rebake NavMesh so workers can reach new oven
            RebakeNavMesh();
        }

        private void ApplyHireDTWorker()
        {
            if (GameObject.Find("DriveThruWorker") != null) return;

            var worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            worker.name = "DriveThruWorker";
            worker.transform.position = new Vector3(14f, 0.5f, 5f);
            Object.Destroy(worker.GetComponent<CapsuleCollider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.9f, 0.45f, 0.9f); // purple/pink
            worker.GetComponent<MeshRenderer>().material = mat;

            // Small delivery cap
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "DTCap";
            cap.transform.SetParent(worker.transform);
            cap.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            cap.transform.localScale = new Vector3(0.55f, 0.12f, 0.55f);
            Object.Destroy(cap.GetComponent<CapsuleCollider>());

            var capMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            capMat.color = new Color(0.9f, 0.2f, 0.2f);
            cap.GetComponent<MeshRenderer>().material = capMat;

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

            // Background
            var bgObj = new GameObject("BannerBG");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.05f, 0.42f);
            bgRect.anchorMax = new Vector2(0.95f, 0.58f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.15f, 0.85f);

            // Text
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

            // Scale punch
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

            // Fade out
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
