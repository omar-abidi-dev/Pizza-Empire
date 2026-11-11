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
        private bool _isVip;
        private System.Action<Customer> _onReachedExit;

        // Dine-in state
        private Tables.DineInTable _dineTable;
        private int _dineSeat;
        private int _dineMoneyAmount;
        private Vector3 _exitPosition;
        private float _eatTimer;
        private bool _isDining;

        // Emoji reaction
        private GameObject _emojiObj;
        private float _emojiTimer;
        private const float EmojiDuration = 1.5f;

        // Patience (for angry emoji / leaving)
        private float _waitTimer;
        private const float MaxWaitTime = 30f;
        private bool _leftAngry;

        public int PizzasRemaining => _pizzasWanted - _pizzasReceived;
        public bool IsFullyServed => _pizzasReceived >= _pizzasWanted;
        public bool IsVip => _isVip;
        public bool LeftAngry => _leftAngry;

        public void Setup(int pizzasWanted, bool isVip = false)
        {
            _pizzasWanted = pizzasWanted;
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

        /// <summary>Show a happy emoji when fully served.</summary>
        public void ShowHappyEmoji()
        {
            ShowEmoji("\u2764", new Color(1f, 0.3f, 0.3f)); // heart
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
            // Emoji lifetime
            if (_emojiObj != null)
            {
                _emojiTimer -= Time.deltaTime;
                if (_emojiTimer <= 0f)
                {
                    Destroy(_emojiObj);
                    _emojiObj = null;
                }
            }

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
            ShowEmoji("\u0021", new Color(1f, 0.2f, 0.1f)); // exclamation
            MarkServed();

            // Remove from queue
            var queue = Object.FindAnyObjectByType<CustomerQueue>();
            if (queue != null)
                queue.RemoveCustomer(this);

            // Walk to exit
            var spawner = Object.FindAnyObjectByType<CustomerSpawner>();
            if (spawner != null)
                SetTarget(spawner.ExitPosition);
        }

        private void ShowEmoji(string emoji, Color color)
        {
            if (_emojiObj != null) Destroy(_emojiObj);

            _emojiObj = new GameObject("Emoji");
            _emojiObj.transform.SetParent(transform);
            _emojiObj.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            _emojiObj.AddComponent<UI.Billboard>();

            var tmp = _emojiObj.AddComponent<TextMeshPro>();
            tmp.text = emoji;
            tmp.fontSize = 8f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.rectTransform.sizeDelta = new Vector2(2f, 2f);

            _emojiTimer = EmojiDuration;
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
