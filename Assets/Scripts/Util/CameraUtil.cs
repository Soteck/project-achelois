using UnityEngine;

namespace Util {
    public static class CameraUtil {
        public static void DisableAllCameras(Camera dontDisable) {
            Camera[] cameras = Camera.allCameras;
            foreach (Camera camera in cameras) {
                bool status = dontDisable != null && camera == dontDisable;
                camera.enabled = status;
                SetEnabledToAudioListenerFromCamera(camera, status);
            }
        }

        public static void SetEnabledToAudioListenerFromCamera(Camera camera, bool status) {
            if (camera != null) {
                AudioListener al = camera.GetComponent<AudioListener>();
                if (al) {
                    al.enabled = status;
                }
            }
            
        }
    }
}