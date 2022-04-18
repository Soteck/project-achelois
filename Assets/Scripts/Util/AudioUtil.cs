using UnityEngine;

namespace Util {
    public static class AudioUtil {
        public static AudioSource AddAudio(GameObject gameObject, bool loop, bool playAwake, float vol, AudioClip clip) {
            AudioSource newAudio = gameObject.AddComponent<AudioSource>();
            newAudio.clip = clip;
            newAudio.loop = loop;
            newAudio.playOnAwake = playAwake;
            newAudio.volume = vol;

            return newAudio;
        }
    }
}