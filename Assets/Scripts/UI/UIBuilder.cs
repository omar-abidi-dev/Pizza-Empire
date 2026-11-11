using UnityEngine;
using UnityEngine.UI;

namespace ShopIdle.UI
{
    /// <summary>
    /// Builds the HUD at startup: header (level, XP, cash, gems), objective card and a round
    /// joystick. Removes the old scene money labels it replaces.
    /// </summary>
    [DefaultExecutionOrder(60)]
    public class UIBuilder : MonoBehaviour
    {
        private static UIBuilder _instance;

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
            var canvas = UIFactory.MainCanvas();
            if (canvas == null) return;

            foreach (var old in FindObjectsByType<MoneyUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Destroy(old.gameObject);

            HudTopBar.Build(canvas.transform);
            ObjectiveCard.Build(canvas.transform);
            RestyleJoystick();
        }

        /// <summary>Round translucent base with a solid white knob.</summary>
        private static void RestyleJoystick()
        {
            var joystick = FindAnyObjectByType<VirtualJoystick>();
            if (joystick == null) return;

            var bg = joystick.GetComponent<Image>();
            if (bg != null)
            {
                bg.sprite = UISprites.Circle;
                bg.color = new Color(1f, 1f, 1f, 0.28f);
            }
            foreach (var img in joystick.GetComponentsInChildren<Image>(true))
            {
                if (img == bg) continue;
                img.sprite = UISprites.Circle; // the knob
                img.color = new Color(1f, 1f, 1f, 0.9f);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
