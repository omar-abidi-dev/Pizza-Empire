using UnityEngine;

namespace ShopIdle.Customers
{
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("Spawning")]
        [SerializeField] private GameObject _customerPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _exitPoint;
        [SerializeField] private float _spawnInterval = 3f;
        [SerializeField] private int _minPizzas = 1;
        [SerializeField] private int _maxPizzas = 3;

        [Header("References")]
        [SerializeField] private CustomerQueue _queue;

        public Vector3 ExitPosition => _exitPoint != null ? _exitPoint.position : Vector3.zero;

        private float _rateMultiplier = 1f;

        /// <summary>Faster spawning (parking lots, extra counters).</summary>
        public void SetRateMultiplier(float m) { _rateMultiplier = Mathf.Max(0.1f, m); }

        /// <summary>Move where customers appear and leave (the restaurant's front door).</summary>
        public void SetDoor(Vector3 spawn, Vector3 exit)
        {
            if (_spawnPoint != null) _spawnPoint.position = spawn;
            if (_exitPoint != null) _exitPoint.position = exit;
        }

        /// <summary>Queue with the most free room (the scene queue when nothing else exists).</summary>
        private CustomerQueue PickQueue()
        {
            CustomerQueue best = null;
            int bestFree = 0;
            foreach (var q in CustomerQueue.All)
            {
                int free = q.MaxSize - q.Count;
                if (free > bestFree) { best = q; bestFree = free; }
            }
            return best;
        }

        private float _spawnTimer;
        private const float VipChance = 0.05f;

        private static readonly Color[] CustomerColors = new Color[]
        {
            new Color(0.2f, 0.6f, 1f),   // blue
            new Color(1f, 0.4f, 0.4f),    // red
            new Color(0.4f, 0.9f, 0.4f),  // green
            new Color(1f, 0.8f, 0.2f),    // yellow
            new Color(0.8f, 0.4f, 0.9f),  // purple
            new Color(1f, 0.6f, 0.2f),    // orange
        };

        private void Start()
        {
            // Spawn first customer immediately
            _spawnTimer = _spawnInterval;
        }

        private void Update()
        {
            var queue = PickQueue();
            if (queue == null) return;

            // Rush hour makes spawn interval shorter
            float interval = _spawnInterval / _rateMultiplier;
            var rush = Core.RushHourManager.Instance;
            if (rush != null && rush.IsRushing)
                interval *= rush.SpawnMultiplier;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer < interval) return;

            _spawnTimer = 0f;
            SpawnCustomer(queue);
        }

        private void SpawnCustomer(CustomerQueue queue)
        {
            if (_customerPrefab == null || _spawnPoint == null) return;

            GameObject obj = Instantiate(_customerPrefab, _spawnPoint.position, Quaternion.identity);
            obj.SetActive(true);
            var customer = obj.GetComponent<Customer>();
            if (customer == null) return;

            // Determine VIP status (5% chance)
            bool isVip = Random.value < VipChance;

            // Random order size (VIPs order more)
            int pizzas = Random.Range(_minPizzas, _maxPizzas + 1);
            if (isVip) pizzas = Mathf.Max(pizzas * 2, 4);

            customer.Setup(pizzas, isVip);

            // Build stylized visual
            var oldMr = obj.GetComponent<MeshRenderer>();
            if (oldMr != null) oldMr.enabled = false;
            Core.VisualFactory.BuildCustomerVisual(obj.transform, isVip);

            // Add crown for VIP
            if (isVip) CreateCrown(obj.transform);

            // Set exit callback
            customer.SetExitCallback(OnCustomerReachedExit);

            // Add to queue
            queue.AddCustomer(customer);
        }

        private void CreateCrown(Transform parent)
        {
            var crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.name = "Crown";
            // Sit on the head inside the (scaled, floor-pinned) character visual.
            var visual = parent.Find("Visual");
            crown.transform.SetParent(visual != null ? visual : parent, false);
            bool rig = visual != null && visual.Find("Hips") != null;
            crown.transform.localPosition = new Vector3(0f, rig ? Core.CharacterBuilder.HeadTop : 1.42f, 0f);
            crown.transform.localScale = new Vector3(0.35f, 0.08f, 0.35f);
            Object.Destroy(crown.GetComponent<CapsuleCollider>());

            var mat = new Material(ShopIdle.Core.ColorPalette.LitShader);
            mat.color = new Color(1f, 0.75f, 0f);
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Smoothness", 0.9f);
            crown.GetComponent<MeshRenderer>().material = mat;
        }

        private void OnCustomerReachedExit(Customer customer)
        {
            Destroy(customer.gameObject);
        }

        public void SendCustomerToExit(Customer customer)
        {
            if (_exitPoint == null) return;
            customer.MarkServed();
            customer.SetTarget(_exitPoint.position);
        }
    }
}
