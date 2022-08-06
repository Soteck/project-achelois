using UnityEngine;
using World;

namespace Util {
    public static class AudioUtil {
        
        
        
        public static AudioSource AddAudio(string category, string id, bool loop, bool playAwake, float vol, AudioClip clip) {
            AudioSource newAudio = AudioMasterSingleton.Instance.Register(category, id);
            newAudio.clip = clip;
            newAudio.loop = loop;
            newAudio.playOnAwake = playAwake;
            newAudio.volume = vol;

            return newAudio;
        }
    }
}