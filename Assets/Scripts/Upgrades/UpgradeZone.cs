using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Upgrades
{
    /// <summary>
    /// Stand-on-to-pay floor pad. Drains money while the player stands on it and fires
    /// <see cref="OnPurchased"/> when fully paid. Visuals live in <see cref="BuyPadVisual"/>.
    /// </summary>
    public class UpgradeZone : MonoBehaviour
    {
        [SerializeField] private int _totalCost;
        [SerializeField] private float _drainInterval = 0.1f;
        [SerializeField] private int _drainPerTick = 5;

        /// <summary>Every unpaid pad (the guide arrow picks one).</summary>
        public static readonly List<UpgradeZone> All = new();

        private int _remaining;
        private Transform _playerInRange;
        private float _drainTimer;
        private bool _purchased;
        private bool _canAfford;
        private BuyPadVisual _visual;

        public event System.Action OnPurchased;

        public int TotalCost => _totalCost;
        public int Remaining => _remaining;

        public void Init(string displayName, int cost, bool isNextRestaurant = false)
        {
            _totalCost = cost;
            _remaining = cost;

            _visual = gameObject.AddComponent<BuyPadVisual>();
            _visual.Build(displayName, cost, isNextRestaurant);
            _visual.SetAffordable(false);

            var col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(BuyPadVisual.Size, 2f, BuyPadVisual.Size);
            col.center = new Vector3(0f, 1f, 0f);
        }

        private void OnEnable() { All.Add(this); }
        private void OnDisable() { All.Remove(this); }

        /// <summary>Restore partial payment progress from save data.</summary>
        public void SetPartialSpent(int spent)
        {
            _remaining = Mathf.Max(0, _totalCost - spent);
            Refresh();
        }

        /// <summary>Change the price, keeping what was already paid.</summary>
        public void Reprice(int newCost)
        {
            if (_purchased) return;
            int spent = _totalCost - _remaining;
            _totalCost = newCost;
            _remaining = Mathf.Max(1, newCost - spent);
            Refresh();
        }

        private void Refresh() { if (_visual != null) _visual.SetProgress(_remaining, _totalCost); }

        private void Update()
        {
            if (_purchased) return;

            var mm = Money.MoneyManager.Instance;
            bool affordable = mm != null && mm.TotalMoney > 0;
            if (affordable != _canAfford)
            {
                _canAfford = affordable;
                _visual.SetAffordable(affordable);
            }
            if (_canAfford) _visual.Pulse(Time.deltaTime);

            if (_playerInRange == null || _remaining <= 0 || !affordable) return;

            _drainTimer += Time.deltaTime;
            if (_drainTimer < _drainInterval) return;
            _drainTimer = 0f;

            // Pay faster on big prices so a pad never takes more than ~3 s.
            int perTick = Mathf.Max(_drainPerTick, Mathf.CeilToInt(_totalCost / 30f));
            int spent = mm.SpendUpTo(Mathf.Min(perTick, _remaining));
            if (spent <= 0) return;

            _remaining = Mathf.Max(0, _remaining - spent);
            SpawnMoneyFlyVisual();
            Refresh();

            if (_remaining <= 0)
                StartCoroutine(PurchaseSequence());
        }

        private void SpawnMoneyFlyVisual()
        {
            Vector3 start = _playerInRange.position + Vector3.up * 1.5f;
            var bill = Core.VisualFactory.CreateCashVisual("FlyBill", start, 0f);
            if (bill == null) return;
            bill.AddComponent<Kitchen.PizzaFly>().Init(start, transform.position + Vector3.up * 0.2f,
                0.25f, () => Destroy(bill));
        }

        private IEnumerator PurchaseSequence()
        {
            _purchased = true;

            Core.AudioManager.Instance?.Play(Core.SoundType.Whoosh);
            Core.AudioManager.Instance?.Play(Core.SoundType.Fanfare, 0.8f);
            var juice = Core.JuiceManager.Instance;
            if (juice != null)
            {
                juice.SpawnSparkle(transform.position + Vector3.up, new Color(1f, 0.85f, 0.1f), 20);
                juice.ShakeCamera(0.15f);
            }

            Vector3 originalScale = transform.localScale;
            for (float t = 0f; t < 0.2f; t += Time.deltaTime)
            {
                transform.localScale = originalScale * (1f + Mathf.Sin(t / 0.2f * Mathf.PI) * 0.3f);
                yield return null;
            }

            OnPurchased?.Invoke();
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = other.transform;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = null;
        }
    }
}
