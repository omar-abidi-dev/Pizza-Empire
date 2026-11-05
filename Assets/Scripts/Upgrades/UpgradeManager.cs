using UnityEngine;

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
            new UpgradeDef { Id = "SecondOven",  Name = "Second Oven",  Cost = 50,  Position = new Vector3(-6f, 0f, 5.8f) },
            new UpgradeDef { Id = "BiggerStack", Name = "Bigger Stack", Cost = 80,  Position = new Vector3(3f, 0f, 2f) },
            new UpgradeDef { Id = "FasterOven",  Name = "Faster Oven",  Cost = 120, Position = new Vector3(-4f, 0f, 4f) },
            new UpgradeDef { Id = "HireCashier", Name = "Hire Cashier", Cost = 200, Position = new Vector3(3f, 0f, 8f) },
        };

        private int _nextIndex;
        private UpgradeZone _activeZone;

        private void Start()
        {
            ApplyPurchasedUpgrades();
            ShowNextZone();
        }

        private void ApplyPurchasedUpgrades()
        {
            _nextIndex = 0;
            for (int i = 0; i < AllUpgrades.Length; i++)
            {
                if (PlayerPrefs.GetInt("Upgrade_" + AllUpgrades[i].Id, 0) == 1)
                {
                    ApplyUpgrade(AllUpgrades[i].Id);
                    _nextIndex = i + 1;
                }
                else
                {
                    break;
                }
            }
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

            var def = AllUpgrades[_nextIndex];
            PlayerPrefs.SetInt("Upgrade_" + def.Id, 1);
            PlayerPrefs.Save();

            ApplyUpgrade(def.Id);
            _nextIndex++;
            _activeZone = null;

            // Small delay before showing next zone
            Invoke(nameof(ShowNextZone), 0.5f);
        }

        private void ApplyUpgrade(string id)
        {
            switch (id)
            {
                case "SecondOven":  ApplySecondOven();  break;
                case "BiggerStack": ApplyBiggerStack(); break;
                case "FasterOven":  ApplyFasterOven();  break;
                case "HireCashier": ApplyHireCashier(); break;
            }
        }

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
            marker.name = "Oven2Marker";
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
            // 30% faster: default 2s → 1.4s (fixed value to avoid compounding on reload)
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

            // Visual NPC behind counter
            var worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            worker.name = "CashierWorker";
            worker.transform.position = new Vector3(0f, 0.5f, 8.5f);
            Object.Destroy(worker.GetComponent<CapsuleCollider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.3f, 0.5f, 0.9f);
            worker.GetComponent<MeshRenderer>().material = mat;

            worker.AddComponent<Workers.CashierWorker>();
        }
    }
}
