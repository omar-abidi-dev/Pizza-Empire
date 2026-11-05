using UnityEngine;

namespace ShopIdle.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 12f, -9f);
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _lookAheadY = 1f;

        private void Start()
        {
            if (_target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _target = player.transform;
            }

            if (_target != null)
            {
                transform.position = _target.position + _offset;
                transform.LookAt(_target.position + Vector3.up * _lookAheadY);
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPos = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, _smoothSpeed * Time.deltaTime);
            transform.LookAt(_target.position + Vector3.up * _lookAheadY);
        }
    }
}
