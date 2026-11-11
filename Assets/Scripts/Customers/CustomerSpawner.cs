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
            if (_queue == null || _queue.IsFull) return;

            // Rush hour makes spawn interval shorter
            float interval = _spawnInterval;
            var rush = Core.RushHourManager.Instance;
            if (rush != null && rush.IsRushing)
                interval *= rush.SpawnMultiplier;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer < interval) return;

            _spawnTimer = 0f;
            SpawnCustomer();
        }

        private void SpawnCustomer()
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

            // Color: VIP is gold, normal is random
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = obj.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                var mat = renderer.material;
                mat.color = isVip
                    ? new Color(1f, 0.85f, 0.2f)
                    : CustomerColors[Random.Range(0, CustomerColors.Length)];
            }

            // Add crown for VIP
            if (isVip) CreateCrown(obj.transform);

            // Set exit callback
            customer.SetExitCallback(OnCustomerReachedExit);

            // Add to queue
            _queue.AddCustomer(customer);
        }

        private void CreateCrown(Transform parent)
        {
            var crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.name = "Crown";
            crown.transform.SetParent(parent);
            crown.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            crown.transform.localScale = new Vector3(0.35f, 0.08f, 0.35f);
            Object.Destroy(crown.GetComponent<CapsuleCollider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
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
