using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

namespace ShopIdle.Core
{
    /// <summary>
    /// Manages restaurant progression, theming, and transitions.
    /// Persists across scene reloads via DontDestroyOnLoad.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class RestaurantManager : MonoBehaviour
    {
        private static RestaurantManager _instance;
        public static RestaurantManager Instance => _instance;

        private RestaurantConfig[] _templates;
        private int _restaurantIndex;
        private bool _transitioning;

        private static Material _foodMaterial;
        private static Color _cachedFoodColor;

        // Cost formula: baseCost * CostGrowth ^ restaurantIndex
        private const float CostGrowth = 3f;
        private const int NextRestaurantBaseCost = 2500;
        public const int UpgradeCount = 14;

        private static readonly int[] BaseCosts =
        {
            50, 80, 120, 200, 250, 300, 350, 400, 600, 700, 1000, 1200, 1500, 1800
        };

        public RestaurantConfig CurrentConfig => GetConfig(_restaurantIndex);
        public int RestaurantIndex => _restaurantIndex;
        public bool IsTransitioning => _transitioning;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            BuildTemplates();
            LoadIndex();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            ApplyTheme();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _foodMaterial = null; // Reset cached material for new restaurant
            StartCoroutine(ApplyThemeNextFrame());
        }

        private IEnumerator ApplyThemeNextFrame()
        {
            yield return null;
            ApplyTheme();
        }

        // ── Config Creation ──

        private void BuildTemplates()
        {
            _templates = new RestaurantConfig[4];

            _templates[0] = MakeConfig(
                "Pizza Palace", "Pizza",
                new Color(1f, 0.85f, 0.2f), PrimitiveType.Cylinder,
                new Vector3(0.4f, 0.05f, 0.4f),
                new Color(0.75f, 0.72f, 0.68f),
                new Color(0.85f, 0.82f, 0.75f),
                new Color(0.5f, 0.15f, 0.1f),
                10, 15, 0);

            _templates[1] = MakeConfig(
                "Burger Barn", "Burger",
                new Color(0.6f, 0.35f, 0.15f), PrimitiveType.Sphere,
                new Vector3(0.35f, 0.25f, 0.35f),
                new Color(0.82f, 0.78f, 0.65f),
                new Color(0.9f, 0.75f, 0.55f),
                new Color(0.25f, 0.25f, 0.3f),
                30, 45, 100);

            _templates[2] = MakeConfig(
                "Sushi Sensei", "Sushi",
                new Color(0.95f, 0.4f, 0.35f), PrimitiveType.Cube,
                new Vector3(0.3f, 0.15f, 0.2f),
                new Color(0.85f, 0.82f, 0.78f),
                new Color(0.7f, 0.6f, 0.5f),
                new Color(0.2f, 0.2f, 0.2f),
                90, 135, 300);

            _templates[3] = MakeConfig(
                "Taco Fiesta", "Taco",
                new Color(0.95f, 0.8f, 0.3f), PrimitiveType.Capsule,
                new Vector3(0.2f, 0.15f, 0.35f),
                new Color(0.9f, 0.82f, 0.65f),
                new Color(0.85f, 0.55f, 0.3f),
                new Color(0.6f, 0.3f, 0.15f),
                270, 405, 900);
        }

        private static RestaurantConfig MakeConfig(
            string name, string food,
            Color foodColor, PrimitiveType foodShape, Vector3 foodScale,
            Color floorColor, Color wallColor, Color ovenColor,
            int moneyPerItem, int dtMoneyPerItem, int bonus)
        {
            var c = ScriptableObject.CreateInstance<RestaurantConfig>();
            c.restaurantName = name;
            c.foodName = food;
            c.foodColor = foodColor;
            c.foodShape = foodShape;
            c.foodScale = foodScale;
            c.floorColor = floorColor;
            c.wallColor = wallColor;
            c.ovenColor = ovenColor;
            c.moneyPerItem = moneyPerItem;
            c.driveThruMoneyPerItem = dtMoneyPerItem;
            c.startingBonus = bonus;
            return c;
        }

        // ── Config Access ──

        /// <summary>Get config for any restaurant index. Cycles themes, scales economy.</summary>
        public RestaurantConfig GetConfig(int index)
        {
            if (index < _templates.Length)
                return _templates[index];

            // Infinite generation: cycle theme, scale economy
            int theme = index % _templates.Length;
            var t = _templates[theme];
            float scale = Mathf.Pow(CostGrowth, index);

            var c = ScriptableObject.CreateInstance<RestaurantConfig>();
            int cycle = index / _templates.Length + 1;
            c.restaurantName = $"{t.restaurantName} {cycle}";
            c.foodName = t.foodName;
            c.foodColor = t.foodColor;
            c.foodShape = t.foodShape;
            c.foodScale = t.foodScale;
            c.floorColor = t.floorColor;
            c.wallColor = t.wallColor;
            c.ovenColor = t.ovenColor;
            c.moneyPerItem = Mathf.RoundToInt(10f * scale);
            c.driveThruMoneyPerItem = Mathf.RoundToInt(15f * scale);
            c.startingBonus = Mathf.RoundToInt(100f * scale);
            return c;
        }

        /// <summary>Upgrade cost for a given index at the current restaurant.</summary>
        public int GetUpgradeCost(int upgradeIndex)
        {
            if (upgradeIndex < 0 || upgradeIndex >= BaseCosts.Length)
                return 9999;
            return Mathf.RoundToInt(BaseCosts[upgradeIndex] * Mathf.Pow(CostGrowth, _restaurantIndex));
        }

