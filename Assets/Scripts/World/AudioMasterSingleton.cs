using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace World {
    public class AudioMasterSingleton : Singleton<AudioMasterSingleton> {

        private Dictionary<string, Dictionary<string, AudioSource>> _registeredAudioSources = new Dictionary<string, Dictionary<string, AudioSource>>();

        public AudioSource Register(string category, string id) {
            if (!_registeredAudioSources.ContainsKey(category)) {
                _registeredAudioSources[category] = new Dictionary<string, AudioSource>();
            }
            if (_registeredAudioSources[category].ContainsKey(id)) {
                // throw new Exception("Cannot register AudioSource with ID " + id + " it already exists.");
                return _registeredAudioSources[category][id];
            }

            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            _registeredAudioSources[category][id] = audioSource;
            return audioSource;
        }

        public AudioSource GetAudioSource(string category, string id) {
            return _registeredAudioSources[category]?[id];
        }

    }
}