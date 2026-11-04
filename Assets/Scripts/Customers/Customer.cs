using UnityEngine;
using TMPro;

namespace ShopIdle.Customers
{
    public class Customer : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 3f;

        [Header("Order UI")]
        [SerializeField] private GameObject _speechBubble;
        [SerializeField] private TextMeshPro _orderText;

        private int _pizzasWanted;
        private int _pizzasReceived;
        private Vector3 _targetPosition;
        private bool _hasTarget;
        private bool _isServed;
        private System.Action<Customer> _onReachedExit;

        public int PizzasRemaining => _pizzasWanted - _pizzasReceived;
        public bool IsFullyServed => _pizzasReceived >= _pizzasWanted;

        public void Setup(int pizzasWanted)
        {
            _pizzasWanted = pizzasWanted;
            _pizzasReceived = 0;
            _isServed = false;
            UpdateOrderDisplay();
        }

        public void SetTarget(Vector3 target)
        {
            _targetPosition = target;
            _hasTarget = true;
        }

        public void SetExitCallback(System.Action<Customer> callback)
        {
            _onReachedExit = callback;
        }

        public void ReceivePizza()
        {
            _pizzasReceived++;
            UpdateOrderDisplay();
        }

        public void MarkServed()
        {
            _isServed = true;
            if (_speechBubble != null)
                _speechBubble.SetActive(false);
        }

        private void Update()
        {
            if (!_hasTarget) return;

            Vector3 dir = _targetPosition - transform.position;
            dir.y = 0f;
            float dist = dir.magnitude;

            if (dist < 0.1f)
            {
                _hasTarget = false;

                if (_isServed)
                {
                    _onReachedExit?.Invoke(this);
                }
                return;
            }

            Vector3 move = dir.normalized * _moveSpeed * Time.deltaTime;
            if (move.magnitude > dist)
                move = dir;

            transform.position += move;

            // Face movement direction
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, 720f * Time.deltaTime);
            }
        }

        private void UpdateOrderDisplay()
        {
            if (_orderText != null)
                _orderText.text = $"x{PizzasRemaining}";

            if (_speechBubble != null)
                _speechBubble.SetActive(PizzasRemaining > 0);
        }
    }
}
