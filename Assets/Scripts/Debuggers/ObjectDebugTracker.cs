using UnityEngine;
using System.Collections.Generic;

namespace BobaShooter
{
    /// <summary>
    /// Focused debug tracker that logs only meaningful changes with compact, informative messages
    /// </summary>
    public class ObjectDebugTracker : MonoBehaviour
    {
        [Header("Debug Settings")] [SerializeField]
        private bool enableDebug = true;

        [SerializeField] private float logInterval = 0.1f; // Check more frequently
        [SerializeField] private bool logPositionChanges = true;
        [SerializeField] private float positionThreshold = 0.01f; // Min position change to log
        [SerializeField] private float scaleThreshold = 0.01f; // Min scale change to log

        [Header("Critical Alerts")] [SerializeField]
        private float disappearanceDistance = 100f; // Distance considered "vanished"

        [SerializeField] private float minVisibleScale = 0.001f; // Scale considered "invisible"

        // Previous state tracking
        private bool wasActive = true;
        private bool wasVisible = true;
        private bool wasEnabled = true;
        private Vector3 lastPosition;
        private Vector3 lastScale;
        private Transform lastParent;
        private float lastLogTime;
        private Camera mainCamera;
        private Renderer[] renderers;
        private string objectName;

        private void Start()
        {
            if (!enableDebug) return;

            objectName = gameObject.name;
            mainCamera = Camera.main;
            renderers = GetComponentsInChildren<Renderer>();

            // Initialize tracking variables
            wasActive = gameObject.activeInHierarchy;
            wasVisible = IsObjectVisible();
            wasEnabled = gameObject.activeSelf;
            lastPosition = transform.position;
            lastScale = transform.localScale;
            lastParent = transform.parent;

            LogCritical("STARTED",
                $"Pos={lastPosition:F2} Scale={lastScale:F3} Active={wasActive} Visible={wasVisible}");
        }

        private void Update()
        {
            if (!enableDebug) return;

            if (Time.time - lastLogTime >= logInterval)
            {
                CheckForCriticalChanges();
                lastLogTime = Time.time;
            }
        }

        private void CheckForCriticalChanges()
        {
            bool currentActive = gameObject.activeInHierarchy;
            bool currentSelfActive = gameObject.activeSelf;
            bool currentVisible = IsObjectVisible();
            Vector3 currentPos = transform.position;
            Vector3 currentScale = transform.localScale;
            Transform currentParent = transform.parent;

            // Check for active state changes (CRITICAL)
            if (currentActive != wasActive)
            {
                LogCritical("ACTIVE_CHANGED",
                    $"{wasActive}→{currentActive} Self={currentSelfActive} Pos={currentPos:F2}");
                wasActive = currentActive;
            }

            // Check for visibility changes (CRITICAL)
            if (currentVisible != wasVisible)
            {
                LogCritical("VISIBILITY_CHANGED",
                    $"{wasVisible}→{currentVisible} Active={currentActive} Pos={currentPos:F2}");
                wasVisible = currentVisible;
            }

            // Check for parent changes (CRITICAL)
            if (currentParent != lastParent)
            {
                string parentName = currentParent != null ? currentParent.name : "null";
                string lastParentName = lastParent != null ? lastParent.name : "null";
                LogCritical("PARENT_CHANGED", $"{lastParentName}→{parentName} Active={currentActive}");
                lastParent = currentParent;
            }

            // Check for significant position changes
            if (logPositionChanges && Vector3.Distance(currentPos, lastPosition) > positionThreshold)
            {
                float distance = mainCamera != null ? Vector3.Distance(currentPos, mainCamera.transform.position) : 0f;

                if (distance > disappearanceDistance)
                {
                    LogCritical("POSITION_FAR", $"Pos={lastPosition:F2}→{currentPos:F2} CamDist={distance:F1}");
                }
                else
                {
                    LogInfo("POSITION", $"Pos={lastPosition:F2}→{currentPos:F2} CamDist={distance:F1}");
                }

                lastPosition = currentPos;
            }

            // Check for scale changes (might indicate scaling to zero)
            if (Vector3.Distance(currentScale, lastScale) > scaleThreshold)
            {
                if (currentScale.magnitude < minVisibleScale)
                {
                    LogCritical("SCALE_TINY", $"Scale={lastScale:F3}→{currentScale:F3} (INVISIBLE!)");
                }
                else
                {
                    LogInfo("SCALE", $"Scale={lastScale:F3}→{currentScale:F3}");
                }

                lastScale = currentScale;
            }

            // Check for renderer component issues
            CheckRendererIssues();
        }

