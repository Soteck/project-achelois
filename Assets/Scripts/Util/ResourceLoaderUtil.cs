using System;
using Core;
using ScriptableObjects;
using UnityEngine;

namespace Util {
    public class ResourceLoaderUtil : Singleton<ResourceLoaderUtil> {
        
        [SerializeField]
        private SoundsScriptableObject _soundsScriptableObject;

        public SoundsScriptableObject soundsScriptableObject => _soundsScriptableObject;
    }
}