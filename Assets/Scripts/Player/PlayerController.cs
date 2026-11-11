using UnityEngine;
using UnityEngine.InputSystem;

namespace ShopIdle.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 720f;
        [SerializeField] private float _gravity = -20f;

        [Header("Walk Bob")]
        [SerializeField] private float _bobFrequency = 10f;
        [SerializeField] private float _bobAmplitude = 0.06f;

        private CharacterController _controller;
        private InputSystem_Actions _input;
        private Vector3 _velocity;
        private Transform _cameraTransform;
        private float _bobTimer;
        private float _pickupBounce;

        // External joystick input (set by VirtualJoystick)
        private Vector2 _joystickInput;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = new InputSystem_Actions();
        }

        private void Start()
        {
            var cam = UnityEngine.Camera.main;
            if (cam != null)
                _cameraTransform = cam.transform;

            // Restore saved position
            var sm = Save.SaveManager.Instance;
            if (sm != null && sm.Data != null && sm.Data.lastSaveTimestamp > 0)
            {
                var pos = new Vector3(sm.Data.playerX, sm.Data.playerY, sm.Data.playerZ);
                _controller.enabled = false;
                transform.position = pos;
                _controller.enabled = true;
            }
        }

        private void OnEnable()
        {
            if (_input == null) _input = new InputSystem_Actions();
            _input.Player.Enable();
        }

        private void OnDisable()
        {
            _input?.Player.Disable();
        }

        private void Update()
        {
            // Read keyboard/gamepad input
            if (_input == null) return;
            Vector2 rawInput = _input.Player.Move.ReadValue<Vector2>();

            // Use joystick input if keyboard input is zero
            if (rawInput.sqrMagnitude < 0.01f)
                rawInput = _joystickInput;

            // Convert input to camera-relative direction
            Vector3 moveDir = Vector3.zero;
            if (rawInput.sqrMagnitude > 0.01f && _cameraTransform != null)
            {
                Vector3 camForward = _cameraTransform.forward;
                Vector3 camRight = _cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveDir = (camForward * rawInput.y + camRight * rawInput.x).normalized;
            }

            // Apply gravity
            if (_controller.isGrounded && _velocity.y < 0f)
                _velocity.y = -2f;

            _velocity.y += _gravity * Time.deltaTime;

            // Move
            _controller.Move((moveDir * _moveSpeed + _velocity) * Time.deltaTime);

            // Rotate to face movement direction
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
            }

            // Walking bob via scale (lightweight, no transform conflicts)
            float baseScaleY = 1f;
            float baseScaleXZ = 1f;
            if (moveDir.sqrMagnitude > 0.01f)
            {
                _bobTimer += Time.deltaTime * _bobFrequency;
                float bob = Mathf.Sin(_bobTimer) * _bobAmplitude;
                baseScaleY = 1f + bob;
                baseScaleXZ = 1f - bob * 0.5f;
            }
            else
            {
                _bobTimer = 0f;
            }

            // Pickup bounce (squash-and-stretch)
            if (_pickupBounce > 0f)
            {
                _pickupBounce -= Time.deltaTime * 6f;
                float t = Mathf.Clamp01(_pickupBounce);
                float squash = Mathf.Sin(t * Mathf.PI) * 0.2f;
                baseScaleY += squash;
                baseScaleXZ -= squash * 0.5f;
            }

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                new Vector3(baseScaleXZ, baseScaleY, baseScaleXZ),
                10f * Time.deltaTime);
        }

        public void SetJoystickInput(Vector2 input)
        {
            _joystickInput = input;
        }

        public void TriggerPickupBounce()
        {
            _pickupBounce = 1f;
        }
    }
}
