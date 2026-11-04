using UnityEngine;

namespace ShopIdle.Money
{
    public class MoneyManager : MonoBehaviour
    {
        private static MoneyManager _instance;
        public static MoneyManager Instance => _instance;

        [SerializeField] private int _totalMoney;

        public int TotalMoney => _totalMoney;
        public event System.Action<int> OnMoneyChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _totalMoney = PlayerPrefs.GetInt("TotalMoney", 0);
        }

        public void AddMoney(int amount)
        {
            _totalMoney += amount;
            PlayerPrefs.SetInt("TotalMoney", _totalMoney);
            OnMoneyChanged?.Invoke(_totalMoney);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
