using UnityEngine;
using ShopIdle.Customers;
using TMPro;

namespace ShopIdle.Core
{
    /// <summary>
    /// One-time setup script for Step 3. Attach to an empty GameObject and press Play.
    /// Creates counter, cashier zone, customer queue, spawner, and customer prefab.
    /// Remove this script after the scene is set up and saved.
    /// </summary>
    public class Step3Setup : MonoBehaviour
    {
        [SerializeField] private bool _runSetup = true;

        private void Awake()
        {
            if (!_runSetup) return;
            _runSetup = false;

            BuildScene();
            Debug.Log("[Step3Setup] Scene setup complete. Save the scene and remove this script.");
        }

        private void BuildScene()
        {
            // --- Customer Prefab ---
            GameObject customerPrefab = CreateCustomerPrefab();

            // --- Counter (visual) ---
            GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Counter";
            counter.transform.position = new Vector3(0f, 0.5f, 2f);
            counter.transform.localScale = new Vector3(3f, 1f, 0.8f);
            var counterMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            counterMat.color = new Color(0.55f, 0.35f, 0.15f); // brown
            counter.GetComponent<MeshRenderer>().material = counterMat;
            // Counter is just visual, no collider trigger needed on it directly

            // --- Cashier Zone (trigger behind counter, where player stands) ---
            GameObject cashierZone = new GameObject("CashierZone");
            cashierZone.transform.position = new Vector3(0f, 0.5f, 0.8f);
            var cashierCollider = cashierZone.AddComponent<BoxCollider>();
            cashierCollider.isTrigger = true;
            cashierCollider.size = new Vector3(2f, 2f, 1.5f);

            // --- Queue system ---
            GameObject queueObj = new GameObject("CustomerQueue");
            queueObj.transform.position = Vector3.zero;
            var queue = queueObj.AddComponent<CustomerQueue>();

            // Queue start point (in front of counter, customer side)
            GameObject queueStart = new GameObject("QueueStart");
            queueStart.transform.position = new Vector3(0f, 0f, 3.5f);
            queueStart.transform.SetParent(queueObj.transform);

            // Set queue start via serialized field
            SetPrivateField(queue, "_queueStart", queueStart.transform);

            // --- Spawn and Exit points ---
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.position = new Vector3(0f, 0f, 12f);

            GameObject exitPoint = new GameObject("ExitPoint");
            exitPoint.transform.position = new Vector3(5f, 0f, 12f);

            // --- Customer Spawner ---
            GameObject spawnerObj = new GameObject("CustomerSpawner");
            var spawner = spawnerObj.AddComponent<CustomerSpawner>();
            SetPrivateField(spawner, "_customerPrefab", customerPrefab);
            SetPrivateField(spawner, "_spawnPoint", spawnPoint.transform);
            SetPrivateField(spawner, "_exitPoint", exitPoint.transform);
            SetPrivateField(spawner, "_queue", queue);

            // --- CounterZone component on cashier zone ---
            var counterZone = cashierZone.AddComponent<CounterZone>();
            SetPrivateField(counterZone, "_queue", queue);
            SetPrivateField(counterZone, "_spawner", spawner);
        }

        private GameObject CreateCustomerPrefab()
        {
            // Build customer hierarchy in scene (will act as prefab since we can't save prefabs at runtime)
            GameObject customer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            customer.name = "CustomerPrefab";
            customer.transform.position = new Vector3(100f, -100f, 100f); // hide off-screen

            // Add Customer component
            var customerComp = customer.AddComponent<Customer>();

            // Speech bubble
            GameObject speechBubble = new GameObject("SpeechBubble");
            speechBubble.transform.SetParent(customer.transform);
            speechBubble.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            speechBubble.AddComponent<UI.Billboard>();

            // Background quad for speech bubble
            GameObject bubbleBg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bubbleBg.name = "BubbleBackground";
            bubbleBg.transform.SetParent(speechBubble.transform);
            bubbleBg.transform.localPosition = Vector3.zero;
            bubbleBg.transform.localScale = new Vector3(1f, 0.6f, 1f);
            var bubbleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bubbleMat.color = Color.white;
            bubbleBg.GetComponent<MeshRenderer>().material = bubbleMat;
            Destroy(bubbleBg.GetComponent<MeshCollider>());

            // Pizza icon (small yellow cylinder)
            GameObject pizzaIcon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pizzaIcon.name = "PizzaIcon";
            pizzaIcon.transform.SetParent(speechBubble.transform);
            pizzaIcon.transform.localPosition = new Vector3(-0.25f, 0f, -0.01f);
            pizzaIcon.transform.localScale = new Vector3(0.2f, 0.02f, 0.2f);
            pizzaIcon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var pizzaIconMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pizzaIconMat.color = new Color(1f, 0.85f, 0.2f); // yellow
            pizzaIcon.GetComponent<MeshRenderer>().material = pizzaIconMat;
            Destroy(pizzaIcon.GetComponent<CapsuleCollider>());

            // Order text
            GameObject textObj = new GameObject("OrderText");
            textObj.transform.SetParent(speechBubble.transform);
            textObj.transform.localPosition = new Vector3(0.15f, 0f, -0.01f);
            var tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = "x1";
            tmp.fontSize = 4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            tmp.rectTransform.sizeDelta = new Vector2(1f, 0.8f);

            // Wire up Customer references
            SetPrivateField(customerComp, "_speechBubble", speechBubble);
            SetPrivateField(customerComp, "_orderText", tmp);

            // Disable customer prefab object until spawned
            customer.SetActive(false);

            return customer;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
            else
                Debug.LogWarning($"[Step3Setup] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}
