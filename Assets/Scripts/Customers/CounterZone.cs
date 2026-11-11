using UnityEngine;

namespace ShopIdle.Customers
{
    public class CounterZone : MonoBehaviour
    {
        [Header("Serving")]
        [SerializeField] private float _serveInterval = 0.4f;
        [SerializeField] private float _flyDuration = 0.3f;

        /// <summary>Every active counter (workers pick the nearest).</summary>
        public static readonly System.Collections.Generic.List<CounterZone> All = new();

        [Header("References")]
        [SerializeField] private CustomerQueue _queue;
        [SerializeField] private CustomerSpawner _spawner;
        [SerializeField] private Money.MoneySpot _moneySpot;

        private Player.PlayerStack _playerInRange;
        private float _serveTimer;
        private int _currentOrderTotal;

        private const float DineInChance = 0.4f;

        private bool _hasCashierWorker;
        private readonly System.Collections.Generic.Dictionary<int, int> _stored = new();
        private int _storedPizzas;  // total over all types
        private float _depositTimer;
        private const float DepositInterval = 0.12f;

        public bool HasCashier => _hasCashierWorker;
        public void EnableCashierWorker() { _hasCashierWorker = true; }

        /// <summary>Wire a runtime-built counter (plot content).</summary>
        public void Init(CustomerQueue queue, CustomerSpawner spawner, Money.MoneySpot moneySpot)
        {
            _queue = queue;
            _spawner = spawner;
            _moneySpot = moneySpot;
        }

        private void OnEnable() { All.Add(this); }
        private void OnDisable() { All.Remove(this); }

        public void DepositFromChef(int count, int type = 0)
        {
            _stored.TryGetValue(type, out int n);
            _stored[type] = n + count;
            _storedPizzas += count;
        }

        private int StoredOf(int type) => _stored.TryGetValue(type, out int n) ? n : 0;

        /// <summary>First food type the customer still needs that <paramref name="available"/> can supply.</summary>
        private static int MatchType(Customer customer, System.Func<int, bool> available)
        {
            for (int t = 0; t < customer.OrderTypes; t++)
                if (customer.Needs(t) > 0 && available(t)) return t;
            return -1;
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
            DepositFromChef(1, Kitchen.FoodItem.TypeOf(pizza));
            Destroy(pizza.gameObject);
        }

        private void AutoServe(Customer customer)
        {
            if (_storedPizzas <= 0) return;
            if (customer == null || customer.PizzasRemaining <= 0) return;
            int type = MatchType(customer, t => StoredOf(t) > 0);
            if (type < 0) { ManualServe(customer); return; }

            _serveTimer += Time.deltaTime;
            if (_serveTimer < _serveInterval) return;
            _serveTimer = 0f;

            _stored[type]--;
            _storedPizzas--;
            FlyPizzaToCustomer(customer, type);
        }

        private void ManualServe(Customer customer)
        {
            if (_playerInRange == null || _playerInRange.Count == 0) return;
            if (customer == null || customer.PizzasRemaining <= 0) return;
            var stack = _playerInRange;
            int type = MatchType(customer, t => stack.CountOfType(t) > 0);
            if (type < 0) return;

            _serveTimer += Time.deltaTime;
            if (_serveTimer < _serveInterval) return;

            _serveTimer = 0f;
            ServePizza(customer, type);
        }

        private void FlyPizzaToCustomer(Customer customer, int type)
        {
            var pizzaObj = Kitchen.FoodCatalog.Create(type);
            pizzaObj.transform.position = transform.position + Vector3.up;

            Vector3 endPos = customer.transform.position + Vector3.up * 1.2f;
            var fly = pizzaObj.AddComponent<Kitchen.PizzaFly>();
            fly.Init(pizzaObj.transform.position, endPos, _flyDuration, () =>
            {
                Destroy(pizzaObj);
            });

            customer.ReceivePizza(type);
            _currentOrderTotal += GetItemPrice(customer, type);

            if (customer.IsFullyServed)
                CompleteOrder(customer);
        }

        private void ServePizza(Customer customer, int type)
        {
            Transform pizza = _playerInRange.RemoveTopOfType(type);
            if (pizza == null) return;

            Vector3 startPos = pizza.position;
            Vector3 endPos = customer.transform.position + Vector3.up * 1.2f;

            var fly = pizza.gameObject.AddComponent<Kitchen.PizzaFly>();
            fly.Init(startPos, endPos, _flyDuration, () =>
            {
                Destroy(pizza.gameObject);
            });

            customer.ReceivePizza(type);
            _currentOrderTotal += GetItemPrice(customer, type);

            if (customer.IsFullyServed)
                CompleteOrder(customer);
        }

        /// <summary>Calculate item price with VIP (x5) and rush hour (x2) multipliers.</summary>
        private static int GetItemPrice(Customer customer, int type)
        {
            int price = Kitchen.FoodCatalog.Price(type);

            // VIP pays x5
            if (customer.IsVip)
                price *= 5;

            // Rush hour pays x2
            var rush = Core.RushHourManager.Instance;
            if (rush != null && rush.IsRushing)
                price = Mathf.RoundToInt(price * rush.PayMultiplier);

            // Prestige income multiplier
            var pm = Core.PrestigeManager.Instance;
            if (pm != null)
                price = Mathf.RoundToInt(price * pm.IncomeMultiplier);

            return price;
        }

        private void CompleteOrder(Customer customer)
        {
            int earned = _currentOrderTotal;
            _currentOrderTotal = 0;
            _queue.RemoveFirst();

            // Quest tracking
            var qm = Core.DailyQuestManager.Instance;
            qm?.TrackServeCustomer();
            qm?.TrackMoneyCollected(earned);
            if (customer.IsVip) qm?.TrackVIPServed();

            // Emoji reaction
            customer.ShowHappyEmoji();

            // Juice: sound + floating text + sparkle
            Core.AudioManager.Instance?.Play(Core.SoundType.ChaChing);
            var juice = Core.JuiceManager.Instance;
            if (juice != null)
            {
                Vector3 textPos = customer.transform.position + Vector3.up * 2.5f;
                Color textColor = customer.IsVip
                    ? new Color(1f, 0.85f, 0.2f)
                    : new Color(0.2f, 0.9f, 0.3f);
                juice.SpawnFloatingText($"+${earned}", textPos, textColor);
                juice.SpawnCoinBurst(customer.transform.position + Vector3.up * 1f);
            }

            // 40% want to eat in: take a free seat, or say so and take it away.
            if (Random.value < DineInChance)
            {
                var tm = Tables.TableManager.Instance;
                int seatIndex = -1;
                var table = tm != null ? tm.TryReserveSeat(out seatIndex) : null;
                if (table != null)
                {
                    customer.AssignDineIn(table, seatIndex, earned, _spawner.ExitPosition);
                    return;
                }
                customer.ShowNoSeat();
            }
            customer.CarryFood();

            // Normal flow: money at counter, exit
            if (_moneySpot != null && earned > 0)
                _moneySpot.SpawnBills(earned);
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
