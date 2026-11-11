using UnityEngine;
using System.Linq;
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

        [SerializeField] private GameObject _orderIcon;

        private int[] _order = new int[1];   // items still wanted, per food type
        private int _pizzasWanted;
        private int _pizzasReceived;
        private Vector3 _targetPosition;
        private bool _hasTarget;
        private bool _isServed;
        private bool _isVip;
        private System.Action<Customer> _onReachedExit;

        // Dine-in state
        private Tables.DineInTable _dineTable;
        private int _dineSeat;
        private int _dineMoneyAmount;
        private Vector3 _exitPosition;
        private float _eatTimer;
        private bool _isDining;

        private int _mainType;   // the food this customer ordered most (carried out when leaving)

        // Patience (for angry emoji / leaving)
        private float _waitTimer;
        private const float MaxWaitTime = 30f;
        private bool _leftAngry;

        public int PizzasRemaining => _pizzasWanted - _pizzasReceived;
        public bool IsFullyServed => _pizzasReceived >= _pizzasWanted;
        public bool IsVip => _isVip;
        public bool LeftAngry => _leftAngry;

        /// <summary>Items still wanted of a food type.</summary>
        public int Needs(int type) => type >= 0 && type < _order.Length ? _order[type] : 0;
        public int OrderTypes => _order.Length;

        /// <summary>Order <paramref name="pizzasWanted"/> items, each a random food from the menu.</summary>
        public void Setup(int pizzasWanted, bool isVip = false)
        {
            _order = new int[Kitchen.Menu.Count];
            for (int i = 0; i < pizzasWanted; i++) _order[Kitchen.Menu.RandomType()]++;
            _pizzasWanted = pizzasWanted;
            _mainType = 0;
            for (int t = 1; t < _order.Length; t++) if (_order[t] > _order[_mainType]) _mainType = t;
            _pizzasReceived = 0;
            _isServed = false;
            _isVip = isVip;
            _waitTimer = 0f;
            _leftAngry = false;
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

        public void ReceivePizza(int type = 0)
        {
            if (Needs(type) <= 0) return;
            _order[type]--;
            _pizzasReceived++;
            UpdateOrderDisplay();
        }

        public void MarkServed()
        {
            _isServed = true;
            if (_speechBubble != null)
                _speechBubble.SetActive(false);
        }

        /// <summary>Show a happy face when fully served.</summary>
        public void ShowHappyEmoji() => UI.PopBubble.Emoji(transform, UI.EmojiTextures.Happy);

        /// <summary>Wanted to eat in but every seat is taken (or there are no tables yet).</summary>
        public void ShowNoSeat() => UI.PopBubble.Text(transform, "No seat!");

        /// <summary>Hold the order in both hands while walking out (take-away).</summary>
        public void CarryFood()
        {
            var food = Kitchen.FoodCatalog.Create(_mainType);
            food.transform.SetParent(transform, false);
            food.transform.localPosition = new Vector3(0f, 1.0f, 0.42f);
            food.transform.localRotation = Quaternion.identity;
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
            {
                HandleDining();
                return;
            }

            // Patience timer (only while waiting in queue, not served)
            if (!_isServed && !_leftAngry)
            {
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= MaxWaitTime)
                {
                    LeaveAngry();
                }
            }
        }

        private void LeaveAngry()
        {
            _leftAngry = true;
            UI.PopBubble.Emoji(transform, UI.EmojiTextures.Angry);
            MarkServed();

            // Remove from queue
            foreach (var queue in CustomerQueue.All.ToArray())
                queue.RemoveCustomer(this);

            // Walk to exit
            var spawner = Object.FindAnyObjectByType<CustomerSpawner>();
            if (spawner != null)
                SetTarget(spawner.ExitPosition);
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
            ShowHappyEmoji();
            if (_dineTable != null)
                _dineTable.CustomerFinished(_dineSeat, _dineMoneyAmount);
            SetTarget(_exitPosition);
        }

        private void UpdateOrderDisplay()
        {
            if (_orderText != null)
                _orderText.text = OrderBubble.Label(_order, PizzasRemaining);
            if (_orderIcon == null && _speechBubble != null)
            {
                var icon = _speechBubble.transform.Find("PizzaIcon");
                if (icon != null) _orderIcon = icon.gameObject;
            }
            if (_orderIcon != null)
                _orderIcon.SetActive(_order.Length == 1); // single-food menus keep the icon
            if (_orderText != null && _order.Length > 1)
                OrderBubble.Fit(_speechBubble.transform, _orderText, Mathf.Max(1, _orderText.text.Split('\n').Length));

            if (_speechBubble != null)
                _speechBubble.SetActive(PizzasRemaining > 0);
        }
    }
}
