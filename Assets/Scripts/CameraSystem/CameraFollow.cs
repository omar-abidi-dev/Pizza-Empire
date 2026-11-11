using UnityEngine;

namespace ShopIdle.CameraSystem
{
    /// <summary>
    /// Close orthographic isometric camera (35° X, 45° Y) that follows the player.
    /// Zoom is defined by visible world width so portrait phones always see ~10 m
    /// across; the focus point leads slightly ahead in the movement direction.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        [Header("Zoom (visible width in metres)")]
        [SerializeField] private float _viewWidth = 10f;
        [SerializeField] private float _expandedViewWidth = 13f;
        [SerializeField] private float _zoomSpeed = 2f;

        [Header("Follow")]
        [SerializeField] private float _followSmoothTime = 0.18f;
        [SerializeField] private float _leadTime = 0.35f;      // seconds of movement to look ahead
        [SerializeField] private float _maxLead = 1.5f;        // metres
        [SerializeField] private float _leadSmoothTime = 0.4f;

        private static readonly Quaternion IsoRotation = Quaternion.Euler(35f, 45f, 0f);
        private const float DistanceBehind = 20f; // far enough to not clip

        private Camera _cam;
        private float _targetWidth;
        private Vector3 _focus;
        private Vector3 _focusVelocity;
        private Vector3 _lead;
        private Vector3 _leadVelocity;
        private Vector3 _lastTargetPos;
        private Vector3 _shakeOffset;

        private void Start()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = gameObject.AddComponent<Camera>();

            _cam.orthographic = true;
            _cam.nearClipPlane = 0.1f;
            _cam.farClipPlane = 100f;
            _cam.backgroundColor = new Color(0.72f, 0.88f, 0.95f);
            _cam.clearFlags = CameraClearFlags.SolidColor;

            _targetWidth = _viewWidth;
            _cam.orthographicSize = SizeForWidth(_targetWidth);
            transform.rotation = IsoRotation;

            if (_target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _target = player.transform;
            }

            if (_target != null)
            {
                _focus = _lastTargetPos = _target.position;
                transform.position = CalculatePosition(_focus);
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // Lead: look ahead along the target's horizontal velocity.
            Vector3 targetPos = _target.position;
            Vector3 velocity = (targetPos - _lastTargetPos) / dt;
            _lastTargetPos = targetPos;
            velocity.y = 0f;
            Vector3 desiredLead = Vector3.ClampMagnitude(velocity * _leadTime, _maxLead);
            _lead = Vector3.SmoothDamp(_lead, desiredLead, ref _leadVelocity, _leadSmoothTime);

            // Smooth follow of the led focus point.
            _focus = Vector3.SmoothDamp(_focus, targetPos + _lead, ref _focusVelocity, _followSmoothTime);
            transform.SetPositionAndRotation(CalculatePosition(_focus) + _shakeOffset, IsoRotation);

            // Width-driven zoom (adapts to portrait/landscape aspect).
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, SizeForWidth(_targetWidth), _zoomSpeed * dt);

            // Decay shake
            _shakeOffset = Vector3.Lerp(_shakeOffset, Vector3.zero, 15f * dt);
        }

        private float SizeForWidth(float width) => width / (2f * Mathf.Max(0.01f, _cam.aspect));

        private static Vector3 CalculatePosition(Vector3 focus)
        {
            return focus - IsoRotation * Vector3.forward * DistanceBehind;
        }

        /// <summary>Zoom out to show expanded area.</summary>
        public void ZoomOut()
        {
            _targetWidth = _expandedViewWidth;
        }

        /// <summary>Reset to default zoom.</summary>
        public void ZoomDefault()
        {
            _targetWidth = _viewWidth;
        }

        public void Shake(float intensity = 0.3f)
        {
            var rand = Random.insideUnitSphere * intensity;
            rand.z = 0f;
            _shakeOffset = transform.rotation * rand;
        }
    }
}
