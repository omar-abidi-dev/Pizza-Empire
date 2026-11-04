using UnityEngine;
using TMPro;

namespace ShopIdle.UI
{
    public class MoneyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moneyText;

        private int _displayedMoney;
        private int _targetMoney;
        private Vector3 _originalScale;
        private float _bounceTimer;
        private const float BounceDuration = 0.2f;
        private const float BounceScale = 0.2f;
        private const int CountSpeed = 5;

        private void Start()
        {
            _originalScale = transform.localScale;

            if (Money.MoneyManager.Instance != null)
            {
                _displayedMoney = Money.MoneyManager.Instance.TotalMoney;
                _targetMoney = _displayedMoney;
                Money.MoneyManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            UpdateText();
        }

        private void OnDestroy()
        {
            if (Money.MoneyManager.Instance != null)
                Money.MoneyManager.Instance.OnMoneyChanged -= OnMoneyChanged;
        }

        private void OnMoneyChanged(int newTotal)
        {
            _targetMoney = newTotal;
            _bounceTimer = BounceDuration;
        }

        private void Update()
        {
            // Count up animation
            if (_displayedMoney < _targetMoney)
            {
                int step = Mathf.Max(1, (_targetMoney - _displayedMoney) / CountSpeed);
                _displayedMoney = Mathf.Min(_displayedMoney + step, _targetMoney);
                UpdateText();
            }

            // Bounce animation
            if (_bounceTimer > 0f)
            {
                _bounceTimer -= Time.deltaTime;
                float t = Mathf.Clamp01(_bounceTimer / BounceDuration);
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * BounceScale;
                transform.localScale = _originalScale * scale;
            }
            else
            {
                transform.localScale = _originalScale;
            }
        }

        private void UpdateText()
        {
            if (_moneyText != null)
                _moneyText.text = $"${_displayedMoney}";
        }
    }
}
