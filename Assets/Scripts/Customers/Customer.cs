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

        // Dine-in state
        private Tables.DineInTable _dineTable;
        private int _dineSeat;
        private int _dineMoneyAmount;
        private Vector3 _exitPosition;
        private float _eatTimer;
        private bool _isDining;

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

        public void AssignDineIn(Tables.DineInTable table, int seatIndex,
            int moneyAmount, Vector3 exitPos)
        {
            _dineTable = table;
            _dineSeat = seatIndex;
            _dineMoneyAmount = moneyAmount;
            _exitPosition = exitPos;
            _isDining = true;
            _eatTimer = 0f;
            MarkServed();
            SetTarget(table.GetSeatWorldPos(seatIndex));
        }

        private void Update()
        {
            if (_hasTarget)
            {
                MoveToward();
                return;
            }

            if (_isDining)
                HandleDining();
        }

        private void MoveToward()
        {
            Vector3 dir = _targetPosition - transform.position;
            dir.y = 0f;
            float dist = dir.magnitude;

            if (dist < 0.1f)
            {
                _hasTarget = false;

                if (_isServed && !_isDining)
                    _onReachedExit?.Invoke(this);
                return;
            }

            Vector3 move = dir.normalized * _moveSpeed * Time.deltaTime;
            if (move.magnitude > dist)
                move = dir;

            transform.position += move;

            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, 720f * Time.deltaTime);
            }
        }

        private void HandleDining()
        {
            _eatTimer += Time.deltaTime;
            if (_eatTimer < 5f) return;

            _isDining = false;
            if (_dineTable != null)
                _dineTable.CustomerFinished(_dineSeat, _dineMoneyAmount);
            SetTarget(_exitPosition);
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
