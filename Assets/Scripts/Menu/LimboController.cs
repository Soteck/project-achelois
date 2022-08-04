using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Menu {
    public class LimboController : BaseMenuController {
        
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button cancelButton;
        
        public CustomSelectableButton medicClass;
        public CustomSelectableButton engineerClass;
        public CustomSelectableButton soldierClass;
        public CustomSelectableButton covertClass;
        public CustomSelectableButton fieldClass;

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
            });

            cancelButton.onClick.AddListener(async () => {
                cancelSource.Play();
            });
        }

        private void OnClickCallback(CustomSelectableButton button) {
            clickSource.Play();
            foreach (CustomSelectableButton customSelectableButton in _all_classes) {
                customSelectableButton.selected = (button == customSelectableButton);
            }
        }

    }
}