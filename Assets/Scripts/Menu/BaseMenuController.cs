using ScriptableObjects;
using UnityEngine;
using Util;

namespace Menu {
    public class BaseMenuController : MonoBehaviour {
        protected AudioSource clickSource;
        protected AudioSource cancelSource;

        private SoundsScriptableObject _soundsScriptableObject;


        protected void Awake() {
            _soundsScriptableObject = ResourceLoaderUtil.Instance.soundsScriptableObject;
        }

        protected void Start() {
            clickSource = AudioUtil.AddAudio("menu_effects", "click", false, false, 1f,
                                             _soundsScriptableObject.clickSound);
            cancelSource = AudioUtil.AddAudio("menu_effects", "cancel", false, false, 1f,
                                              _soundsScriptableObject.cancelSound);
        }
    }
}