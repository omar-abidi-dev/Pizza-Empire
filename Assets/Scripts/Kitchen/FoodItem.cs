using UnityEngine;

namespace ShopIdle.Kitchen
{
    /// <summary>Tags a food visual with its menu type (see <see cref="FoodCatalog"/>).</summary>
    public class FoodItem : MonoBehaviour
    {
        [SerializeField] private int _type;

        public int Type => _type;

        public void SetType(int type) { _type = type; }

        /// <summary>Type of a food transform; untagged items count as pizza.</summary>
        public static int TypeOf(Transform t)
        {
            var item = t != null ? t.GetComponent<FoodItem>() : null;
            return item != null ? item.Type : 0;
        }
    }
}