        private void CheckRendererIssues()
        {
            if (renderers == null || renderers.Length == 0) return;

            int enabledCount = 0;
            int visibleCount = 0;
            int nullCount = 0;

            foreach (var renderer in renderers)
            {
                if (renderer == null)
                {
                    nullCount++;
                }
                else
                {
                    if (renderer.enabled) enabledCount++;
                    if (renderer.isVisible) visibleCount++;
                }
            }

            if (nullCount > 0)
            {
                LogCritical("RENDERER_NULL", $"{nullCount}/{renderers.Length} renderers are null!");
            }

            if (enabledCount == 0 && renderers.Length > 0)
            {
                LogCritical("RENDERER_DISABLED", $"All {renderers.Length} renderers disabled");
            }
        }

        private bool IsObjectVisible()
        {
            if (renderers == null || renderers.Length == 0) return false;

            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.enabled && renderer.isVisible)
                {
                    return true;
                }
            }

            return false;
        }

        private void LogCritical(string event_type, string details)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.LogError($"[{objectName}] {timestamp} | CRITICAL_{event_type} | {details}");
        }

        private void LogInfo(string event_type, string details)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.Log($"[{objectName}] {timestamp} | {event_type} | {details}");
        }

        // Unity lifecycle events
        private void OnEnable()
        {
            if (enableDebug)
            {
                LogCritical("ENABLED", $"Object enabled - Active={gameObject.activeInHierarchy}");
            }
        }

        private void OnDisable()
        {
            if (enableDebug)
            {
                LogCritical("DISABLED", $"Object disabled - ActiveInHierarchy={gameObject.activeInHierarchy}");
            }
        }

        private void OnDestroy()
        {
            if (enableDebug)
            {
                LogCritical("DESTROYED", $"Object destroyed at Pos={transform.position:F2}");
            }
        }

        // Public methods
        public void ForceLog(string reason = "MANUAL")
        {
            if (!enableDebug) return;

            bool isActive = gameObject.activeInHierarchy;
            bool isVisible = IsObjectVisible();
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;
            float camDist = mainCamera != null ? Vector3.Distance(pos, mainCamera.transform.position) : 0f;

            LogCritical(reason,
                $"Active={isActive} Visible={isVisible} Pos={pos:F2} Scale={scale:F3} CamDist={camDist:F1}");
        }

        public void SetDebugEnabled(bool enabled)
        {
            enableDebug = enabled;
        }

        // Method to call when you suspect the object has vanished
        public void CheckIfVanished()
        {
            if (!enableDebug) return;

            bool active = gameObject.activeInHierarchy;
            bool visible = IsObjectVisible();
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;
            float camDist = mainCamera != null ? Vector3.Distance(pos, mainCamera.transform.position) : 0f;

            if (!active)
            {
                LogCritical("VANISH_CHECK",
                    $"INACTIVE! Self={gameObject.activeSelf} Parent={transform.parent?.name ?? "null"}");
            }
            else if (!visible)
            {
                LogCritical("VANISH_CHECK", $"NOT_VISIBLE! Pos={pos:F2} Scale={scale:F3} CamDist={camDist:F1}");
            }
            else if (camDist > disappearanceDistance)
            {
                LogCritical("VANISH_CHECK", $"TOO_FAR! CamDist={camDist:F1} Pos={pos:F2}");
            }
            else if (scale.magnitude < minVisibleScale)
            {
                LogCritical("VANISH_CHECK", $"TOO_SMALL! Scale={scale:F3}");
            }
            else
            {
                LogInfo("VANISH_CHECK",
                    $"SEEMS_OK - Active={active} Visible={visible} Pos={pos:F2} CamDist={camDist:F1}");
            }
        }
    }
}