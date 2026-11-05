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
        [SerializeField] private Money.MoneySpot _moneySpot;

        private Player.PlayerStack _playerInRange;
        private float _serveTimer;
        private int _currentOrderTotal;

        private bool _hasCashierWorker;
        private int _storedPizzas;
        private float _depositTimer;
        private const float DepositInterval = 0.12f;

        public void EnableCashierWorker() { _hasCashierWorker = true; }

        public void DepositFromChef(int count)
        {
            _storedPizzas += count;
        }

        private void Update()
        {
            Customer customer = _queue != null ? _queue.FirstCustomer : null;

            // Player deposits into storage when cashier is active
            if (_hasCashierWorker)
                DepositFromPlayer();

            // Auto-serve from stored pizzas (deposited by chef or player)
            if (_storedPizzas > 0)
            {
                AutoServe(customer);
            }
            else if (!_hasCashierWorker)
            {
                // Direct serve from player's hand stack
                ManualServe(customer);
            }
        }

        private void DepositFromPlayer()
        {
            if (_playerInRange == null || _playerInRange.Count == 0) return;

            _depositTimer += Time.deltaTime;
            if (_depositTimer < DepositInterval) return;
            _depositTimer = 0f;

            Transform pizza = _playerInRange.RemoveTopPizza();
            if (pizza == null) return;
            Destroy(pizza.gameObject);
            _storedPizzas++;
        }

        private void AutoServe(Customer customer)
        {
            if (_storedPizzas <= 0) return;
            if (customer == null || customer.PizzasRemaining <= 0) return;

            _serveTimer += Time.deltaTime;
            if (_serveTimer < _serveInterval) return;
            _serveTimer = 0f;

            _storedPizzas--;
            FlyPizzaToCustomer(customer);
        }

        private void ManualServe(Customer customer)
        {
            if (_playerInRange == null || _playerInRange.Count == 0) return;
            if (customer == null || customer.PizzasRemaining <= 0) return;

            _serveTimer += Time.deltaTime;
            if (_serveTimer < _serveInterval) return;

            _serveTimer = 0f;
            ServePizza(customer);
        }

        private void FlyPizzaToCustomer(Customer customer)
        {
            var pizzaObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pizzaObj.transform.localScale = new Vector3(0.4f, 0.05f, 0.4f);
            pizzaObj.transform.position = transform.position + Vector3.up;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.85f, 0.2f);
            pizzaObj.GetComponent<MeshRenderer>().material = mat;
            Destroy(pizzaObj.GetComponent<CapsuleCollider>());

            Vector3 endPos = customer.transform.position + Vector3.up * 1.2f;
            var fly = pizzaObj.AddComponent<Kitchen.PizzaFly>();
            fly.Init(pizzaObj.transform.position, endPos, _flyDuration, () =>
            {
                Destroy(pizzaObj);
            });

            customer.ReceivePizza();
            _currentOrderTotal += _moneyPerPizza;

            if (customer.IsFullyServed)
                CompleteOrder(customer);
        }

        private void ServePizza(Customer customer)
        {
            Transform pizza = _playerInRange.RemoveTopPizza();
            if (pizza == null) return;

            Vector3 startPos = pizza.position;
            Vector3 endPos = customer.transform.position + Vector3.up * 1.2f;

            var fly = pizza.gameObject.AddComponent<Kitchen.PizzaFly>();
            fly.Init(startPos, endPos, _flyDuration, () =>
            {
                Destroy(pizza.gameObject);
            });

            customer.ReceivePizza();
            _currentOrderTotal += _moneyPerPizza;

            if (customer.IsFullyServed)
                CompleteOrder(customer);
        }

        private void CompleteOrder(Customer customer)
        {
            if (_moneySpot != null && _currentOrderTotal > 0)
                _moneySpot.SpawnBills(_currentOrderTotal);

            _currentOrderTotal = 0;
            _queue.RemoveFirst();
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
                _serveTimer = _serveInterval;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                _playerInRange = null;
        }
    }
}
