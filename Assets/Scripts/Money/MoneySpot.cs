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

        // Pile cell size for 0.32 x 0.18 m bill stacks turned along Z (fits the 0.9 m deep counter).
        private const float CellX = 0.2f;
        private const float CellZ = 0.34f;

        public void SetBillAnchor(Transform anchor) { _billAnchor = anchor; }

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
            Transform anchor = _billAnchor != null ? _billAnchor : transform;
            float yaw = Random.Range(-8f, 8f);

            // Cash-stack model in a 3x3 pile that grows upward (pickup lives on the model root)
            var stackPos = anchor.position + CashPile.Offset(stackIndex, CellX, CellZ, Core.VisualFactory.CashStackHeight);
            var model = Core.VisualFactory.CreateCashVisual("Bill", stackPos, CashPile.RandomYaw());
            if (model != null)
                return model.AddComponent<MoneyPickup>();

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "Bill";
            obj.transform.localScale = new Vector3(0.4f, 0.03f, 0.25f);

            // Position on stack
            float y = stackIndex * _billHeight;
            obj.transform.position = anchor.position + Vector3.up * y;

            // Slight random rotation for natural look
            obj.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            // Green material from palette
            if (_billMaterial == null)
            {
                var p = Core.ColorPalette.Instance;
                _billMaterial = new Material(ShopIdle.Core.ColorPalette.LitShader);
                _billMaterial.color = p.billGreen;
                _billMaterial.SetFloat("_Smoothness", 0.2f);
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
            Core.GameEvents.RaiseCashCollected(bill.Value);

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
