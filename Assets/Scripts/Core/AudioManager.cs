using UnityEngine;
using UnityEngine.UI;

namespace ShopIdle.Core
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        [SerializeField] private int _poolSize = 6;

        private AudioSource[] _pool;
        private int _nextSource;
        private bool _muted;

        // Procedural clips (generated once)
        private AudioClip _clipPizzaPickup;
        private AudioClip _clipPizzaDrop;
        private AudioClip _clipChaChing;
        private AudioClip _clipCoinTick;
        private AudioClip _clipWhoosh;
        private AudioClip _clipFanfare;

        private GameObject _muteButton;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            _pool = new AudioSource[_poolSize];
            for (int i = 0; i < _poolSize; i++)
            {
                _pool[i] = gameObject.AddComponent<AudioSource>();
                _pool[i].playOnAwake = false;
                _pool[i].spatialBlend = 0f;
            }

            GenerateClips();
        }

        private void Start()
        {
            CreateMuteButton();
        }

        public void Play(SoundType type, float volume = 1f)
        {
            if (_muted) return;

            AudioClip clip = type switch
            {
                SoundType.PizzaPickup => _clipPizzaPickup,
                SoundType.PizzaDrop => _clipPizzaDrop,
                SoundType.ChaChing => _clipChaChing,
                SoundType.CoinTick => _clipCoinTick,
                SoundType.Whoosh => _clipWhoosh,
                SoundType.Fanfare => _clipFanfare,
                _ => null
            };

            if (clip == null) return;

            var src = _pool[_nextSource];
            _nextSource = (_nextSource + 1) % _pool.Length;
            src.clip = clip;
            src.volume = Mathf.Clamp01(volume);
            src.pitch = Random.Range(0.92f, 1.08f);
            src.Play();
        }

        private void ToggleMute()
        {
            _muted = !_muted;
            UpdateMuteVisual();
        }

        private void UpdateMuteVisual()
        {
            if (_muteButton == null) return;
            var txt = _muteButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt != null)
                txt.text = _muted ? "<s>♪</s>" : "♪";
        }

        private void CreateMuteButton()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _muteButton = new GameObject("MuteButton");
            _muteButton.transform.SetParent(canvas.transform, false);

            var rect = _muteButton.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-10f, -10f);
            rect.sizeDelta = new Vector2(60f, 60f);

            var img = _muteButton.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.35f);

            var btn = _muteButton.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(ToggleMute);

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(_muteButton.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var tmp = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = "♪";
            tmp.fontSize = 28;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        // ── Procedural audio generation ──

        private void GenerateClips()
        {
            _clipPizzaPickup = MakeClip("PizzaPickup", 0.12f, PopWave);
            _clipPizzaDrop = MakeClip("PizzaDrop", 0.15f, ThudWave);
            _clipChaChing = MakeClip("ChaChing", 0.35f, ChaChingWave);
            _clipCoinTick = MakeClip("CoinTick", 0.08f, CoinTickWave);
            _clipWhoosh = MakeClip("Whoosh", 0.25f, WhooshWave);
            _clipFanfare = MakeClip("Fanfare", 0.5f, FanfareWave);
        }

        private delegate float WaveFunc(float t, float duration);

        private static AudioClip MakeClip(string name, float duration, WaveFunc wave)
        {
            int sampleRate = 22050;
            int samples = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                data[i] = Mathf.Clamp(wave(t, duration), -1f, 1f);
            }

            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Pop: quick sine burst rising in pitch
        private static float PopWave(float t, float dur)
        {
            float env = 1f - t / dur;
            float freq = Mathf.Lerp(600f, 1200f, t / dur);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.6f;
        }

        // Thud: low sine with fast decay
        private static float ThudWave(float t, float dur)
        {
            float env = Mathf.Exp(-12f * t);
            return Mathf.Sin(2f * Mathf.PI * 180f * t) * env * 0.7f;
        }

        // Cha-ching: two-tone bell
        private static float ChaChingWave(float t, float dur)
        {
            float env = Mathf.Exp(-5f * t);
            float bell1 = Mathf.Sin(2f * Mathf.PI * 1400f * t);
            float bell2 = Mathf.Sin(2f * Mathf.PI * 1800f * t);
            float hit = t < 0.15f ? bell1 : bell2;
            return hit * env * 0.5f;
        }

        // Coin tick: very short high ping
        private static float CoinTickWave(float t, float dur)
        {
            float env = 1f - t / dur;
            return Mathf.Sin(2f * Mathf.PI * 2200f * t) * env * env * 0.4f;
        }

        // Whoosh: filtered noise sweep
        private static float WhooshWave(float t, float dur)
        {
            float env = Mathf.Sin(Mathf.PI * t / dur);
            float noise = Mathf.PerlinNoise(t * 800f, 0f) * 2f - 1f;
            return noise * env * 0.35f;
        }

        // Fanfare: ascending arpeggio
        private static float FanfareWave(float t, float dur)
        {
            float env = Mathf.Clamp01(1f - t / dur);
            float note = t < 0.12f ? 523f   // C5
                       : t < 0.24f ? 659f   // E5
                       : t < 0.36f ? 784f   // G5
                       : 1047f;              // C6
            return Mathf.Sin(2f * Mathf.PI * note * t) * env * 0.5f;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }

    public enum SoundType
    {
        PizzaPickup,
        PizzaDrop,
        ChaChing,
        CoinTick,
        Whoosh,
        Fanfare
    }
}
