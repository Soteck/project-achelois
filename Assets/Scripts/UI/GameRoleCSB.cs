using Enums;
using UnityEngine;

namespace UI {
    public class GameRoleCSB : CustomSelectableButton {
        
        [SerializeField]
        private GameRole _gameRole;

        public GameRole gameRole => _gameRole;
    }
}