using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShopIdle.UI
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float _handleRange = 1f;

        private Canvas _canvas;
        private UnityEngine.Camera _cam;
        private Vector2 _inputVector;

        // Reference set in inspector or found at runtime
        [SerializeField] private Player.PlayerController _playerController;

        private void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
                _cam = _canvas.worldCamera;

            if (_playerController == null)
                _playerController = FindAnyObjectByType<Player.PlayerController>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background, eventData.position, _cam, out Vector2 localPoint);

            Vector2 bgSize = _background.sizeDelta;
            // Normalize to -1..1
            localPoint.x /= bgSize.x * 0.5f;
            localPoint.y /= bgSize.y * 0.5f;

            _inputVector = localPoint.magnitude > 1f ? localPoint.normalized : localPoint;

            // Move handle visual
            _handle.anchoredPosition = _inputVector * bgSize * 0.5f * _handleRange;

            if (_playerController != null)
                _playerController.SetJoystickInput(_inputVector);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _inputVector = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;

            if (_playerController != null)
                _playerController.SetJoystickInput(Vector2.zero);
        }
    }
}
