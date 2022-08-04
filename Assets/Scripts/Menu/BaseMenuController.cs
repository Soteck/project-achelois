using UnityEngine;
using Util;

namespace Menu {
    public class BaseMenuController : MonoBehaviour {
        
        public AudioClip clickSound;
        public AudioClip cancelSound;
        
        protected AudioSource _clickSource;
        protected AudioSource _cancelSource;
        
        
        protected void Start() {
            _clickSource = AudioUtil.AddAudio(gameObject, false, false, 1f, clickSound);
            _cancelSource = AudioUtil.AddAudio(gameObject, false, false, 1f, cancelSound);
        }
    }
}