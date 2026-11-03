using UnityEngine;

namespace ShopIdle.Kitchen
{
    public class PizzaFly : MonoBehaviour
    {
        private Vector3 _start;
        private Vector3 _end;
        private float _duration;
        private float _elapsed;
        private System.Action _onComplete;
        private bool _initialized;

        public void Init(Vector3 start, Vector3 end, float duration, System.Action onComplete)
        {
            _start = start;
            _end = end;
            _duration = Mathf.Max(duration, 0.05f);
            _onComplete = onComplete;
            _initialized = true;
            _elapsed = 0f;
            transform.position = start;
        }

        private void Update()
        {
            if (!_initialized) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);

            // Arc motion
            Vector3 pos = Vector3.Lerp(_start, _end, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * 0.8f;
            transform.position = pos;

            if (t >= 1f)
            {
                _onComplete?.Invoke();
                Destroy(this); // remove component, keep GameObject
            }
        }
    }
}
