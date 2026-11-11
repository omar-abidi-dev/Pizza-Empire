using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Procedural animation for rig characters: legs and arms swing and the body bobs
    /// with movement speed; arms reach forward while carrying food (any FoodItem below).
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        private const float StepRate = 9f;         // radians per metre-ish, scaled by speed
        private const float LegSwing = 32f, ArmSwing = 26f, CarryAngle = -78f;
        private const float BobHeight = 0.035f;

        private Transform _hips, _legL, _legR, _armL, _armR;
        private Quaternion _armLRest, _armRRest;
        private float _hipsY;
        private Vector3 _lastPos;
        private float _phase, _speed, _carryBlend, _carryPoll;
        private bool _carrying;

        public void Bind(Transform hips, Transform legL, Transform legR, Transform armL, Transform armR)
        {
            _hips = hips; _legL = legL; _legR = legR; _armL = armL; _armR = armR;
            _armLRest = armL.localRotation;
            _armRRest = armR.localRotation;
            _hipsY = hips.localPosition.y;
            _lastPos = transform.position;
        }

        private void LateUpdate()
        {
            if (_hips == null) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 delta = transform.position - _lastPos;
            delta.y = 0f;
            _lastPos = transform.position;
            _speed = Mathf.Lerp(_speed, delta.magnitude / dt, 10f * dt);
            float move = Mathf.Clamp01(_speed / 3f);

            _phase += _speed * dt * StepRate * 0.5f;
            float s = Mathf.Sin(_phase) * move;

            _carryPoll -= dt;
            if (_carryPoll <= 0f)
            {
                _carryPoll = 0.15f;
                _carrying = GetComponentInChildren<Kitchen.FoodItem>() != null;
            }
            _carryBlend = Mathf.MoveTowards(_carryBlend, _carrying ? 1f : 0f, 6f * dt);

            _legL.localRotation = Quaternion.Euler(s * LegSwing, 0f, 0f);
            _legR.localRotation = Quaternion.Euler(-s * LegSwing, 0f, 0f);

            float swingL = -s * ArmSwing * (1f - _carryBlend) + CarryAngle * _carryBlend;
            float swingR = s * ArmSwing * (1f - _carryBlend) + CarryAngle * _carryBlend;
            _armL.localRotation = _armLRest * Quaternion.Euler(swingL, 0f, 0f);
            _armR.localRotation = _armRRest * Quaternion.Euler(swingR, 0f, 0f);

            float bob = Mathf.Abs(Mathf.Sin(_phase)) * BobHeight * move;
            _hips.localPosition = new Vector3(0f, _hipsY + bob, 0f);
        }
    }
}
