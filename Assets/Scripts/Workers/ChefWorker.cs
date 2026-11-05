using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace ShopIdle.Workers
{
    public class ChefWorker : MonoBehaviour
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
        private Customers.CounterZone _counterZone;
        private ChefState _state = ChefState.Idle;
        private float _actionTimer;
        private float _idleCooldown;

        private const float PizzaHeight = 0.15f;
        private const float IdleRetry = 0.5f;

        private enum ChefState
        {
            Idle,
            GoingToOven,
            PickingUp,
            GoingToCounter,
            Depositing
        }

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.speed = _moveSpeed;
                _agent.stoppingDistance = _arrivalDist;
            }

            // Stack anchor on top of capsule
            var anchor = new GameObject("ChefStack");
            anchor.transform.SetParent(transform);
            anchor.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            _stackAnchor = anchor.transform;

            // Find counter zone
            var cashierObj = GameObject.Find("CashierZone");
            if (cashierObj != null)
                _counterZone = cashierObj.GetComponent<Customers.CounterZone>();
        }

        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
            if (_agent != null) _agent.speed = speed;
        }

        public void SetMaxCarry(int max) { _maxCarry = max; }

        private void Update()
        {
            switch (_state)
            {
                case ChefState.Idle:
                    _idleCooldown -= Time.deltaTime;
                    if (_idleCooldown <= 0f)
                    {
                        FindOvenWithPizzas();
                        _idleCooldown = IdleRetry;
                    }
                    break;

                case ChefState.GoingToOven:
                    if (HasArrived())
                    {
                        _state = ChefState.PickingUp;
                        _actionTimer = _pickupInterval;
                    }
                    break;

                case ChefState.PickingUp:
                    PickUpPizzas();
                    break;

                case ChefState.GoingToCounter:
                    if (HasArrived())
                    {
                        _state = ChefState.Depositing;
                        _actionTimer = _depositInterval;
                    }
                    break;

                case ChefState.Depositing:
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
                if (oven.TrayCount > bestCount)
                {
                    best = oven;
                    bestCount = oven.TrayCount;
                }
            }

            if (best != null && bestCount > 0)
            {
                _targetOven = best;
                _agent.SetDestination(best.transform.position);
                _state = ChefState.GoingToOven;
            }
        }

        private void PickUpPizzas()
        {
            // Full or no more pizzas — go deliver
            if (_carried.Count >= _maxCarry)
            {
                GoToCounter();
                return;
            }

            if (_targetOven == null || _targetOven.TrayCount <= 0)
            {
                if (_carried.Count > 0)
                    GoToCounter();
                else
                    _state = ChefState.Idle;
                return;
            }

            _actionTimer += Time.deltaTime;
            if (_actionTimer < _pickupInterval) return;
            _actionTimer = 0f;

            Transform pizza = _targetOven.TakeTopPizza();
            if (pizza == null)
            {
                if (_carried.Count > 0) GoToCounter();
                else _state = ChefState.Idle;
                return;
            }

            // Fly pizza to chef stack
            Vector3 startPos = pizza.position;
            int slotIndex = _carried.Count;
            Vector3 endPos = _stackAnchor.position + Vector3.up * (slotIndex * PizzaHeight);
            _carried.Add(pizza);

            var fly = pizza.gameObject.AddComponent<Kitchen.PizzaFly>();
            fly.Init(startPos, endPos, 0.25f, () =>
            {
                if (this != null && pizza != null)
                {
                    pizza.SetParent(_stackAnchor);
                    pizza.localPosition = new Vector3(0f, slotIndex * PizzaHeight, 0f);
                    pizza.localRotation = Quaternion.identity;
                }
            });
        }

        private void GoToCounter()
        {
            if (_counterZone == null)
            {
                _state = ChefState.Idle;
                return;
            }
            _agent.SetDestination(_counterZone.transform.position);
            _state = ChefState.GoingToCounter;
        }

        private void DepositPizzas()
        {
            if (_carried.Count <= 0)
            {
                _state = ChefState.Idle;
                return;
            }

            _actionTimer += Time.deltaTime;
            if (_actionTimer < _depositInterval) return;
            _actionTimer = 0f;

            int last = _carried.Count - 1;
            Transform pizza = _carried[last];
            _carried.RemoveAt(last);

            if (pizza != null)
            {
                Vector3 startPos = pizza.position;
                Vector3 endPos = _counterZone.transform.position + Vector3.up;
                pizza.SetParent(null);

                var fly = pizza.gameObject.AddComponent<Kitchen.PizzaFly>();
                fly.Init(startPos, endPos, 0.25f, () =>
                {
                    if (pizza != null)
                        Object.Destroy(pizza.gameObject);
                });
            }

            if (_counterZone != null)
                _counterZone.DepositFromChef(1);
        }
    }
}
