using System.Collections.Generic;

namespace Core {
    public class ItemPrefabFactory : Singleton<ItemPrefabFactory> {
        public EquipableItem[] items;

        private Dictionary<string, EquipableItem> itemsMap = new Dictionary<string, EquipableItem>();

        private void Awake() {
            foreach (EquipableItem item in items) {
                itemsMap[item.item_id] = item;
            }
        }

        public static EquipableItem PrefabById(string id) {
            return Instance.itemsMap[id];
        }
    }
}