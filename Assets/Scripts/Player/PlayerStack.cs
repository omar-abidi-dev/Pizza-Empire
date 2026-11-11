using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ShopIdle.Player
{
    public class PlayerStack : MonoBehaviour
    {
        [Header("Stack Settings")]
        [SerializeField] private int _maxCapacity = 5;
        [SerializeField] private float _stackBaseHeight = 1.2f;

        [Header("References")]
        [SerializeField] private Transform _stackAnchor;
        [SerializeField] private GameObject _maxLabel;

        private readonly List<Transform> _pizzas = new List<Transform>();
        private float _wobblePhase;
        private Vector3 _lastPos;

        public int Count => _pizzas.Count;
        public bool IsFull => _pizzas.Count >= _maxCapacity;
        public int MaxCapacity => _maxCapacity;

        public void SetMaxCapacity(int newMax) { _maxCapacity = newMax; }

        private void Update()
        {
            if (_maxLabel != null)
                _maxLabel.SetActive(IsFull);

            // Pizza wobble while moving
            if (_pizzas.Count > 0)
            {
                Vector3 delta = transform.position - _lastPos;
                float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.001f);
                _lastPos = transform.position;

                if (speed > 0.5f)
                {
                    _wobblePhase += Time.deltaTime * 8f;
                    float wobble = Mathf.Sin(_wobblePhase) * 3f * Mathf.Clamp01(speed / 5f);
                    for (int i = 0; i < _pizzas.Count; i++)
                    {
                        if (_pizzas[i] != null)
                        {
                            float layerFactor = (i + 1f) / _pizzas.Count;
                            _pizzas[i].localRotation = Quaternion.Euler(
                                wobble * layerFactor, 0f, wobble * 0.5f * layerFactor);
                        }
                    }
                }
                else
                {
                    _wobblePhase = 0f;
                    for (int i = 0; i < _pizzas.Count; i++)
                    {
                        if (_pizzas[i] != null)
                            _pizzas[i].localRotation = Quaternion.Lerp(
                                _pizzas[i].localRotation, Quaternion.identity, 10f * Time.deltaTime);
                    }
                }
            }
        }

        public bool TryAddPizza(Transform pizza)
        {
            if (IsFull) return false;

            _pizzas.Add(pizza);
            pizza.SetParent(_stackAnchor != null ? _stackAnchor : transform);
            SnapPizzaToSlot(pizza, _pizzas.Count - 1);

            Core.AudioManager.Instance?.Play(Core.SoundType.PizzaPickup, 0.7f);

            var pc = GetComponent<PlayerController>();
            if (pc != null) pc.TriggerPickupBounce();

            return true;
        }

        public Transform RemoveTopPizza() => RemoveAt(_pizzas.Count - 1);

        /// <summary>Remove the highest item of <paramref name="type"/> (null if none is carried).</summary>
        public Transform RemoveTopOfType(int type)
        {
            for (int i = _pizzas.Count - 1; i >= 0; i--)
                if (Kitchen.FoodItem.TypeOf(_pizzas[i]) == type) return RemoveAt(i);
            return null;
        }

        public int CountOfType(int type)
        {
            int n = 0;
            foreach (var p in _pizzas) if (Kitchen.FoodItem.TypeOf(p) == type) n++;
            return n;
        }

        private Transform RemoveAt(int index)
        {
            if (index < 0 || index >= _pizzas.Count) return null;
            Transform pizza = _pizzas[index];
            _pizzas.RemoveAt(index);
            pizza.SetParent(null);
            for (int i = index; i < _pizzas.Count; i++) SnapPizzaToSlot(_pizzas[i], i);

            Core.AudioManager.Instance?.Play(Core.SoundType.PizzaDrop, 0.5f);
            return pizza;
        }

        public Vector3 GetNextSlotWorldPos()
        {
            Transform anchor = _stackAnchor != null ? _stackAnchor : transform;
            float y = _stackBaseHeight + HeightBelow(_pizzas.Count);
            return anchor.position + Vector3.up * y;
        }

        /// <summary>Total height of the items under slot <paramref name="index"/> (mixed foods differ).</summary>
        private float HeightBelow(int index)
        {
            float y = 0f;
            for (int i = 0; i < index && i < _pizzas.Count; i++)
                y += Kitchen.FoodCatalog.StackHeight(Kitchen.FoodItem.TypeOf(_pizzas[i]));
            return y;
        }

        private void SnapPizzaToSlot(Transform pizza, int index)
        {
            pizza.localPosition = new Vector3(0f, HeightBelow(index), 0f);
            pizza.localRotation = Quaternion.identity;
        }
    }
}
