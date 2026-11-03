using UnityEngine;

namespace ShopIdle.UI
{
    public class Billboard : MonoBehaviour
    {
        private Transform _cam;

        private void Start()
        {
            var cam = UnityEngine.Camera.main;
            if (cam != null)
                _cam = cam.transform;
        }

        private void LateUpdate()
        {
            if (_cam == null) return;
            transform.forward = _cam.forward;
        }
    }
}
