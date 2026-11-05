using UnityEngine;
using UnityEngine.AI;

namespace ShopIdle.Workers
{
    public class CleanerWorker : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _cleanDuration = 0.8f;
        [SerializeField] private float _arrivalDist = 1.5f;

        private NavMeshAgent _agent;
        private Tables.DineInTable _targetTable;
        private CleanerState _state = CleanerState.Idle;
        private float _cleanTimer;
        private float _idleCooldown;

        private const float IdleRetry = 0.5f;

        private enum CleanerState
        {
            Idle,
            GoingToTable,
            Cleaning
        }

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.speed = _moveSpeed;
                _agent.stoppingDistance = _arrivalDist;
            }
        }

        private void Update()
        {
            switch (_state)
            {
                case CleanerState.Idle:
                    _idleCooldown -= Time.deltaTime;
                    if (_idleCooldown <= 0f)
                    {
                        FindDirtyTable();
                        _idleCooldown = IdleRetry;
                    }
                    break;

                case CleanerState.GoingToTable:
                    if (_targetTable == null || !_targetTable.HasDirtyPlates)
                    {
                        _state = CleanerState.Idle;
                        return;
                    }
                    if (HasArrived())
                    {
                        _state = CleanerState.Cleaning;
                        _cleanTimer = 0f;
                    }
                    break;

                case CleanerState.Cleaning:
                    _cleanTimer += Time.deltaTime;
                    if (_cleanTimer >= _cleanDuration)
                    {
                        if (_targetTable != null)
                            _targetTable.CleanAllPlates();
                        _targetTable = null;
                        _state = CleanerState.Idle;
                    }
                    break;
            }

            UpdateBob();
        }

        private bool HasArrived()
        {
            if (_agent == null) return true;
            return !_agent.pathPending && _agent.remainingDistance <= _arrivalDist;
        }

        private void FindDirtyTable()
        {
            var tm = Tables.TableManager.Instance;
            if (tm == null) return;

            var table = tm.FindDirtyTable();
            if (table != null)
            {
                _targetTable = table;
                _agent.SetDestination(table.transform.position);
                _state = CleanerState.GoingToTable;
            }
        }

        private float _bobTimer;
        private void UpdateBob()
        {
            if (_state != CleanerState.Idle) return;
            _bobTimer += Time.deltaTime;
            float y = 0.5f + Mathf.Sin(_bobTimer * 2f) * 0.03f;
            var pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }
    }
}
