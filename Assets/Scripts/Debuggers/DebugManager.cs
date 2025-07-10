using UnityEngine;
using System.Collections.Generic;

namespace BobaShooter
{
    /// <summary>
    /// Simple manager for controlling debugging and providing quick checks
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        [Header("Global Debug Settings")] [SerializeField]
        private bool globalDebugEnabled = true;

        [SerializeField] private KeyCode toggleDebugKey = KeyCode.F1;
        [SerializeField] private KeyCode checkAllObjectsKey = KeyCode.F2;
        [SerializeField] private KeyCode systemInfoKey = KeyCode.F3;

        [Header("Specific Object Monitoring")] [SerializeField]
        private GameObject[] specificObjects; // Drag objects here to monitor them specifically

        private List<ObjectDebugTracker> trackers = new List<ObjectDebugTracker>();

        private void Start()
        {
            FindAllTrackers();
            LogSystemInfo();

            Debug.Log(
                $"[DebugManager] Started | F1=Toggle Debug | F2=Check All | F3=System Info | Monitoring {trackers.Count} objects");
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleDebugKey))
            {
                ToggleGlobalDebug();
            }

            if (Input.GetKeyDown(checkAllObjectsKey))
            {
                CheckAllObjects();
            }

            if (Input.GetKeyDown(systemInfoKey))
            {
                LogSystemInfo();
            }

            // Check for vanished specific objects every few seconds
            if (Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
            {
                CheckSpecificObjects();
            }
        }

        private void FindAllTrackers()
        {
            trackers.Clear();
            ObjectDebugTracker[] foundTrackers = FindObjectsByType<ObjectDebugTracker>(FindObjectsSortMode.None);
            trackers.AddRange(foundTrackers);

            Debug.Log($"[DebugManager] Found {trackers.Count} debug trackers");
        }

        public void ToggleGlobalDebug()
        {
            globalDebugEnabled = !globalDebugEnabled;

            foreach (var tracker in trackers)
            {
                if (tracker != null)
                {
                    tracker.SetDebugEnabled(globalDebugEnabled);
                }
            }

            Debug.LogWarning($"[DebugManager] Global debug {(globalDebugEnabled ? "ENABLED" : "DISABLED")}");
        }

        public void CheckAllObjects()
        {
            Debug.LogWarning("[DebugManager] === CHECKING ALL TRACKED OBJECTS ===");

            int activeCount = 0;
            int inactiveCount = 0;

            foreach (var tracker in trackers)
            {
                if (tracker != null)
                {
                    if (tracker.gameObject.activeInHierarchy)
                    {
                        activeCount++;
                        tracker.ForceLog("MANAGER_CHECK");
                    }
                    else
                    {
                        inactiveCount++;
                        Debug.LogError($"[DebugManager] INACTIVE OBJECT: {tracker.gameObject.name}");
                    }
                }
                else
                {
                    inactiveCount++;
                    Debug.LogError($"[DebugManager] NULL TRACKER (object destroyed?)");
                }
            }

            Debug.LogWarning($"[DebugManager] Status: {activeCount} active, {inactiveCount} inactive/destroyed");
        }

        private void CheckSpecificObjects()
        {
            if (specificObjects == null || specificObjects.Length == 0) return;

            foreach (var obj in specificObjects)
            {
                if (obj == null)
                {
                    Debug.LogError($"[DebugManager] SPECIFIC OBJECT IS NULL! (Was it destroyed?)");
                    continue;
                }

                if (!obj.activeInHierarchy)
                {
                    Debug.LogError($"[DebugManager] SPECIFIC OBJECT INACTIVE: {obj.name}");

                    var tracker = obj.GetComponent<ObjectDebugTracker>();
                    if (tracker != null)
                    {
                        tracker.CheckIfVanished();
                    }
                }
            }
        }

        private void LogSystemInfo()
        {
            Debug.Log(
                $"[SYSTEM] Unity {Application.unityVersion} | Platform: {Application.platform} | Memory: {SystemInfo.systemMemorySize}MB");
            Debug.Log($"[SYSTEM] Device: {SystemInfo.deviceModel} | Graphics: {SystemInfo.graphicsDeviceName}");
            Debug.Log($"[SYSTEM] Screen: {Screen.width}x{Screen.height} | FPS Target: {Application.targetFrameRate}");

            // AR info
            CheckARComponents();
        }

        private void CheckARComponents()
        {
            var arSession = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
            // var arCamera = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARCamera>();

            /*if (arSession != null)
            {
                Debug.Log($"[AR] AR Session State: {arSession.state}");
            }
            else
            {
                Debug.Log("[AR] No AR Session found");
            }*/

            /*if (arCamera != null)
            {
                Debug.Log($"[AR] AR Camera: {arCamera.name} | Enabled: {arCamera.enabled}");
            }*/
        }

        // Method to manually check if a specific object has vanished
        public void CheckObjectVanished(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[DebugManager] Cannot check null object!");
                return;
            }

            var tracker = obj.GetComponent<ObjectDebugTracker>();
            if (tracker != null)
            {
                tracker.CheckIfVanished();
            }
            else
            {
                // Manual check without tracker
                bool active = obj.activeInHierarchy;
                Vector3 pos = obj.transform.position;
                Camera cam = Camera.main;
                float dist = cam != null ? Vector3.Distance(pos, cam.transform.position) : 0f;

                Debug.LogWarning(
                    $"[DebugManager] Manual Check: {obj.name} | Active={active} | Pos={pos:F2} | CamDist={dist:F1}");
            }
        }

        // Public methods for UI buttons or external calls
        public void OnCheckAllButton() => CheckAllObjects();
        public void OnToggleDebugButton() => ToggleGlobalDebug();
        public void OnSystemInfoButton() => LogSystemInfo();
    }
}