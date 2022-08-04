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
            clickSource = AudioUtil.AddAudio(gameObject, false, false, 1f, _soundsScriptableObject.clickSound);
            cancelSource = AudioUtil.AddAudio(gameObject, false, false, 1f, _soundsScriptableObject.cancelSound);
        }
    }
}