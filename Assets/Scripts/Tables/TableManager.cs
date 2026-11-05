using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Tables
{
    public class TableManager : MonoBehaviour
    {
        private static TableManager _instance;
        public static TableManager Instance => _instance;

        private readonly List<DineInTable> _tables = new();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public void RegisterTable(DineInTable table)
        {
            if (!_tables.Contains(table))
                _tables.Add(table);
        }

        public DineInTable TryReserveSeat(out int seatIndex)
        {
            seatIndex = -1;
            foreach (var table in _tables)
            {
                int seat = table.TryReserveSeat();
                if (seat >= 0)
                {
                    seatIndex = seat;
                    return table;
                }
            }
            return null;
        }

        public DineInTable FindDirtyTable()
        {
            foreach (var table in _tables)
            {
                if (table.HasDirtyPlates)
                    return table;
            }
            return null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
