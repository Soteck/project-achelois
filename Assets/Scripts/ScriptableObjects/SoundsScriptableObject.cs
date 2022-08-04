using UnityEngine;

namespace ScriptableObjects {
    
    [CreateAssetMenu(fileName = "Sounds", menuName = "ScriptableObjects/SoundsScriptableObject", order = 1)]
    public class SoundsScriptableObject : ScriptableObject {

        public AudioClip clickSound;
        public AudioClip cancelSound;
        
        public AudioClip hitBodySound;
        public AudioClip hitHeadSound;
        public AudioClip hitTeamBodySound;
        public AudioClip hitTeamHeadSound;
        
    }
}