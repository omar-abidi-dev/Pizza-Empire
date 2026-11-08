using UnityEngine;
using TMPro;
using System.Collections;

namespace ShopIdle.Upgrades
{
    public class UpgradeZone : MonoBehaviour
    {
        [SerializeField] private int _totalCost;
        [SerializeField] private float _drainInterval = 0.1f;
        [SerializeField] private int _drainPerTick = 5;

        private int _remaining;
        private TextMeshPro _priceText;
        private Transform _playerInRange;
        private float _drainTimer;
        private bool _purchased;

        private GameObject _fillQuad;
        private GameObject _outlineQuad;

        public event System.Action OnPurchased;

        public int TotalCost => _totalCost;
        public int Remaining => _remaining;

        private static readonly Color ZoneColor = new Color(0.6f, 0.3f, 1f, 0.5f);
        private static readonly Color OutlineColor = new Color(0.6f, 0.3f, 1f, 0.18f);
        private const float ZoneSize = 2.5f;

        public void Init(string displayName, int cost)
        {
            _totalCost = cost;
            _remaining = cost;
            BuildVisuals(displayName);
        }

        /// <summary>Restore partial payment progress from save data.</summary>
        public void SetPartialSpent(int spent)
        {
            _remaining = Mathf.Max(0, _totalCost - spent);
            UpdateLabel();
        }

        private void BuildVisuals(string displayName)
        {
            // Outline quad (soft glow behind)
            _outlineQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _outlineQuad.name = "Outline";
            _outlineQuad.transform.SetParent(transform);
            _outlineQuad.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            _outlineQuad.transform.localScale = new Vector3(ZoneSize + 0.5f, ZoneSize + 0.5f, 1f);
            _outlineQuad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Object.Destroy(_outlineQuad.GetComponent<MeshCollider>());
            _outlineQuad.GetComponent<MeshRenderer>().material = CreateTransparentMat(OutlineColor);

            // Fill quad
            _fillQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _fillQuad.name = "Fill";
            _fillQuad.transform.SetParent(transform);
            _fillQuad.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            _fillQuad.transform.localScale = new Vector3(ZoneSize, ZoneSize, 1f);
            _fillQuad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Object.Destroy(_fillQuad.GetComponent<MeshCollider>());
            _fillQuad.GetComponent<MeshRenderer>().material = CreateTransparentMat(ZoneColor);

            // Trigger collider
            var col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(ZoneSize, 2f, ZoneSize);
            col.center = new Vector3(0f, 1f, 0f);

            // Label (3D text floating above)
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(transform);
            labelObj.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            labelObj.AddComponent<UI.Billboard>();

            _priceText = labelObj.AddComponent<TextMeshPro>();
            _priceText.text = $"{displayName}\n${_remaining}";
            _priceText.fontSize = 5f;
            _priceText.alignment = TextAlignmentOptions.Center;
            _priceText.color = Color.white;
            _priceText.outlineWidth = 0.2f;
            _priceText.outlineColor = new Color32(50, 10, 80, 255);
            _priceText.rectTransform.sizeDelta = new Vector2(3f, 2f);
        }

        private void Update()
        {
            if (_purchased || _playerInRange == null) return;
            if (_remaining <= 0) return;

            var mm = Money.MoneyManager.Instance;
            if (mm == null || mm.TotalMoney <= 0) return;

            _drainTimer += Time.deltaTime;
            if (_drainTimer < _drainInterval) return;
            _drainTimer = 0f;

            int want = Mathf.Min(_drainPerTick, _remaining);
            int spent = mm.SpendUpTo(want);
            if (spent <= 0) return;

            _remaining = Mathf.Max(0, _remaining - spent);
            SpawnMoneyFlyVisual();
            UpdateLabel();

            if (_remaining <= 0)
                StartCoroutine(PurchaseSequence());
        }

        private void UpdateLabel()
        {
            if (_priceText != null)
            {
                string name = _priceText.text.Split('\n')[0];
                _priceText.text = $"{name}\n${_remaining}";
            }
        }

        private void SpawnMoneyFlyVisual()
        {
            var bill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bill.name = "FlyBill";
            bill.transform.localScale = new Vector3(0.3f, 0.03f, 0.2f);
            bill.transform.position = _playerInRange.position + Vector3.up * 1.5f;
            Object.Destroy(bill.GetComponent<BoxCollider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.2f, 0.75f, 0.3f);
            bill.GetComponent<MeshRenderer>().material = mat;

            var fly = bill.AddComponent<Kitchen.PizzaFly>();
            fly.Init(bill.transform.position, transform.position + Vector3.up * 0.5f,
                0.25f, () => Destroy(bill));
        }

        private IEnumerator PurchaseSequence()
        {
            _purchased = true;

            // Pop scale up
            float t = 0f;
            Vector3 originalScale = transform.localScale;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float s = 1f + Mathf.Sin(t / 0.2f * Mathf.PI) * 0.4f;
                transform.localScale = originalScale * s;
                yield return null;
            }

            OnPurchased?.Invoke();
            Destroy(gameObject);
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

        private static Material CreateTransparentMat(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = color;
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            return mat;
        }
    }
}
