using UnityEngine;
using TMPro;

namespace ShopIdle.Core
{
    public class FloatingText : MonoBehaviour
    {
        private const float Duration = 0.8f;
        private const float RiseSpeed = 2f;

        private float _timer;
        private TextMeshPro _tmp;
        private Color _startColor;

        private void Start()
        {
            _tmp = GetComponent<TextMeshPro>();
            if (_tmp != null)
                _startColor = _tmp.color;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            transform.position += Vector3.up * (RiseSpeed * Time.deltaTime);

            if (_tmp != null)
            {
                float alpha = 1f - (_timer / Duration);
                _tmp.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);
            }

            if (_timer >= Duration)
                Destroy(gameObject);
        }
    }
}
