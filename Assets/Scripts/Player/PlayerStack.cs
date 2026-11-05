using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ShopIdle.Player
{
    public class PlayerStack : MonoBehaviour
    {
        [Header("Stack Settings")]
        [SerializeField] private int _maxCapacity = 5;
        [SerializeField] private float _pizzaHeight = 0.15f;
        [SerializeField] private float _stackBaseHeight = 1.2f;

        [Header("References")]
        [SerializeField] private Transform _stackAnchor;
        [SerializeField] private GameObject _maxLabel;

        private readonly List<Transform> _pizzas = new List<Transform>();

        public int Count => _pizzas.Count;
        public bool IsFull => _pizzas.Count >= _maxCapacity;
        public int MaxCapacity => _maxCapacity;

        public void SetMaxCapacity(int newMax) { _maxCapacity = newMax; }

        private void Update()
        {
            if (_maxLabel != null)
                _maxLabel.SetActive(IsFull);
        }

        public bool TryAddPizza(Transform pizza)
        {
            if (IsFull) return false;

            _pizzas.Add(pizza);
            pizza.SetParent(_stackAnchor != null ? _stackAnchor : transform);
            SnapPizzaToSlot(pizza, _pizzas.Count - 1);
            return true;
        }

        public Transform RemoveTopPizza()
        {
            if (_pizzas.Count == 0) return null;

            int lastIndex = _pizzas.Count - 1;
            Transform pizza = _pizzas[lastIndex];
            _pizzas.RemoveAt(lastIndex);
            pizza.SetParent(null);
            return pizza;
        }

        public Vector3 GetNextSlotWorldPos()
        {
            Transform anchor = _stackAnchor != null ? _stackAnchor : transform;
            float y = _stackBaseHeight + _pizzas.Count * _pizzaHeight;
            return anchor.position + Vector3.up * y;
        }

        private void SnapPizzaToSlot(Transform pizza, int index)
        {
            float y = index * _pizzaHeight;
            pizza.localPosition = new Vector3(0f, y, 0f);
            pizza.localRotation = Quaternion.identity;
        }
    }
}
