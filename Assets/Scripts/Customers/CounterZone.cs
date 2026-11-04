using UnityEngine;

namespace ShopIdle.Customers
{
    public class CounterZone : MonoBehaviour
    {
        [Header("Serving")]
        [SerializeField] private float _serveInterval = 0.4f;
        [SerializeField] private float _flyDuration = 0.3f;
        [SerializeField] private int _moneyPerPizza = 10;

        [Header("References")]
        [SerializeField] private CustomerQueue _queue;
        [SerializeField] private CustomerSpawner _spawner;

        private Player.PlayerStack _playerInRange;
        private float _serveTimer;
        private int _currentOrderTotal;

        private void Update()
        {
            if (_playerInRange == null) return;
            if (_playerInRange.Count == 0) return;

            Customer customer = _queue.FirstCustomer;
            if (customer == null || customer.PizzasRemaining <= 0) return;

            _serveTimer += Time.deltaTime;
            if (_serveTimer < _serveInterval) return;

            _serveTimer = 0f;
            ServePizza(customer);
        }

        private void ServePizza(Customer customer)
        {
            Transform pizza = _playerInRange.RemoveTopPizza();
            if (pizza == null) return;

            // Fly pizza to customer
            Vector3 startPos = pizza.position;
            Vector3 endPos = customer.transform.position + Vector3.up * 1.2f;

            var fly = pizza.gameObject.AddComponent<Kitchen.PizzaFly>();
            fly.Init(startPos, endPos, _flyDuration, () =>
            {
                Destroy(pizza.gameObject);
            });

            customer.ReceivePizza();
            _currentOrderTotal += _moneyPerPizza;
            Debug.Log($"[Money] Served pizza! +${_moneyPerPizza}");

            // Check if order complete
            if (customer.IsFullyServed)
            {
                CompleteOrder(customer);
            }
        }

        private void CompleteOrder(Customer customer)
        {
            Debug.Log($"[Money] Order complete! Earned ${_currentOrderTotal} total.");
            _currentOrderTotal = 0;

            // Remove from queue
            _queue.RemoveFirst();

            // Send to exit
            _spawner.SendCustomerToExit(customer);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            var stack = other.GetComponent<Player.PlayerStack>();
            if (stack == null) stack = other.GetComponentInChildren<Player.PlayerStack>();
            if (stack != null)
            {
                _playerInRange = stack;
                _serveTimer = _serveInterval; // Serve immediately
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                _playerInRange = null;
        }
    }
}
