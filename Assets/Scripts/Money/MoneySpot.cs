using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Money
{
    public class MoneySpot : MonoBehaviour
    {
        [Header("Bill Layout")]
        [SerializeField] private Transform _billAnchor;
        [SerializeField] private float _billHeight = 0.035f;

        [Header("Pickup")]
        [SerializeField] private float _pickupInterval = 0.08f;

        private readonly List<MoneyPickup> _bills = new List<MoneyPickup>();
        private Transform _playerInRange;
        private float _pickupTimer;

        private static Material _billMaterial;

        public void SpawnBills(int totalMoney, int perBill = 10)
        {
            int count = Mathf.Max(1, totalMoney / perBill);
            int valuePerBill = totalMoney / count;
            int existingCount = _bills.Count;

            for (int i = 0; i < count; i++)
            {
                var bill = CreateBill(existingCount + i);
                bill.SetValue(valuePerBill);
                _bills.Add(bill);
            }
        }

        private MoneyPickup CreateBill(int stackIndex)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "Bill";
            obj.transform.localScale = new Vector3(0.4f, 0.03f, 0.25f);

            // Position on stack
            Transform anchor = _billAnchor != null ? _billAnchor : transform;
            float y = stackIndex * _billHeight;
            obj.transform.position = anchor.position + Vector3.up * y;

            // Slight random rotation for natural look
            obj.transform.rotation = Quaternion.Euler(0f, Random.Range(-8f, 8f), 0f);

            // Green material
            if (_billMaterial == null)
            {
                _billMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                _billMaterial.color = new Color(0.2f, 0.7f, 0.25f);
            }
            obj.GetComponent<MeshRenderer>().material = _billMaterial;

            // Remove default collider, add pickup
            Destroy(obj.GetComponent<BoxCollider>());
            var pickup = obj.AddComponent<MoneyPickup>();
            return pickup;
        }

        private void Update()
        {
            if (_playerInRange == null || _bills.Count == 0) return;

            _pickupTimer += Time.deltaTime;
            if (_pickupTimer < _pickupInterval) return;

            _pickupTimer = 0f;
            SendNextBill();
        }

        private void SendNextBill()
        {
            // Find next bill that isn't already flying
            for (int i = 0; i < _bills.Count; i++)
            {
                if (!_bills[i].IsFlying)
                {
                    var bill = _bills[i];
                    _bills.RemoveAt(i);
                    bill.FlyToPlayer(_playerInRange, OnBillCollected);
                    return;
                }
            }
        }

        private void OnBillCollected(MoneyPickup bill)
        {
            if (MoneyManager.Instance != null)
                MoneyManager.Instance.AddMoney(bill.Value);

            Core.AudioManager.Instance?.Play(Core.SoundType.CoinTick, 0.5f);
            Core.JuiceManager.Instance?.SpawnCoinBurst(
                bill.transform.position, 4);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                _playerInRange = other.transform;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                _playerInRange = null;
        }
    }
}
