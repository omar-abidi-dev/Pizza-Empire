using TMPro;
using UnityEngine;

namespace ShopIdle.Customers
{
    /// <summary>Text and sizing of a customer's order bubble (one coloured line per wanted food).</summary>
    public static class OrderBubble
    {
        /// <summary>Grow the bubble to fit one line per wanted food, keeping its bottom above the head.</summary>
        public static void Fit(Transform bubble, TextMeshPro text, int lines)
        {
            const float LineHeight = 0.3f;
            float h = 0.2f + LineHeight * lines;
            var lp = text.transform.localPosition;
            text.transform.localPosition = new Vector3(0f, lp.y, lp.z);
            text.rectTransform.sizeDelta = new Vector2(1.25f, LineHeight * lines);
            text.enableAutoSizing = true;
            text.fontSizeMin = 1f;
            text.fontSizeMax = 2.4f;
            text.alignment = TextAlignmentOptions.Center;

            var bg = bubble.Find("BubbleBackground");
            if (bg != null) bg.localScale = new Vector3(1.4f, h, 1f);
            var bp = bubble.localPosition;
            bubble.localPosition = new Vector3(bp.x, 2.2f + (h - 0.6f) * 0.5f, bp.z);
        }

        /// <summary>"x3" for a single-food menu, otherwise one coloured line per wanted food.</summary>
        public static string Label(int[] order, int remaining)
        {
            if (order.Length == 1) return $"x{remaining}";
            var sb = new System.Text.StringBuilder();
            for (int t = 0; t < order.Length; t++)
            {
                if (order[t] <= 0) continue;
                if (sb.Length > 0) sb.Append('\n');
                string hex = ColorUtility.ToHtmlStringRGB(Kitchen.FoodCatalog.Color(t) * 0.6f);
                sb.Append($"<color=#{hex}>{order[t]} {Kitchen.FoodCatalog.Name(t)}</color>");
            }
            return sb.ToString();
        }
    }
}
