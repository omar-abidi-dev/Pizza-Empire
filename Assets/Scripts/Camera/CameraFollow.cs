using UnityEngine;

namespace ShopIdle.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, -10f);
        [SerializeField] private float _smoothSpeed = 8f;

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPos = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, _smoothSpeed * Time.deltaTime);
        }
    }
}
