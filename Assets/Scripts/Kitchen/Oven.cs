using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Kitchen
{
    public class Oven : MonoBehaviour
    {
        [Header("Production")]
        [SerializeField] private float _produceInterval = 2f;
        [SerializeField] private int _maxTrayCapacity = 6;

        [Header("Tray Layout")]
        [SerializeField] private Transform _trayAnchor;
        [SerializeField] private float _pizzaHeight = 0.15f;
        [SerializeField] private float _trayBaseHeight = 1.1f;

        [Header("Pickup")]
        [SerializeField] private float _pickupInterval = 0.15f;
        [SerializeField] private float _flyDuration = 0.3f;

        [Header("Pizza Prefab")]
        [SerializeField] private GameObject _pizzaPrefab;

        private readonly List<Transform> _trayPizzas = new List<Transform>();
        private float _produceTimer;
        private float _pickupTimer;
        private Player.PlayerStack _playerInRange;

        private void Update()
        {
            ProducePizzas();
            TransferToPlayer();
        }

        private void ProducePizzas()
        {
            if (_trayPizzas.Count >= _maxTrayCapacity) return;

            _produceTimer += Time.deltaTime;
            if (_produceTimer < _produceInterval) return;

            _produceTimer = 0f;
            SpawnPizzaOnTray();
        }

        private void SpawnPizzaOnTray()
        {
            if (_pizzaPrefab == null) return;

            GameObject pizza = Instantiate(_pizzaPrefab);
            Transform t = pizza.transform;
            Transform anchor = _trayAnchor != null ? _trayAnchor : transform;
            t.SetParent(anchor);

            float y = _trayBaseHeight + _trayPizzas.Count * _pizzaHeight;
            t.localPosition = new Vector3(0f, y, 0f);
            t.localRotation = Quaternion.identity;

            _trayPizzas.Add(t);
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
                float y = _trayBaseHeight + i * _pizzaHeight;
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
