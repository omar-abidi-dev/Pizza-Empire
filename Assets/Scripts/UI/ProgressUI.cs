using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShopIdle.UI
{
    /// <summary>
    /// Small pill under the objective card: fills as plots are bought towards the next
    /// menu milestone ("Sushi at 7 plots  4/7").
    /// </summary>
    public class ProgressUI : MonoBehaviour
    {
        private Image _fill;
        private TextMeshProUGUI _text;

        public void Init(string restaurantName)
        {
            var canvas = UIFactory.MainCanvas();
            if (canvas == null) return;
            float top = ObjectiveCard.Top + ObjectiveCard.Height + 14f;
            var pill = UIFactory.Fixed("MenuProgress", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(30f, -top), new Vector2(440f, 50f));
            _fill = UIFactory.Bar(pill, new Color(0.08f, 0.2f, 0.3f, 0.7f), new Color(1f, 0.72f, 0.3f));
            _text = UIFactory.Text(UIFactory.Rect("Text", pill, Vector2.zero, Vector2.one), "", 24, Color.white,
                TextAlignmentOptions.Center, true);
        }

        /// <summary>Fill from the last menu milestone to the next one.</summary>
        public void Show(int plotsOwned)
        {
            if (_fill == null) return;
            int size = Kitchen.Menu.SizeFor(plotsOwned);
            int prev = size == 1 ? 1 : Kitchen.Menu.MilestoneFor(size);
            int next = Kitchen.Menu.MilestoneFor(size + 1);
            UIFactory.SetFill(_fill, Mathf.Clamp01((float)(plotsOwned - prev) / (next - prev)));
            _text.text = $"New food: {Kitchen.FoodCatalog.Name(size)}  {plotsOwned}/{next} areas";
        }
    }
}
