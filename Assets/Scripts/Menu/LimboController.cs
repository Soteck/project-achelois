using System.Collections.Generic;
using Enums;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using NetworkPlayer = Network.NetworkPlayer;

namespace Menu {
    public class LimboController : BaseMenuController {
        
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button cancelButton;
        
        public GameRoleCSB medicClass;
        public GameRoleCSB engineerClass;
        public GameRoleCSB soldierClass;
        public GameRoleCSB covertClass;
        public GameRoleCSB fieldClass;
        public MainUIMenuController uiController;

        public TMP_Dropdown teamDropDown;

        private List<GameRoleCSB> _all_classes = new List<GameRoleCSB>();
        private GameRole _selectedRole;


        protected new void Start() {
            base.Start();
            
            _all_classes.Add(medicClass);
            _all_classes.Add(engineerClass);
            _all_classes.Add(soldierClass);
            _all_classes.Add(covertClass);
            _all_classes.Add(fieldClass);
            
            medicClass.OnButtonClickCallback += OnClickCallback;
            engineerClass.OnButtonClickCallback += OnClickCallback;
            soldierClass.OnButtonClickCallback += OnClickCallback;
            covertClass.OnButtonClickCallback += OnClickCallback;
            fieldClass.OnButtonClickCallback += OnClickCallback;
            
            acceptButton.onClick.AddListener(async () => {
                clickSource.Play();
                NetworkPlayer.networkPlayerOwner.RequestJoinTeam(GetSelectedGameTeam(), GetSelectedGameRole());
                uiController.HideAllElements();
            });

            cancelButton.onClick.AddListener(async () => {
                cancelSource.Play();
                uiController.HideAllElements();
            });
        }

        private GameTeam GetSelectedGameTeam() {
            int value = teamDropDown.value;
            
            if (value == 1) {
                return GameTeam.TeamA;
            }
            
            if (value == 2) {
                return GameTeam.TeamB;
            }
            
            return GameTeam.Spectator;
        }

        private GameRole GetSelectedGameRole() {
            return _selectedRole;
        }

        private void OnClickCallback(CustomSelectableButton button) {
            clickSource.Play();
            foreach (CustomSelectableButton customSelectableButton in _all_classes) {
                customSelectableButton.selected = (button == customSelectableButton);
            }
            //TODO: Ugly AF?
            this._selectedRole = ((GameRoleCSB) button).gameRole;
        }

    }
}