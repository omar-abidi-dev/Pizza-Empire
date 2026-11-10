using UnityEngine;
using TMPro;

namespace ShopIdle.Core
{
    public class JuiceManager : MonoBehaviour
    {
        private static JuiceManager _instance;
        public static JuiceManager Instance => _instance;

        private static Material _particleMat;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        // ── Floating text ──

        public void SpawnFloatingText(string text, Vector3 worldPos, Color color, float fontSize = 5f)
        {
            var obj = new GameObject("FloatingText");
            obj.transform.position = worldPos;
            obj.AddComponent<UI.Billboard>();

            var tmp = obj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.15f;
            tmp.outlineColor = new Color32(0, 0, 0, 180);
            tmp.rectTransform.sizeDelta = new Vector2(4f, 2f);

            obj.AddComponent<FloatingText>();
        }

        // ── Particles ──

        public void SpawnSparkle(Vector3 pos, Color color, int count = 12)
        {
            var obj = new GameObject("Sparkle");
            obj.transform.position = pos;

            var ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.6f;
            main.startSpeed = 3f;
            main.startSize = 0.12f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = count;
            main.gravityModifier = 0.5f;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMat();

            ps.Play();
            Destroy(obj, 1f);
        }

        public void SpawnCoinBurst(Vector3 pos, int count = 8)
        {
            SpawnSparkle(pos, new Color(1f, 0.85f, 0.1f), count);
        }

        public void SpawnSteamPuff(Vector3 pos)
        {
            var obj = new GameObject("SteamPuff");
            obj.transform.position = pos;

            var ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.2f;
            main.startSpeed = 0.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startColor = new Color(0.9f, 0.9f, 0.95f, 0.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 15;
            main.gravityModifier = -0.1f;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 5) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 0.5f, 1f, 1.5f));

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.4f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLife.color = gradient;

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMat();

            ps.Play();
            Destroy(obj, 2f);
        }

        // ── Camera Shake ──

        public void ShakeCamera(float intensity = 0.3f)
        {
            var cam = Object.FindAnyObjectByType<CameraSystem.CameraFollow>();
            if (cam != null)
                cam.Shake(intensity);
        }

        // ── Helpers ──

        private static Material GetParticleMat()
        {
            if (_particleMat != null) return _particleMat;
            // Try URP particle shader first, fall back to built-in
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
                shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            _particleMat = new Material(shader);
            _particleMat.SetFloat("_Surface", 1f); // Transparent
            _particleMat.SetOverrideTag("RenderType", "Transparent");
            _particleMat.renderQueue = 3000;
            return _particleMat;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
