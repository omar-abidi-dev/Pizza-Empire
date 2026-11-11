using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Kitchen
{
    public class Oven : MonoBehaviour
    {
        [Header("Production")]
        [SerializeField] private float _produceInterval = 2f;
        [SerializeField] private int _maxTrayCapacity = 20;

        [Header("Tray Layout")]
        [SerializeField] private Transform _trayAnchor;
                [SerializeField] private float _trayBaseHeight = 1.1f;

        [Header("Pickup")]
        [SerializeField] private float _pickupInterval = 0.15f;
        [SerializeField] private float _flyDuration = 0.3f;

        [Header("Pizza Prefab")]
        [SerializeField] private GameObject _pizzaPrefab;

        [Header("Food")]
        [SerializeField] private int _foodType;

        public GameObject PizzaPrefab => _pizzaPrefab;
        public int FoodType => _foodType;
        private float StackStep => FoodCatalog.StackHeight(_foodType);
        private int Capacity => FoodCatalog.TrayCapacity(_foodType, _maxTrayCapacity);
        public float ProduceInterval => _produceInterval;
        public int TrayCount => _trayPizzas.Count;
        public bool IsFull => _trayPizzas.Count >= Capacity;

        /// <summary>World position just above the top item of the tray stack.</summary>
        public Vector3 StackTopWorld
        {
            get
            {
                Transform anchor = _trayAnchor != null ? _trayAnchor : transform;
                return anchor.TransformPoint(new Vector3(0f, _trayBaseHeight + _trayPizzas.Count * StackStep, 0f));
            }
        }

        public void SetPizzaPrefab(GameObject prefab) { _pizzaPrefab = prefab; }
        public void SetTrayAnchor(Transform anchor) { _trayAnchor = anchor; }
        public void SetProduceInterval(float interval) { _produceInterval = interval; }
        public void SetFoodType(int type) { _foodType = type; }

        /// <summary>Move the tray anchor so the first item rests on a surface at <paramref name="surfaceWorldY"/>.</summary>
        public void AlignTraySurface(float surfaceWorldY)
        {
            if (_trayAnchor == null)
            {
                _trayAnchor = new GameObject("TrayAnchor").transform;
                _trayAnchor.SetParent(transform, false);
            }
            Vector3 p = _trayAnchor.position;
            _trayAnchor.position = new Vector3(p.x, surfaceWorldY - _trayBaseHeight * _trayAnchor.lossyScale.y, p.z);
        }

        public Transform TakeTopPizza()
        {
            if (_trayPizzas.Count == 0) return null;
            int last = _trayPizzas.Count - 1;
            Transform pizza = _trayPizzas[last];
            _trayPizzas.RemoveAt(last);
            RestackTray();
            pizza.SetParent(null);
            return pizza;
        }

        private readonly List<Transform> _trayPizzas = new List<Transform>();
        private float _produceTimer;
        private float _pickupTimer;
        private Player.PlayerStack _playerInRange;

        private void Start()
        {
            if (GetComponent<MaxTag>() == null) gameObject.AddComponent<MaxTag>();
        }

        private void Update()
        {
            ProducePizzas();
            TransferToPlayer();
        }

        private void ProducePizzas()
        {
            if (_trayPizzas.Count >= Capacity) return;

            _produceTimer += Time.deltaTime;
            if (_produceTimer < _produceInterval) return;

            _produceTimer = 0f;
            SpawnPizzaOnTray();
        }

        private void SpawnPizzaOnTray()
        {
            GameObject pizza = FoodCatalog.Create(_foodType);
            if (pizza == null && _pizzaPrefab != null)
                pizza = Instantiate(_pizzaPrefab);
            if (pizza == null) return;
            Transform t = pizza.transform;
            Transform anchor = _trayAnchor != null ? _trayAnchor : transform;
            t.SetParent(anchor);

            float y = _trayBaseHeight + _trayPizzas.Count * StackStep;
            t.localPosition = new Vector3(0f, y, 0f);
            t.localRotation = Quaternion.identity;

            _trayPizzas.Add(t);

            // Steam puff above oven
            Core.JuiceManager.Instance?.SpawnSteamPuff(
                transform.position + Vector3.up * 1.5f);
        }

        private void TransferToPlayer()
        {
            if (_playerInRange == null || _playerInRange.IsFull || _trayPizzas.Count == 0)
                return;

            _pickupTimer += Time.deltaTime;
            if (_pickupTimer < _pickupInterval) return;

            _pickupTimer = 0f;

            // Take top pizza from tray
            int lastIndex = _trayPizzas.Count - 1;
            Transform pizza = _trayPizzas[lastIndex];
            _trayPizzas.RemoveAt(lastIndex);

            // Restack remaining tray pizzas
            RestackTray();

            // Fly to player
            Vector3 startPos = pizza.position;
            Vector3 endPos = _playerInRange.GetNextSlotWorldPos();
            var playerStack = _playerInRange;

            pizza.SetParent(null);
            var fly = pizza.gameObject.AddComponent<Kitchen.PizzaFly>();
            fly.Init(startPos, endPos, _flyDuration, () =>
            {
                if (playerStack != null)
                    playerStack.TryAddPizza(pizza);
            });
        }

        private void RestackTray()
        {
            Transform anchor = _trayAnchor != null ? _trayAnchor : transform;
            for (int i = 0; i < _trayPizzas.Count; i++)
            {
                float y = _trayBaseHeight + i * StackStep;
                _trayPizzas[i].localPosition = new Vector3(0f, y, 0f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var stack = other.GetComponent<Player.PlayerStack>();
                if (stack == null) stack = other.GetComponentInChildren<Player.PlayerStack>();
                if (stack != null)
                {
                    _playerInRange = stack;
                    _pickupTimer = _pickupInterval; // pick up immediately
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                _playerInRange = null;
        }
    }
}
