using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

namespace ShopIdle.UI
{
    /// <summary>
    /// Title/loading screen shown on startup.
    /// Displays "PIZZA EMPIRE" title, mascot, and pulsing "Tap to Start".
    /// </summary>
    [DefaultExecutionOrder(-90)]
    public class TitleScreen : MonoBehaviour
    {
        private static TitleScreen _instance;
        public static TitleScreen Instance => _instance;

        private Canvas _canvas;
        private GameObject _screenRoot;
        private TextMeshProUGUI _tapText;
        private float _pulseTimer;
        private bool _dismissed;
        private InputSystem_Actions _input;

        public bool IsDismissed => _dismissed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            _canvas = FindAnyObjectByType<Canvas>();
            if (_canvas == null) return;

            _input = new InputSystem_Actions();
            _input.Player.Enable();

            BuildScreen();

            // Pause gameplay while title is showing
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (_dismissed) return;

            // Pulse "Tap to Start" text
            if (_tapText != null)
            {
                _pulseTimer += Time.unscaledDeltaTime * 2.5f;
                float alpha = 0.5f + Mathf.Sin(_pulseTimer) * 0.5f;
                _tapText.color = new Color(1f, 1f, 1f, alpha);
            }

            // Detect tap/click via pointer or any movement input
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Dismiss();
                return;
            }
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Dismiss();
                return;
            }
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                Dismiss();
                return;
            }
        }

        private void Dismiss()
        {
            if (_dismissed) return;
            _dismissed = true;
            Time.timeScale = 1f;

            // Play sound if available
            Core.AudioManager.Instance?.Play(Core.SoundType.Whoosh);

            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            var cg = _screenRoot.GetComponent<CanvasGroup>();
            if (cg == null) cg = _screenRoot.AddComponent<CanvasGroup>();

            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = 1f - (t / 0.4f);
                yield return null;
            }

            Destroy(_screenRoot);
        }

        private void BuildScreen()
        {
            var p = Core.ColorPalette.Instance;

            // Root container covering full screen
            _screenRoot = new GameObject("TitleScreen");
            _screenRoot.transform.SetParent(_canvas.transform, false);
            var rootRect = _screenRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // Ensure it renders on top
            var sortCanvas = _screenRoot.AddComponent<Canvas>();
            sortCanvas.overrideSorting = true;
            sortCanvas.sortingOrder = 100;
            _screenRoot.AddComponent<GraphicRaycaster>();

            // Background — teal gradient feel (solid teal)
            var bg = CreateUI("Background", _screenRoot.transform);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = p.playerCapTeal; // Brand teal

            // Darker top gradient strip
            var topStrip = CreateUI("TopStrip", _screenRoot.transform);
            var tsRect = topStrip.GetComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0f, 0.85f);
            tsRect.anchorMax = Vector2.one;
            tsRect.offsetMin = Vector2.zero;
            tsRect.offsetMax = Vector2.zero;
            var tsImg = topStrip.AddComponent<Image>();
            tsImg.color = new Color(0.18f, 0.6f, 0.58f, 0.6f);

            // Bottom darker strip
            var botStrip = CreateUI("BotStrip", _screenRoot.transform);
            var bsRect = botStrip.GetComponent<RectTransform>();
            bsRect.anchorMin = Vector2.zero;
            bsRect.anchorMax = new Vector2(1f, 0.15f);
            bsRect.offsetMin = Vector2.zero;
            bsRect.offsetMax = Vector2.zero;
            var bsImg = botStrip.AddComponent<Image>();
            bsImg.color = new Color(0.18f, 0.6f, 0.58f, 0.6f);

            // ── Title Text: "PIZZA EMPIRE" ──
            var titleObj = CreateUI("Title", _screenRoot.transform);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.68f);
            titleRect.anchorMax = new Vector2(0.95f, 0.88f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "PIZZA\nEMPIRE";
            titleText.fontSize = 72;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyles.Bold;
            titleText.outlineWidth = 0.25f;
            titleText.outlineColor = new Color32(20, 80, 75, 255);
            titleText.textWrappingMode = TextWrappingModes.NoWrap;

            // ── Subtitle ──
            var subObj = CreateUI("Subtitle", _screenRoot.transform);
            var subRect = subObj.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.1f, 0.63f);
            subRect.anchorMax = new Vector2(0.9f, 0.69f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;

            var subText = subObj.AddComponent<TextMeshProUGUI>();
            subText.text = "Build Your Pizza Kingdom!";
            subText.fontSize = 20;
            subText.alignment = TextAlignmentOptions.Center;
            subText.color = new Color(1f, 1f, 1f, 0.8f);
            subText.fontStyle = FontStyles.Italic;

            // ── Mascot (player character holding pizza) ──
            BuildMascotUI(_screenRoot.transform);

            // ── "Tap to Start" pulsing text ──
            var tapObj = CreateUI("TapToStart", _screenRoot.transform);
            var tapRect = tapObj.GetComponent<RectTransform>();
            tapRect.anchorMin = new Vector2(0.15f, 0.06f);
            tapRect.anchorMax = new Vector2(0.85f, 0.14f);
            tapRect.offsetMin = Vector2.zero;
            tapRect.offsetMax = Vector2.zero;

            _tapText = tapObj.AddComponent<TextMeshProUGUI>();
            _tapText.text = "TAP TO START";
            _tapText.fontSize = 28;
            _tapText.alignment = TextAlignmentOptions.Center;
            _tapText.color = Color.white;
            _tapText.fontStyle = FontStyles.Bold;
            _tapText.outlineWidth = 0.15f;
            _tapText.outlineColor = new Color32(20, 80, 75, 200);
        }

        private void BuildMascotUI(Transform parent)
        {
            var p = Core.ColorPalette.Instance;

            // Mascot container centered in screen
            var mascot = CreateUI("Mascot", parent);
            var mRect = mascot.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.15f, 0.2f);
            mRect.anchorMax = new Vector2(0.85f, 0.62f);
            mRect.offsetMin = Vector2.zero;
            mRect.offsetMax = Vector2.zero;

            // Glow circle behind mascot
            var glow = CreateUI("Glow", mascot.transform);
            var glowRect = glow.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.1f, 0.0f);
            glowRect.anchorMax = new Vector2(0.9f, 0.95f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;
            var glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(1f, 1f, 1f, 0.15f);

            // Body (capsule shape — tall rounded rect)
            var body = CreateUI("Body", mascot.transform);
            var bodyRect = body.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.32f, 0.08f);
            bodyRect.anchorMax = new Vector2(0.68f, 0.55f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;
            var bodyImg = body.AddComponent<Image>();
            bodyImg.color = p.playerSkin;

            // Apron (white front)
            var apron = CreateUI("Apron", mascot.transform);
            var apronRect = apron.GetComponent<RectTransform>();
            apronRect.anchorMin = new Vector2(0.36f, 0.12f);
            apronRect.anchorMax = new Vector2(0.64f, 0.48f);
            apronRect.offsetMin = Vector2.zero;
            apronRect.offsetMax = Vector2.zero;
            var apronImg = apron.AddComponent<Image>();
            apronImg.color = p.playerApron;

            // Head (circle)
            var head = CreateUI("Head", mascot.transform);
            var headRect = head.GetComponent<RectTransform>();
            headRect.anchorMin = new Vector2(0.33f, 0.52f);
            headRect.anchorMax = new Vector2(0.67f, 0.82f);
            headRect.offsetMin = Vector2.zero;
            headRect.offsetMax = Vector2.zero;
            var headImg = head.AddComponent<Image>();
            headImg.color = p.playerSkin;

            // Cap (teal)
            var cap = CreateUI("Cap", mascot.transform);
            var capRect = cap.GetComponent<RectTransform>();
            capRect.anchorMin = new Vector2(0.3f, 0.76f);
            capRect.anchorMax = new Vector2(0.7f, 0.88f);
            capRect.offsetMin = Vector2.zero;
            capRect.offsetMax = Vector2.zero;
            var capImg = cap.AddComponent<Image>();
            capImg.color = new Color(0.15f, 0.65f, 0.62f); // Darker teal for contrast

            // Cap brim
            var brim = CreateUI("Brim", mascot.transform);
            var brimRect = brim.GetComponent<RectTransform>();
            brimRect.anchorMin = new Vector2(0.28f, 0.74f);
            brimRect.anchorMax = new Vector2(0.72f, 0.78f);
            brimRect.offsetMin = Vector2.zero;
            brimRect.offsetMax = Vector2.zero;
            var brimImg = brim.AddComponent<Image>();
            brimImg.color = new Color(0.15f, 0.65f, 0.62f);

            // Eyes (two small dark rects)
            var eyeL = CreateUI("EyeL", mascot.transform);
            var elRect = eyeL.GetComponent<RectTransform>();
            elRect.anchorMin = new Vector2(0.41f, 0.64f);
            elRect.anchorMax = new Vector2(0.46f, 0.70f);
            elRect.offsetMin = Vector2.zero;
            elRect.offsetMax = Vector2.zero;
            var elImg = eyeL.AddComponent<Image>();
            elImg.color = new Color(0.15f, 0.12f, 0.18f);

            var eyeR = CreateUI("EyeR", mascot.transform);
            var erRect = eyeR.GetComponent<RectTransform>();
            erRect.anchorMin = new Vector2(0.54f, 0.64f);
            erRect.anchorMax = new Vector2(0.59f, 0.70f);
            erRect.offsetMin = Vector2.zero;
            erRect.offsetMax = Vector2.zero;
            var erImg = eyeR.AddComponent<Image>();
            erImg.color = new Color(0.15f, 0.12f, 0.18f);

            // Smile
            var smile = CreateUI("Smile", mascot.transform);
            var smRect = smile.GetComponent<RectTransform>();
            smRect.anchorMin = new Vector2(0.44f, 0.57f);
            smRect.anchorMax = new Vector2(0.56f, 0.60f);
            smRect.offsetMin = Vector2.zero;
            smRect.offsetMax = Vector2.zero;
            var smImg = smile.AddComponent<Image>();
            smImg.color = new Color(0.8f, 0.35f, 0.3f);

            // Left arm
            var armL = CreateUI("ArmL", mascot.transform);
            var alRect = armL.GetComponent<RectTransform>();
            alRect.anchorMin = new Vector2(0.2f, 0.25f);
            alRect.anchorMax = new Vector2(0.33f, 0.50f);
            alRect.offsetMin = Vector2.zero;
            alRect.offsetMax = Vector2.zero;
            var alImg = armL.AddComponent<Image>();
            alImg.color = p.playerSkin;

            // Right arm (holding up pizza)
            var armR = CreateUI("ArmR", mascot.transform);
            var arRect = armR.GetComponent<RectTransform>();
            arRect.anchorMin = new Vector2(0.67f, 0.38f);
            arRect.anchorMax = new Vector2(0.80f, 0.62f);
            arRect.offsetMin = Vector2.zero;
            arRect.offsetMax = Vector2.zero;
            var arImg = armR.AddComponent<Image>();
            arImg.color = p.playerSkin;

            // Pizza on right hand (stack of discs)
            // Pizza base (crust)
            var pizzaBase = CreateUI("PizzaBase", mascot.transform);
            var pbRect = pizzaBase.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0.62f, 0.58f);
            pbRect.anchorMax = new Vector2(0.88f, 0.66f);
            pbRect.offsetMin = Vector2.zero;
            pbRect.offsetMax = Vector2.zero;
            var pbImg = pizzaBase.AddComponent<Image>();
            pbImg.color = p.pizzaCrust;

            // Pizza sauce
            var pizzaSauce = CreateUI("PizzaSauce", mascot.transform);
            var psRect = pizzaSauce.GetComponent<RectTransform>();
            psRect.anchorMin = new Vector2(0.64f, 0.62f);
            psRect.anchorMax = new Vector2(0.86f, 0.67f);
            psRect.offsetMin = Vector2.zero;
            psRect.offsetMax = Vector2.zero;
            var psImg = pizzaSauce.AddComponent<Image>();
            psImg.color = p.pizzaSauce;

            // Pizza cheese top
            var pizzaCheese = CreateUI("PizzaCheese", mascot.transform);
            var pcRect = pizzaCheese.GetComponent<RectTransform>();
            pcRect.anchorMin = new Vector2(0.65f, 0.65f);
            pcRect.anchorMax = new Vector2(0.85f, 0.69f);
            pcRect.offsetMin = Vector2.zero;
            pcRect.offsetMax = Vector2.zero;
            var pcImg = pizzaCheese.AddComponent<Image>();
            pcImg.color = p.pizzaCheese;

            // Second pizza stacked
            var pizza2 = CreateUI("Pizza2", mascot.transform);
            var p2Rect = pizza2.GetComponent<RectTransform>();
            p2Rect.anchorMin = new Vector2(0.63f, 0.68f);
            p2Rect.anchorMax = new Vector2(0.87f, 0.74f);
            p2Rect.offsetMin = Vector2.zero;
            p2Rect.offsetMax = Vector2.zero;
            var p2Img = pizza2.AddComponent<Image>();
            p2Img.color = p.pizzaCrust;

            // Second pizza cheese
            var pizza2c = CreateUI("Pizza2Cheese", mascot.transform);
            var p2cRect = pizza2c.GetComponent<RectTransform>();
            p2cRect.anchorMin = new Vector2(0.65f, 0.72f);
            p2cRect.anchorMax = new Vector2(0.85f, 0.76f);
            p2cRect.offsetMin = Vector2.zero;
            p2cRect.offsetMax = Vector2.zero;
            var p2cImg = pizza2c.AddComponent<Image>();
            p2cImg.color = p.pizzaCheese;
        }

        private static GameObject CreateUI(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private void OnDestroy()
        {
            _input?.Player.Disable();
            if (_instance == this) _instance = null;
        }
    }
}
