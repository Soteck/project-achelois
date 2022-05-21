using Core;
using Map.Maps;

namespace Map {
    public class MapMaster : Singleton<MapMaster> {
        
        public IBaseMapController instance { get; set; }

        public static IBaseMapController MapInstance() {
            return Instance.instance;
        }
    }
}