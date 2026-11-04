using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Customers
{
    public class CustomerQueue : MonoBehaviour
    {
        [Header("Queue Layout")]
        [SerializeField] private Transform _queueStart;
        [SerializeField] private float _spacing = 1.2f;
        [SerializeField] private int _maxQueueSize = 6;

        private readonly List<Customer> _queue = new List<Customer>();

        public int Count => _queue.Count;
        public bool IsFull => _queue.Count >= _maxQueueSize;
        public Customer FirstCustomer => _queue.Count > 0 ? _queue[0] : null;

        public void AddCustomer(Customer customer)
        {
            if (IsFull) return;

            _queue.Add(customer);
            SendToSlot(customer, _queue.Count - 1);
        }

        public void RemoveFirst()
        {
            if (_queue.Count == 0) return;

            _queue.RemoveAt(0);

            // Move everyone forward
            for (int i = 0; i < _queue.Count; i++)
            {
                SendToSlot(_queue[i], i);
            }
        }

        private void SendToSlot(Customer customer, int index)
        {
            Vector3 pos = GetSlotPosition(index);
            customer.SetTarget(pos);
        }

        private Vector3 GetSlotPosition(int index)
        {
            Vector3 start = _queueStart != null ? _queueStart.position : transform.position;
            // Queue extends backward (negative Z) from the start
            return start + Vector3.back * (index * _spacing);
        }
    }
}
