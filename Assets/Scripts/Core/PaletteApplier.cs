using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Recolours a prefab that embeds an imported prop model from <see cref="ColorPalette"/>
    /// when it spawns (models spawned via <see cref="PropSpawner"/> don't need this).
    /// </summary>
    public class PaletteApplier : MonoBehaviour
    {
        private void Awake()
        {
            PropSpawner.ApplyPalette(gameObject);
        }
    }
}
