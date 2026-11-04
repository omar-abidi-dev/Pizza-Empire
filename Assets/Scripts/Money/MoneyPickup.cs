using UnityEngine;

namespace ShopIdle.Money
{
    public class MoneyPickup : MonoBehaviour
    {
        [SerializeField] private int _value = 10;

        private bool _flying;

        public int Value => _value;
        public bool IsFlying => _flying;

        public void SetValue(int value)
        {
            _value = value;
        }

        public void FlyToPlayer(Transform player, System.Action<MoneyPickup> onComplete)
        {
            if (_flying) return;
            _flying = true;

            var fly = gameObject.AddComponent<MoneyFly>();
            fly.Init(transform.position, player, 0.35f, () =>
            {
                onComplete?.Invoke(this);
            });
        }
    }
}
