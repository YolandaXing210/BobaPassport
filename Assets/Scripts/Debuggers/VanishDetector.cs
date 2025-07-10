using UnityEngine;

namespace BobaShooter
{
    /// <summary>
    /// Simple vanish detector - attach to any object you want to monitor for disappearing
    /// Provides immediate alerts when object becomes invisible or inactive
    /// </summary>
    public class VanishDetector : MonoBehaviour
    {
        [Header("Detection Settings")] [SerializeField]
        private bool enableDetection = true;
        [SerializeField] GameObject trackedGameObject;
        [SerializeField] Renderer trackedRenderer;

        [SerializeField] private float checkInterval = 1f; // Check every second
        [SerializeField] private bool alertOnInactive = true;
        [SerializeField] private bool alertOnInvisible = true;
        [SerializeField] private bool alertOnFarAway = true;
        [SerializeField] private float maxDistanceFromCamera = 50f;

        [Header("Debug Info")] [SerializeField]
        private bool showCurrentStatus = true;

        private float lastCheckTime;
        private bool wasVisibleLastFrame = true;
        private bool wasActiveLastFrame = true;
        private string objectName;

        private void Start()
        {
            objectName = trackedGameObject.name;
            wasActiveLastFrame = trackedGameObject.activeInHierarchy;
            wasVisibleLastFrame = IsVisible();

            if (enableDetection)
            {
                Debug.Log(
                    $"[VanishDetector] Monitoring {objectName} - Initial: Active={wasActiveLastFrame}, Visible={wasVisibleLastFrame}");
            }
        }

        private void Update()
        {
            if (!enableDetection) return;

            if (Time.time - lastCheckTime >= checkInterval)
            {
                CheckForVanishing();
                lastCheckTime = Time.time;
            }
        }

        private void CheckForVanishing()
        {
            bool currentlyActive = trackedGameObject.activeInHierarchy;
            bool currentlyVisible = IsVisible();

            // Check for state changes
            if (alertOnInactive && currentlyActive != wasActiveLastFrame)
            {
                if (!currentlyActive)
                {
                    Debug.LogError(
                        $"[VanishDetector] {objectName} BECAME INACTIVE! Parent: {(transform.parent ? transform.parent.name : "null")}");
                }
                else
                {
                    Debug.LogWarning($"[VanishDetector] {objectName} became active again");
                }

                wasActiveLastFrame = currentlyActive;
            }

            if (alertOnInvisible && currentlyVisible != wasVisibleLastFrame)
            {
                if (!currentlyVisible)
                {
                    Vector3 pos = transform.position;
                    Vector3 scale = transform.localScale;
                    Debug.LogError($"[VanishDetector] {objectName} BECAME INVISIBLE! Pos={pos:F2}, Scale={scale:F3}");
                }
                else
                {
                    Debug.LogWarning($"[VanishDetector] {objectName} became visible again");
                }

                wasVisibleLastFrame = currentlyVisible;
            }

            // Check distance from camera
            if (alertOnFarAway && currentlyActive)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    float distance = Vector3.Distance(transform.position, cam.transform.position);
                    if (distance > maxDistanceFromCamera)
                    {
                        Debug.LogError(
                            $"[VanishDetector] {objectName} TOO FAR FROM CAMERA! Distance: {distance:F1}m at {transform.position:F2}");
                    }
                }
            }

            // Show current status if enabled
            if (showCurrentStatus)
            {
                LogCurrentStatus();
            }
        }

        private bool IsVisible()
        {
           // Renderer[] renderers = GetComponentsInChildren<Renderer>();
            /*foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.enabled && renderer.isVisible)
                {
                    return true;
                }
            }*/
            
            if (trackedRenderer != null && trackedRenderer.enabled && trackedRenderer.isVisible)
            {
                return true;
            }

            return false;
        }

        private void LogCurrentStatus()
        {
            bool active = trackedGameObject.activeInHierarchy;
            bool visible = IsVisible();
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;

            Camera cam = Camera.main;
            float dist = cam != null ? Vector3.Distance(pos, cam.transform.position) : -1f;

            string status =
                $"[VanishDetector] {objectName} Status: Active={active}, Visible={visible}, Pos={pos:F2}, Scale={scale:F3}, CamDist={dist:F1}";

            if (!active || !visible || dist > maxDistanceFromCamera)
            {
                Debug.LogWarning(status + " ⚠️ POTENTIAL ISSUE");
            }
            else
            {
                Debug.Log(status + " ✓ OK");
            }
        }

        // Public method to force a check
        public void ForceCheck()
        {
            Debug.LogWarning($"[VanishDetector] FORCE CHECK for {objectName}:");
            LogCurrentStatus();
            CheckForVanishing();
        }

        // Public method to enable/disable detection
        public void SetDetectionEnabled(bool enabled)
        {
            enableDetection = enabled;
            Debug.Log($"[VanishDetector] Detection {(enabled ? "enabled" : "disabled")} for {objectName}");
        }

        // Called when object is destroyed
        private void OnDestroy()
        {
            if (enableDetection)
            {
                Debug.LogError($"[VanishDetector] {objectName} OBJECT DESTROYED!");
            }
        }
    }
}