        /// <summary>Cost to advance to the next restaurant.</summary>
        public int GetNextRestaurantCost()
        {
            return Mathf.RoundToInt(NextRestaurantBaseCost * Mathf.Pow(CostGrowth, _restaurantIndex));
        }

        // ── Food Visual Factory ──

        /// <summary>Create a food item matching the current restaurant's config.</summary>
        public static GameObject CreateFoodVisual()
        {
            var config = _instance != null ? _instance.CurrentConfig : null;
            var shape = config != null ? config.foodShape : PrimitiveType.Cylinder;
            var color = config != null ? config.foodColor : new Color(1f, 0.85f, 0.2f);
            var scale = config != null ? config.foodScale : new Vector3(0.4f, 0.05f, 0.4f);

            var obj = GameObject.CreatePrimitive(shape);
            obj.name = "Food";
            obj.transform.localScale = scale;

            var col = obj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            if (_foodMaterial == null || _cachedFoodColor != color)
            {
                _foodMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                _foodMaterial.color = color;
                _cachedFoodColor = color;
            }
            obj.GetComponent<MeshRenderer>().sharedMaterial = _foodMaterial;

            return obj;
        }

        // ── Restaurant Transition ──

        public void StartNextRestaurant()
        {
            if (_transitioning) return;
            StartCoroutine(TransitionSequence());
        }

        private IEnumerator TransitionSequence()
        {
            _transitioning = true;

            // Gather total stats before resetting
            var sm = Save.SaveManager.Instance;
            if (sm != null)
            {
                var mm = Money.MoneyManager.Instance;
                sm.Data.totalMoneyEarned += mm != null ? mm.TotalMoney : 0;
                sm.Data.restaurantsCompleted = _restaurantIndex + 1;
                sm.Data.restaurantIndex = _restaurantIndex + 1;
                sm.Data.purchasedUpgradeCount = 0;
                sm.Data.activeZonePartialSpent = 0;
                sm.Data.money = GetConfig(_restaurantIndex + 1).startingBonus;
                sm.Data.playerX = 0f;
                sm.Data.playerY = 0.5f;
                sm.Data.playerZ = 0f;
                sm.SaveGame();
            }

            // Juice
            AudioManager.Instance?.Play(SoundType.Fanfare);
            JuiceManager.Instance?.ShakeCamera(0.5f);

            // Create fade overlay
            var fadeCanvas = CreateFadeCanvas();
            DontDestroyOnLoad(fadeCanvas);
            var fadeImg = fadeCanvas.transform.Find("Fade").GetComponent<UnityEngine.UI.Image>();

            // Fade to black
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                fadeImg.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t / 0.6f));
                yield return null;
            }
            fadeImg.color = Color.black;

            // Show next restaurant name
            var nextConfig = GetConfig(_restaurantIndex + 1);
            var textObj = new GameObject("RestaurantTitle");
            textObj.transform.SetParent(fadeCanvas.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.42f);
            textRect.anchorMax = new Vector2(0.9f, 0.58f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = nextConfig.restaurantName;
            tmp.fontSize = 64;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;

            // Scale punch
            textObj.transform.localScale = Vector3.one * 0.5f;
            float punch = 0f;
            while (punch < 0.3f)
            {
                punch += Time.deltaTime;
                float s = Mathf.Lerp(0.5f, 1f, punch / 0.3f);
                textObj.transform.localScale = Vector3.one * s;
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);

            // Increment index and reload scene
            _restaurantIndex++;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            // Wait for scene to load
            yield return null;
            yield return null;

            // Fade out
            t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float a = 1f - Mathf.Clamp01(t / 0.6f);
                fadeImg.color = new Color(0f, 0f, 0f, a);
                if (tmp != null) tmp.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }

            Destroy(fadeCanvas);
            _transitioning = false;
        }

        private static GameObject CreateFadeCanvas()
        {
            var obj = new GameObject("FadeCanvas");
            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var scaler = obj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var fadeObj = new GameObject("Fade");
            fadeObj.transform.SetParent(obj.transform, false);
            var rect = fadeObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = fadeObj.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0f, 0f, 0f, 0f);

            return obj;
        }

        // ── Scene Theming ──

        private void ApplyTheme()
        {
            var config = CurrentConfig;
            if (config == null) return;

            SetObjColor("Floor", config.floorColor);
            SetObjColor("WallLeft", config.wallColor);
            SetObjColor("WallBack", config.wallColor);
            SetObjColor("WallRight", config.wallColor);
            SetObjColor("WallFront", config.wallColor);
            SetObjColor("Oven", config.ovenColor);
        }

        private static void SetObjColor(string objName, Color color)
        {
            var obj = GameObject.Find(objName);
            if (obj == null) return;
            var r = obj.GetComponent<MeshRenderer>();
            if (r != null) r.material.color = color;
        }

        // ── Persistence ──

        private void LoadIndex()
        {
            // Read directly from file to avoid circular dependency with SaveManager
            string path = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
            if (!System.IO.File.Exists(path)) return;

            try
            {
                string json = System.IO.File.ReadAllText(path);
                var data = JsonUtility.FromJson<Save.GameData>(json);
                if (data != null)
                    _restaurantIndex = data.restaurantIndex;
            }
            catch { /* Fresh start if corrupt */ }
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
