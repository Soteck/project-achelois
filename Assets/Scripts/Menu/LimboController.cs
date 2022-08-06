using System.Collections.Generic;
using Enums;
using UI;
using UnityEngine;
using UnityEngine.UI;
using NetworkPlayer = Network.NetworkPlayer;

namespace Menu {
    public class LimboController : BaseMenuController {
        
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button cancelButton;
        
        public CustomSelectableButton medicClass;
        public CustomSelectableButton engineerClass;
        public CustomSelectableButton soldierClass;
        public CustomSelectableButton covertClass;
        public CustomSelectableButton fieldClass;
        public MainUIMenuController uiController; 

        private List<CustomSelectableButton> _all_classes = new List<CustomSelectableButton>();
        
        
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
            return GameTeam.Spectator;
        }

        private GameRole GetSelectedGameRole() {
            return GameRole.Covert;
        }

        private void OnClickCallback(CustomSelectableButton button) {
            clickSource.Play();
            foreach (CustomSelectableButton customSelectableButton in _all_classes) {
                customSelectableButton.selected = (button == customSelectableButton);
            }
        }

    }
}