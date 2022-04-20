using UnityEngine;

namespace Util {
    public static class CameraUtil {
        public static void DisableAllCameras(Camera dontDisable) {
            //Camera[] cameras = FindObjectsOfType(typeof(Camera)) as Camera[];
            Camera[] cameras = Camera.allCameras;
            foreach (Camera camera in cameras) {
                if (dontDisable != null) {
                    if (camera == dontDisable) {
                        camera.enabled = true;
                    }
                    else {
                        camera.enabled = false;
                    }
                }
                else {
                    camera.enabled = false;
                }
            }
        }
    }
}