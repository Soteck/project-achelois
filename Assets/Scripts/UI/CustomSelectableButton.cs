using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class CustomSelectableButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler {
        public RectTransform backgrondNormal;
        public RectTransform backgrondHover;
        public RectTransform backgrondSelected;
        public RectTransform backgrondHoverSelected;

        public event Action<CustomSelectableButton> OnButtonClickCallback = null;

        private bool _selected;
        private bool _hover;

        internal void InvokeOnPointerClick() => OnButtonClickCallback?.Invoke(this);

        public bool selected {
            get => _selected;

            set {
                _selected = value;
                Redraw();
            }
        }

        public bool hover {
            get => _hover;

            set {
                _hover = value;
                Redraw();
            }
        }

        private void Redraw() {
            if (selected) {
                backgrondNormal.gameObject.SetActive(false);
                if (hover) {
                    backgrondSelected.gameObject.SetActive(false);
                    backgrondHover.gameObject.SetActive(false);
                    backgrondHoverSelected.gameObject.SetActive(true);
                } else {
                    backgrondSelected.gameObject.SetActive(true);
                    backgrondHover.gameObject.SetActive(false);
                    backgrondHoverSelected.gameObject.SetActive(false);
                }
            } else {
                backgrondSelected.gameObject.SetActive(false);
                if (hover) {
                    backgrondHover.gameObject.SetActive(true);
                    backgrondNormal.gameObject.SetActive(false);
                    backgrondHoverSelected.gameObject.SetActive(false);
                } else {
                    backgrondNormal.gameObject.SetActive(true);
                    backgrondHover.gameObject.SetActive(false);
                    backgrondHoverSelected.gameObject.SetActive(false);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            hover = false;
        }

        public void OnPointerClick(PointerEventData eventData) {
            InvokeOnPointerClick();
        }
    }
}