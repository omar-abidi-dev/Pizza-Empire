using UnityEngine;
using TMPro;

namespace ShopIdle.Core
{
    [DefaultExecutionOrder(1)] // Run after Step3Setup
    public class Step4Setup : MonoBehaviour
    {
        [SerializeField] private bool _runSetup = true;

        private void Start()
        {
            if (!_runSetup) return;
            _runSetup = false;

            SetupLayout();
            SetupMoneySystem();
            Debug.Log("[Step4Setup] Layout and money setup complete.");
        }

        private void SetupLayout()
        {
            // --- Resize and center floor (16 wide x 20 deep) ---
            var floor = GameObject.Find("Floor");
            if (floor != null)
            {
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(1.6f, 1f, 2f);
            }

            // --- Reposition oven to left side, ~4 units from counter ---
            var oven = GameObject.Find("Oven");
            if (oven != null)
            {
                oven.transform.position = new Vector3(-4f, 0.5f, 7f);
            }

            // --- Move player to middle of floor ---
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0f, 0.5f, 2f);
            }

            // --- Floor marker: Cashier Zone (cyan) ---
            CreateFloorMarker("CashierMarker",
                new Vector3(0f, 0.02f, 8f),
                new Vector3(2.5f, 0.01f, 2f),
                new Color(0.2f, 0.8f, 0.85f, 0.5f));

            // --- Floor marker: Oven Pickup Zone (orange) ---
            CreateFloorMarker("OvenMarker",
                new Vector3(-4f, 0.02f, 5.8f),
                new Vector3(2.5f, 0.01f, 2f),
                new Color(1f, 0.6f, 0.15f, 0.5f));
        }

        private void SetupMoneySystem()
        {
            // --- Money Manager ---
            var managerObj = new GameObject("MoneyManager");
            managerObj.AddComponent<Money.MoneyManager>();

            // --- Money Spot (on counter, with trigger for player pickup) ---
            var moneySpotObj = new GameObject("MoneySpot");
            moneySpotObj.transform.position = new Vector3(1f, 0.5f, 7.5f);

            var spotCollider = moneySpotObj.AddComponent<BoxCollider>();
            spotCollider.isTrigger = true;
            spotCollider.size = new Vector3(2f, 2f, 2.5f);

            var moneySpot = moneySpotObj.AddComponent<Money.MoneySpot>();

            // Bill anchor on counter top surface
            var billAnchor = new GameObject("BillAnchor");
            billAnchor.transform.SetParent(moneySpotObj.transform);
            billAnchor.transform.position = new Vector3(1f, 1.02f, 7f);

            SetPrivateField(moneySpot, "_billAnchor", billAnchor.transform);

            // --- Wire MoneySpot to CounterZone ---
            var cashierZone = GameObject.Find("CashierZone");
            if (cashierZone != null)
            {
                var counterZone = cashierZone.GetComponent<Customers.CounterZone>();
                if (counterZone != null)
                    SetPrivateField(counterZone, "_moneySpot", moneySpot);
            }

            // --- Money UI ---
            SetupMoneyUI();
        }

        private void SetupMoneyUI()
        {
            // Find existing MoneyDisplay text in canvas
            var moneyDisplay = GameObject.Find("MoneyDisplay");
            if (moneyDisplay != null)
            {
                var tmp = moneyDisplay.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    var moneyUI = moneyDisplay.AddComponent<UI.MoneyUI>();
                    SetPrivateField(moneyUI, "_moneyText", tmp);
                    tmp.fontSize = 36f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.text = "$0";
                }
            }
            else
            {
                // Create MoneyDisplay if it doesn't exist
                var canvas = Object.FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    var displayObj = new GameObject("MoneyDisplay");
                    displayObj.transform.SetParent(canvas.transform, false);

                    var rect = displayObj.AddComponent<RectTransform>();
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

                    var moneyUI = displayObj.AddComponent<UI.MoneyUI>();
                    SetPrivateField(moneyUI, "_moneyText", tmp);
                }
            }
        }

        private void CreateFloorMarker(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = name;
            marker.transform.position = position;
            marker.transform.localScale = scale;
            marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Remove collider (purely visual)
            Destroy(marker.GetComponent<MeshCollider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = color;
            // Enable transparency
            mat.SetFloat("_Surface", 1f); // Transparent
            mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            marker.GetComponent<MeshRenderer>().material = mat;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
            else
                Debug.LogWarning($"[Step4Setup] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}
