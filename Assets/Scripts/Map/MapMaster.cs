using Core;
using Map.Maps;

namespace Map {
    public class MapMaster : Singleton<MapMaster> {
        
        public BaseMapControllerInterface instance { get; set; }

        public static BaseMapControllerInterface MapInstance() {
            return Instance.instance;
        }
    }
}