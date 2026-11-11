using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace ShopIdle.Core
{
    /// <summary>
    /// Every 4 minutes, triggers a "RUSH HOUR!" event:
    /// customers spawn 2x faster and pay 2x for 45 seconds.
    /// </summary>
    public class RushHourManager : MonoBehaviour
    {
        private static RushHourManager _instance;
        public static RushHourManager Instance => _instance;

        private const float RushCooldown = 240f; // 4 minutes
        private const float RushDuration = 45f;

        private float _cooldownTimer;
        private float _rushTimer;
        private bool _isRushing;

        public bool IsRushing => _isRushing;
        public float PayMultiplier => _isRushing ? 2f : 1f;
        public float SpawnMultiplier => _isRushing ? 0.5f : 1f; // halve spawn interval = 2x speed

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Update()
        {
            if (_isRushing)
            {
                _rushTimer -= Time.deltaTime;
                if (_rushTimer <= 0f)
                    EndRush();
            }
            else
            {
                _cooldownTimer += Time.deltaTime;
                if (_cooldownTimer >= RushCooldown)
                    StartRush();
            }
        }

        private void StartRush()
        {
            _isRushing = true;
            _rushTimer = RushDuration;
            _cooldownTimer = 0f;

            AudioManager.Instance?.Play(SoundType.Fanfare);
            JuiceManager.Instance?.ShakeCamera(0.3f);
            StartCoroutine(ShowRushBanner());
        }

        private void EndRush()
        {
            _isRushing = false;
            _rushTimer = 0f;
        }

        private IEnumerator ShowRushBanner()
        {
            var canvasObj = new GameObject("RushBanner");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var bgObj = new GameObject("BG");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.05f, 0.44f);
            bgRect.anchorMax = new Vector2(0.95f, 0.56f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.9f, 0.15f, 0.1f, 0.9f);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.44f);
            textRect.anchorMax = new Vector2(0.95f, 0.56f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "RUSH HOUR! x2";
            tmp.fontSize = 48;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.95f, 0.3f);
            tmp.fontStyle = FontStyles.Bold;

            // Scale punch
            canvasObj.transform.localScale = Vector3.one * 0.3f;
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(0.3f, 1.05f, t / 0.25f);
                canvasObj.transform.localScale = Vector3.one * s;
                yield return null;
            }
            canvasObj.transform.localScale = Vector3.one;

            yield return new WaitForSeconds(2f);

            // Fade
            float fade = 0f;
            while (fade < 0.5f)
            {
                fade += Time.deltaTime;
                float a = 1f - fade / 0.5f;
                bgImg.color = new Color(0.9f, 0.15f, 0.1f, 0.9f * a);
                tmp.color = new Color(1f, 0.95f, 0.3f, a);
                yield return null;
            }

            Destroy(canvasObj);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
