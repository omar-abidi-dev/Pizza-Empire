using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// References to the imported .glb prop models (Assets/Models).
    /// Lives at Resources/PropModels.asset so runtime-built visuals can reach it.
    /// A null entry means "no model yet" and callers fall back to primitives.
    /// </summary>
    [CreateAssetMenu(menuName = "ShopIdle/Prop Models", fileName = "PropModels")]
    public class PropModels : ScriptableObject
    {
        [SerializeField] private GameObject _counter;
        [SerializeField] private GameObject _diningTable;
        [SerializeField] private GameObject _chair;
        [SerializeField] private GameObject _trashBin;
        [SerializeField] private GameObject _driveThruBooth;
        [SerializeField] private GameObject _cashStack;
        [SerializeField] private GameObject _pizza;
        [SerializeField] private GameObject _burgerGrill;
        [SerializeField] private GameObject _sushiCounter;
        [SerializeField] private GameObject _tacoCart;
        [SerializeField] private GameObject _oven;

        public GameObject Counter => _counter;
        public GameObject DiningTable => _diningTable;
        public GameObject Chair => _chair;
        public GameObject TrashBin => _trashBin;
        public GameObject DriveThruBooth => _driveThruBooth;
        public GameObject CashStack => _cashStack;
        public GameObject Pizza => _pizza;
        public GameObject BurgerGrill => _burgerGrill;
        public GameObject SushiCounter => _sushiCounter;
        public GameObject TacoCart => _tacoCart;
        public GameObject Oven => _oven;

        private static PropModels _instance;
        private static bool _loaded;

        /// <summary>Loaded once from Resources; may be null if the asset is missing.</summary>
        public static PropModels Instance
        {
            get
            {
                if (!_loaded)
                {
                    _instance = Resources.Load<PropModels>("PropModels");
                    _loaded = true;
                }
                return _instance;
            }
        }

        /// <summary>Cooking-station model for a restaurant index (null = use the primitive oven).</summary>
        public GameObject StationFor(int restaurantIndex) => restaurantIndex switch
        {
            0 => _oven,
            1 => _burgerGrill,
            2 => _sushiCounter,
            3 => _tacoCart,
            _ => null
        };

        /// <summary>
        /// Height (m above the floor) of the surface food rests on for each station model,
        /// measured from the model scripts: oven roof, griddle plate, conveyor rails, cart worktop.
        /// </summary>
        public static float StationSurfaceHeight(int restaurantIndex) => restaurantIndex switch
        {
            0 => 1.12f,
            1 => 0.90f,
            2 => 0.93f,
            3 => 0.95f,
            _ => 0f
        };
    }
}
