using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Keeps a character's visual container on the floor (y = 0) while the root moves.
    /// The root's height varies by controller (CharacterController centre, NavMeshAgent
    /// baseOffset, static placement), so the visual is pinned in world space instead.
    /// </summary>
    public class GroundedVisual : MonoBehaviour
    {
        private const float FloorY = 0f;

        [SerializeField] private Transform _visual;
        [SerializeField] private float _footOffset; // local height of the lowest part (feet)

        public void SetVisual(Transform visual, float footOffset = 0f)
        {
            _visual = visual;
            _footOffset = footOffset;
            LateUpdate(); // ground immediately, not one frame later
        }

        private void LateUpdate()
        {
            if (_visual == null) return;
            Vector3 p = _visual.position;
            float y = FloorY - _footOffset * _visual.lossyScale.y; // offset is in visual-local units
            if (p.y != y)
                _visual.position = new Vector3(p.x, y, p.z);
        }
    }
}
