using UnityEngine;

namespace ShopIdle.CameraSystem
{
    /// <summary>
    /// Orthographic isometric camera (35° X, 45° Y) that smoothly follows
    /// the player. Supports zoom-out when new areas unlock.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothSpeed = 6f;
        [SerializeField] private float _defaultSize = 8f;
        [SerializeField] private float _expandedSize = 12f;
        [SerializeField] private float _zoomSpeed = 2f;

        private static readonly Quaternion IsoRotation = Quaternion.Euler(35f, 45f, 0f);
        private const float DistanceBehind = 20f; // far enough to not clip

        private Camera _cam;
        private float _targetSize;
        private Vector3 _shakeOffset;

        private void Start()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = gameObject.AddComponent<Camera>();

            _cam.orthographic = true;
            _cam.orthographicSize = _defaultSize;
            _cam.nearClipPlane = 0.1f;
            _cam.farClipPlane = 100f;
            _cam.backgroundColor = new Color(0.72f, 0.88f, 0.95f);
            _cam.clearFlags = CameraClearFlags.SolidColor;

            _targetSize = _defaultSize;

            transform.rotation = IsoRotation;

            if (_target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _target = player.transform;
            }

            if (_target != null)
                transform.position = CalculatePosition(_target.position);
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Smooth follow
            Vector3 desiredPos = CalculatePosition(_target.position);
            transform.position = Vector3.Lerp(transform.position, desiredPos,
                _smoothSpeed * Time.deltaTime) + _shakeOffset;

            // Keep exact rotation
            transform.rotation = IsoRotation;

            // Smooth zoom
            if (_cam != null)
            {
                _cam.orthographicSize = Mathf.Lerp(
                    _cam.orthographicSize, _targetSize, _zoomSpeed * Time.deltaTime);
            }

            // Decay shake
            _shakeOffset = Vector3.Lerp(_shakeOffset, Vector3.zero, 15f * Time.deltaTime);
        }

        private Vector3 CalculatePosition(Vector3 targetPos)
        {
            return targetPos - IsoRotation * Vector3.forward * DistanceBehind;
        }

        /// <summary>Zoom out to show expanded area.</summary>
        public void ZoomOut()
        {
            _targetSize = _expandedSize;
        }

        /// <summary>Reset to default zoom.</summary>
        public void ZoomDefault()
        {
            _targetSize = _defaultSize;
        }

        public void Shake(float intensity = 0.3f)
        {
            var rand = Random.insideUnitSphere * intensity;
            rand.z = 0f;
            _shakeOffset = rand;
        }
    }
}
