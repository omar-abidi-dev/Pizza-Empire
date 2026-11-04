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

        private float _spawnTimer;

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

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer < _spawnInterval) return;

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

            // Random order size
            int pizzas = Random.Range(_minPizzas, _maxPizzas + 1);
            customer.Setup(pizzas);

            // Random color
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = obj.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                var mat = renderer.material;
                mat.color = CustomerColors[Random.Range(0, CustomerColors.Length)];
            }

            // Set exit callback
            customer.SetExitCallback(OnCustomerReachedExit);

            // Add to queue
            _queue.AddCustomer(customer);
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
