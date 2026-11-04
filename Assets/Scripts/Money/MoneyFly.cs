using UnityEngine;

namespace ShopIdle.Money
{
    public class MoneyFly : MonoBehaviour
    {
        private Vector3 _start;
        private Transform _target;
        private float _duration;
        private float _elapsed;
        private System.Action _onComplete;
        private bool _initialized;
        private Vector3 _originalScale;

        public void Init(Vector3 start, Transform target, float duration, System.Action onComplete)
        {
            _start = start;
            _target = target;
            _duration = Mathf.Max(duration, 0.05f);
            _onComplete = onComplete;
            _initialized = true;
            _originalScale = transform.localScale;
            transform.position = start;
        }

        private void Update()
        {
            if (!_initialized) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);

            Vector3 endPos = _target != null
                ? _target.position + Vector3.up * 1.5f
                : _start + Vector3.up * 2f;

            Vector3 pos = Vector3.Lerp(_start, endPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * 1.2f;
            transform.position = pos;

            // Shrink as it approaches player
            float scale = Mathf.Lerp(1f, 0.3f, t);
            transform.localScale = _originalScale * scale;

            if (t >= 1f)
            {
                _onComplete?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
