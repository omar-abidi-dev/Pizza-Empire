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

        [SerializeField] private GameObject _plant;
        [SerializeField] private GameObject _plantTall;

        [Header("Character kit (rig parts)")]
        [SerializeField] private GameObject _charTorso;
        [SerializeField] private GameObject _charHead;
        [SerializeField] private GameObject _charArm;
        [SerializeField] private GameObject _charLeg;
        [SerializeField] private GameObject _charCap;
        [SerializeField] private GameObject _charHairShort;
        [SerializeField] private GameObject _charHairBun;
        [SerializeField] private GameObject _charChefHat;
        [SerializeField] private GameObject _charApron;

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
        public GameObject Plant => _plant;
        public GameObject PlantTall => _plantTall;
        public GameObject CharTorso => _charTorso;
        public GameObject CharHead => _charHead;
        public GameObject CharArm => _charArm;
        public GameObject CharLeg => _charLeg;
        public GameObject CharCap => _charCap;
        public GameObject CharHairShort => _charHairShort;
        public GameObject CharHairBun => _charHairBun;
        public GameObject CharChefHat => _charChefHat;
        public GameObject CharApron => _charApron;

        /// <summary>True when the rig parts needed for model characters are assigned.</summary>
        public bool HasCharacterKit => _charTorso != null && _charHead != null && _charArm != null && _charLeg != null;

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
        /// <summary>Where items stack relative to the station root, before its yaw (oven v2 shelf sits in front).</summary>
        public static Vector3 StationTrayOffset(int foodType) => foodType == 0 ? new Vector3(0f, 0f, 0.48f) : Vector3.zero;

        public static float StationSurfaceHeight(int restaurantIndex) => restaurantIndex switch
        {
            0 => 0.80f,   // oven v2: front pickup shelf
            1 => 0.90f,
            2 => 0.93f,
            3 => 0.95f,
            _ => 0f
        };
    }
}
