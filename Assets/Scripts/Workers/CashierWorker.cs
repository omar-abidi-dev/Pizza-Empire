using UnityEngine;

namespace ShopIdle.Workers
{
    public class CashierWorker : MonoBehaviour
    {
        private float _bobTimer;

        private void Update()
        {
            // Gentle idle bob animation
            _bobTimer += Time.deltaTime;
            float y = 0.5f + Mathf.Sin(_bobTimer * 2f) * 0.05f;
            var pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }
    }
}
