using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Tables
{
    public class DineInTable : MonoBehaviour
    {
        private const int SeatCount = 2;
        private const float CleanInterval = 0.3f;
        private const float BillPickupInterval = 0.08f;

        private readonly bool[] _seatOccupied = new bool[SeatCount];
        private readonly bool[] _seatDirty = new bool[SeatCount];
        private readonly GameObject[] _dirtyPlates = new GameObject[SeatCount];
        private readonly List<Money.MoneyPickup> _bills = new();

        private Transform _playerInRange;
        private float _cleanTimer;
        private float _billTimer;

        private static readonly Vector3[] SeatOffsets =
        {
            new(-0.9f, 0f, 0f),
            new(0.9f, 0f, 0f)
        };

        public bool HasFreeSeat
        {
            get
            {
                for (int i = 0; i < SeatCount; i++)
                    if (!_seatOccupied[i] && !_seatDirty[i]) return true;
                return false;
            }
        }

        public bool HasDirtyPlates
        {
            get
            {
                for (int i = 0; i < SeatCount; i++)
                    if (_seatDirty[i]) return true;
                return false;
            }
        }

        public int TryReserveSeat()
        {
            for (int i = 0; i < SeatCount; i++)
            {
                if (!_seatOccupied[i] && !_seatDirty[i])
                {
                    _seatOccupied[i] = true;
                    return i;
                }
            }
            return -1;
        }

        public Vector3 GetSeatWorldPos(int index)
        {
            return transform.position + SeatOffsets[index];
        }

        public void CustomerFinished(int seatIndex, int moneyAmount)
        {
            _seatOccupied[seatIndex] = false;
            _seatDirty[seatIndex] = true;
            SpawnDirtyPlate(seatIndex);
            if (moneyAmount > 0)
                SpawnTableMoney(moneyAmount);
        }

        public void CleanAllPlates()
        {
            for (int i = 0; i < SeatCount; i++)
            {
                if (_seatDirty[i])
                    CleanSeat(i);
            }
        }

        private void Update()
        {
            if (_playerInRange == null) return;

            // Clean dirty plates
            if (HasDirtyPlates)
            {
                _cleanTimer += Time.deltaTime;
                if (_cleanTimer >= CleanInterval)
                {
                    _cleanTimer = 0f;
                    CleanNextPlate();
                }
            }

            // Collect money bills
            if (_bills.Count > 0)
            {
                _billTimer += Time.deltaTime;
                if (_billTimer >= BillPickupInterval)
                {
                    _billTimer = 0f;
                    SendNextBill();
                }
            }
        }

        private void CleanNextPlate()
        {
            for (int i = 0; i < SeatCount; i++)
            {
                if (_seatDirty[i])
                {
                    CleanSeat(i);
                    return;
                }
            }
        }

        private void CleanSeat(int index)
        {
            _seatDirty[index] = false;

            // Track table cleaning for daily quests
            Core.DailyQuestManager.Instance?.TrackTableCleaned();

            if (_dirtyPlates[index] != null)
            {
                var plate = _dirtyPlates[index];
                _dirtyPlates[index] = null;

                // Pop animation: scale up then destroy
                StartCoroutine(PopAndDestroy(plate));
            }
        }

        private System.Collections.IEnumerator PopAndDestroy(GameObject obj)
        {
            Vector3 original = obj.transform.localScale;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float s = 1f + Mathf.Sin(t / 0.2f * Mathf.PI) * 0.5f;
                obj.transform.localScale = original * s;
                yield return null;
            }
            Destroy(obj);
        }

        private void SendNextBill()
        {
            for (int i = 0; i < _bills.Count; i++)
            {
                if (_bills[i] == null)
                {
                    _bills.RemoveAt(i);
                    i--;
                    continue;
                }
                if (!_bills[i].IsFlying)
                {
                    var bill = _bills[i];
                    _bills.RemoveAt(i);
                    bill.FlyToPlayer(_playerInRange, OnBillCollected);
                    return;
                }
            }
        }

        private void OnBillCollected(Money.MoneyPickup bill)
        {
            Money.MoneyManager.Instance?.AddMoney(bill.Value);
            Core.AudioManager.Instance?.Play(Core.SoundType.CoinTick, 0.5f);
        }

        private void SpawnDirtyPlate(int seatIndex)
        {
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plate.name = "DirtyPlate";
            plate.transform.localScale = new Vector3(0.35f, 0.02f, 0.35f);

            // Position on table surface near the seat
            Vector3 pos = transform.position + SeatOffsets[seatIndex] * 0.4f;
            pos.y = 0.56f;
            plate.transform.position = pos;

            Destroy(plate.GetComponent<CapsuleCollider>());

            var mat = new Material(ShopIdle.Core.ColorPalette.LitShader);
            mat.color = new Color(0.6f, 0.6f, 0.55f);
            plate.GetComponent<MeshRenderer>().material = mat;

            _dirtyPlates[seatIndex] = plate;
        }

        private void SpawnTableMoney(int amount)
        {
            int count = Mathf.Max(1, amount / 10);
            int perBill = amount / count;

            for (int i = 0; i < count; i++)
            {
                var bill = CreateBill(i);
                bill.SetValue(perBill);
                _bills.Add(bill);
            }
        }

        private Money.MoneyPickup CreateBill(int stackIndex)
        {
            // Cash-stack 3x3 pile growing upward on the table top (0.55 m)
            var stackPos = transform.position + Vector3.up * 0.55f +
                Money.CashPile.Offset(stackIndex, 0.2f, 0.3f, Core.VisualFactory.CashStackHeight);
            var model = Core.VisualFactory.CreateCashVisual("TableBill", stackPos, Money.CashPile.RandomYaw());
            if (model != null)
                return model.AddComponent<Money.MoneyPickup>();

            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "TableBill";
            obj.transform.localScale = new Vector3(0.3f, 0.025f, 0.2f);

            float y = 0.56f + stackIndex * 0.03f;
            obj.transform.position = transform.position + new Vector3(0f, y, 0.2f);
            obj.transform.rotation = Quaternion.Euler(0f, Random.Range(-15f, 15f), 0f);

            Destroy(obj.GetComponent<BoxCollider>());

            var mat = new Material(ShopIdle.Core.ColorPalette.LitShader);
            mat.color = new Color(0.2f, 0.7f, 0.25f);
            obj.GetComponent<MeshRenderer>().material = mat;

            return obj.AddComponent<Money.MoneyPickup>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = other.transform;
                _cleanTimer = CleanInterval;
                _billTimer = BillPickupInterval;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                _playerInRange = null;
        }
    }
}
