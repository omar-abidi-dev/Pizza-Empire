using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace ShopIdle.Workers
{
    public class DriveThruWorker : MonoBehaviour
    {
        [SerializeField] private int _maxCarry = 3;
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _pickupInterval = 0.2f;
        [SerializeField] private float _depositInterval = 0.15f;
        [SerializeField] private float _arrivalDist = 1.8f;

        private NavMeshAgent _agent;
        private readonly List<Transform> _carried = new();
        private Transform _stackAnchor;

        private Kitchen.Oven _targetOven;
        private DriveThru.DriveThruWindow _window;
        private DTState _state = DTState.Idle;
        private float _actionTimer;
        private float _idleCooldown;

        private const float PizzaHeight = 0.15f;
        private const float IdleRetry = 0.5f;

        private enum DTState { Idle, GoingToOven, PickingUp, GoingToWindow, Depositing }

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.speed = _moveSpeed;
                _agent.stoppingDistance = _arrivalDist;
            }

            var anchor = new GameObject("DTWorkerStack");
            anchor.transform.SetParent(transform);
            anchor.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            _stackAnchor = anchor.transform;
        }

        private void Update()
        {
            if (_window == null)
                _window = DriveThru.DriveThruWindow.Instance;

            switch (_state)
            {
                case DTState.Idle:
                    _idleCooldown -= Time.deltaTime;
                    if (_idleCooldown <= 0f)
                    {
                        if (_window != null && _window.HasWaitingCar)
                            FindOvenWithPizzas();
                        _idleCooldown = IdleRetry;
                    }
                    break;
                case DTState.GoingToOven:
                    if (HasArrived()) { _state = DTState.PickingUp; _actionTimer = _pickupInterval; }
                    break;
                case DTState.PickingUp:
                    PickUpPizzas();
                    break;
                case DTState.GoingToWindow:
                    if (HasArrived()) { _state = DTState.Depositing; _actionTimer = _depositInterval; }
                    break;
                case DTState.Depositing:
                    DepositPizzas();
                    break;
            }
        }

        private bool HasArrived()
        {
            if (_agent == null) return true;
            return !_agent.pathPending && _agent.remainingDistance <= _arrivalDist;
        }

        private void FindOvenWithPizzas()
        {
            var ovens = Object.FindObjectsByType<Kitchen.Oven>(FindObjectsSortMode.None);
            Kitchen.Oven best = null;
            int bestCount = 0;
            foreach (var oven in ovens)
            {
                if (oven.TrayCount > bestCount) { best = oven; bestCount = oven.TrayCount; }
            }
            if (best != null && bestCount > 0)
            {
                _targetOven = best;
                _agent.SetDestination(best.transform.position);
                _state = DTState.GoingToOven;
            }
        }

        private void PickUpPizzas()
        {
            if (_carried.Count >= _maxCarry) { GoToWindow(); return; }
            if (_targetOven == null || _targetOven.TrayCount <= 0)
            {
                if (_carried.Count > 0) GoToWindow(); else _state = DTState.Idle;
                return;
            }
            _actionTimer += Time.deltaTime;
            if (_actionTimer < _pickupInterval) return;
            _actionTimer = 0f;

            Transform pizza = _targetOven.TakeTopPizza();
            if (pizza == null)
            {
                if (_carried.Count > 0) GoToWindow(); else _state = DTState.Idle;
                return;
            }

            Vector3 startPos = pizza.position;
            int slot = _carried.Count;
            _carried.Add(pizza);

            var fly = pizza.gameObject.AddComponent<Kitchen.PizzaFly>();
            fly.Init(startPos, _stackAnchor.position + Vector3.up * (slot * PizzaHeight), 0.25f, () =>
            {
                if (this != null && pizza != null)
                {
                    pizza.SetParent(_stackAnchor);
                    pizza.localPosition = new Vector3(0f, slot * PizzaHeight, 0f);
                    pizza.localRotation = Quaternion.identity;
                }
            });
        }

        private void GoToWindow()
        {
            if (_window == null) { _state = DTState.Idle; return; }
            _agent.SetDestination(_window.ServePoint);
            _state = DTState.GoingToWindow;
        }

        private void DepositPizzas()
        {
            if (_carried.Count <= 0) { _state = DTState.Idle; return; }
            _actionTimer += Time.deltaTime;
            if (_actionTimer < _depositInterval) return;
            _actionTimer = 0f;

            int last = _carried.Count - 1;
            Transform pizza = _carried[last];
            _carried.RemoveAt(last);

            if (pizza != null)
            {
                Vector3 startPos = pizza.position;
                Vector3 endPos = _window.transform.position + Vector3.up;
                pizza.SetParent(null);
                var fly = pizza.gameObject.AddComponent<Kitchen.PizzaFly>();
                fly.Init(startPos, endPos, 0.25f, () =>
                {
                    if (pizza != null) Object.Destroy(pizza.gameObject);
                });
            }
            if (_window != null) _window.DepositFromWorker(1);
        }
    }
}
