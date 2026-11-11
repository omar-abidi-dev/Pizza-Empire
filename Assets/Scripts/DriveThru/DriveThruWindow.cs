using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ShopIdle.DriveThru
{
    public class DriveThruWindow : MonoBehaviour
    {
        public static DriveThruWindow Instance { get; private set; }

        private const float SpawnInterval = 6f;
        private const int MaxCars = 3;
        private const int DefaultMoneyPerPizza = 15;
        private static int MoneyPerItem
        {
            get
            {
                var rm = Core.RestaurantManager.Instance;
                return rm != null ? rm.CurrentConfig.driveThruMoneyPerItem : DefaultMoneyPerPizza;
            }
        }
        private const float CarSpeed = 4f;
        private const float ServeInterval = 0.4f;
        private const float DepositInterval = 0.12f;
        private const float QueueSpacing = 3.5f;

        private class Car
        {
            public GameObject Obj;
            public int PizzasWanted;
            public int PizzasReceived;
            public bool Served;
            public GameObject Bubble;
            public TextMeshPro BubbleText;
        }

        private readonly List<Car> _cars = new();
        private float _spawnTimer;
        private int _storedPizzas;
        private float _depositTimer;
        private float _serveTimer;
        private Player.PlayerStack _playerInRange;
        private Money.MoneySpot _moneySpot;

        private float _roadX;
        private float _windowZ;
        private float _spawnZ;
        private float _exitZ;

        private static readonly Color[] CarColors =
        {
            new(0.92f, 0.55f, 0.55f), // soft red
            new(0.55f, 0.65f, 0.92f), // soft blue
            new(0.95f, 0.95f, 0.92f), // cream
            new(0.65f, 0.88f, 0.65f), // mint
            new(0.92f, 0.82f, 0.55f), // butter
            new(0.85f, 0.65f, 0.88f), // lavender
        };

        public Vector3 ServePoint => transform.position;
        public bool HasWaitingCar => _cars.Count > 0 && !_cars[0].Served;

        public void DepositFromWorker(int count) { _storedPizzas += count; }

        private void Awake()
        {
            Instance = this;
            _roadX = transform.position.x + 2f;
            _windowZ = transform.position.z;
            _spawnZ = _windowZ + 16f;
            _exitZ = _windowZ - 16f;
        }

        private void Start()
        {
            // Trigger zone for player serving
            var col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 2f, 3f);
            col.center = new Vector3(0f, 1f, 0f);

            // Money spot for drive-thru payments
            var spotObj = new GameObject("DriveThruMoneySpot");
            spotObj.transform.SetParent(transform);
            spotObj.transform.localPosition = new Vector3(-1f, 0f, 0f);
            var spotCol = spotObj.AddComponent<BoxCollider>();
            spotCol.isTrigger = true;
            spotCol.size = new Vector3(3f, 2f, 3f);
            spotCol.center = new Vector3(0f, 0.5f, 0f);
            _moneySpot = spotObj.AddComponent<Money.MoneySpot>();
        }

        private void Update()
        {
            SpawnCars();
            MoveCars();
            if (_playerInRange != null && _playerInRange.Count > 0)
                DepositFromPlayer();
            if (_storedPizzas > 0)
                ServeFirstCar();
        }

        private void SpawnCars()
        {
            if (_cars.Count >= MaxCars) return;
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer < SpawnInterval) return;
            _spawnTimer = 0f;

            var car = new Car
            {
                PizzasWanted = Random.Range(1, 3),
                Obj = CreateCarVisual()
            };
            car.Obj.transform.position = new Vector3(_roadX, 0.4f, _spawnZ);
            car.Obj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            CreateBubble(car);
            _cars.Add(car);
        }

        private void MoveCars()
        {
            for (int i = _cars.Count - 1; i >= 0; i--)
            {
                var car = _cars[i];
                float targetZ = car.Served ? _exitZ : _windowZ + i * QueueSpacing;

                Vector3 pos = car.Obj.transform.position;
                float diff = targetZ - pos.z;
                if (Mathf.Abs(diff) > 0.1f)
                {
                    float step = CarSpeed * Time.deltaTime;
                    pos.z += Mathf.Sign(diff) * Mathf.Min(step, Mathf.Abs(diff));
                    car.Obj.transform.position = pos;
                }

                if (car.Served && pos.z <= _exitZ + 0.5f)
                {
                    Object.Destroy(car.Obj);
                    _cars.RemoveAt(i);
                }
            }
        }

        private void DepositFromPlayer()
        {
            _depositTimer += Time.deltaTime;
            if (_depositTimer < DepositInterval) return;
            _depositTimer = 0f;

            Transform pizza = _playerInRange.RemoveTopPizza();
            if (pizza == null) return;
            Object.Destroy(pizza.gameObject);
            _storedPizzas++;
        }

        private void ServeFirstCar()
        {
            if (_cars.Count == 0 || _cars[0].Served) return;
            var car = _cars[0];
            if (Mathf.Abs(car.Obj.transform.position.z - _windowZ) > 0.5f) return;
            if (car.PizzasReceived >= car.PizzasWanted) return;

            _serveTimer += Time.deltaTime;
            if (_serveTimer < ServeInterval) return;
            _serveTimer = 0f;

            _storedPizzas--;
            car.PizzasReceived++;
            FlyPizzaToCar(car);
            UpdateBubble(car);

            if (car.PizzasReceived >= car.PizzasWanted)
            {
                car.Served = true;
                if (car.Bubble != null) car.Bubble.SetActive(false);
                int price = MoneyPerItem;

                // Rush hour pays x2
                var rush = Core.RushHourManager.Instance;
                if (rush != null && rush.IsRushing)
                    price = Mathf.RoundToInt(price * rush.PayMultiplier);

                // Prestige income multiplier
                var pm = Core.PrestigeManager.Instance;
                if (pm != null)
                    price = Mathf.RoundToInt(price * pm.IncomeMultiplier);

                int earned = car.PizzasWanted * price;
                if (_moneySpot != null && earned > 0)
                    _moneySpot.SpawnBills(earned);

                // Quest tracking
                Core.DailyQuestManager.Instance?.TrackServeCustomer();
                Core.DailyQuestManager.Instance?.TrackMoneyCollected(earned);
            }
        }

        private void FlyPizzaToCar(Car car)
        {
            var obj = Core.RestaurantManager.CreateFoodVisual();
            if (obj == null)
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                obj.transform.localScale = new Vector3(0.4f, 0.05f, 0.4f);
                Object.Destroy(obj.GetComponent<CapsuleCollider>());
            }
            obj.transform.position = transform.position + Vector3.up;

            Vector3 end = car.Obj.transform.position + Vector3.up;
            var fly = obj.AddComponent<Kitchen.PizzaFly>();
            fly.Init(obj.transform.position, end, 0.3f, () => Object.Destroy(obj));
        }

        private GameObject CreateCarVisual()
        {
            var car = new GameObject("DriveThruCar");
            Color color = CarColors[Random.Range(0, CarColors.Length)];
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(car.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.5f, 0.6f, 2.5f);
            Object.Destroy(body.GetComponent<BoxCollider>());
            body.GetComponent<MeshRenderer>().material = mat;

            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(car.transform);
            cabin.transform.localPosition = new Vector3(0f, 0.45f, -0.2f);
            cabin.transform.localScale = new Vector3(1.3f, 0.45f, 1.2f);
            Object.Destroy(cabin.GetComponent<BoxCollider>());
            cabin.GetComponent<MeshRenderer>().material = mat;

            return car;
        }

        private void CreateBubble(Car car)
        {
            var bubble = new GameObject("Bubble");
            bubble.transform.SetParent(car.Obj.transform);
            bubble.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            var bg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bg.transform.SetParent(bubble.transform);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localScale = new Vector3(0.6f, 0.6f, 0.15f);
            Object.Destroy(bg.GetComponent<SphereCollider>());
            var bgMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bgMat.color = Color.white;
            bg.GetComponent<MeshRenderer>().material = bgMat;

            var textObj = new GameObject("OrderText");
            textObj.transform.SetParent(bubble.transform);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.08f);
            var tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = $"x{car.PizzasWanted}";
            tmp.fontSize = 3f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            textObj.AddComponent<UI.Billboard>();

            car.Bubble = bubble;
            car.BubbleText = tmp;
        }

        private void UpdateBubble(Car car)
        {
            int remaining = car.PizzasWanted - car.PizzasReceived;
            if (car.BubbleText != null)
                car.BubbleText.text = $"x{remaining}";
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            var stack = other.GetComponent<Player.PlayerStack>();
            if (stack == null) stack = other.GetComponentInChildren<Player.PlayerStack>();
            if (stack != null) _playerInRange = stack;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = null;
        }
    }
}
