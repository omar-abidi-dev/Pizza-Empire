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

            // Load from SaveManager instead of PlayerPrefs
            var sm = Save.SaveManager.Instance;
            if (sm != null && sm.Data != null)
                _totalMoney = sm.Data.money;
        }

        public void SetMoney(int amount)
        {
            _totalMoney = amount;
            OnMoneyChanged?.Invoke(_totalMoney);
        }

        public void AddMoney(int amount)
        {
            _totalMoney += amount;
            OnMoneyChanged?.Invoke(_totalMoney);
        }

        public int SpendUpTo(int maxAmount)
        {
            if (maxAmount <= 0 || _totalMoney <= 0) return 0;
            int spent = Mathf.Min(maxAmount, _totalMoney);
            _totalMoney = Mathf.Max(0, _totalMoney - spent);
            OnMoneyChanged?.Invoke(_totalMoney);
            return spent;
        }

        #if UNITY_EDITOR
        private void OnGUI()
        {
            if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 40), "+$500"))
                AddMoney(500);
        }
        #endif

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
