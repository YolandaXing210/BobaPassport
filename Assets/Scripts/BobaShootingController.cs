using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace BobaShooter
{
    public enum TouchAreaMode
    {
        FullScreen,          // Touch anywhere
        BottomHalf,         // Bottom 50% of screen
        MiddleRectangle,    // Custom rectangle (default)
        MidToBottom,        // Middle to bottom of screen
        TopHalf            // Top 50% of screen
    }
    
    public class BobaShootingController : MonoBehaviour
    {
        [Header("Shooting Settings")]
        [SerializeField] private Transform shootPoint; // Where the boba spawns from
        [SerializeField] private GameObject strawTransform; // The straw object to animate
        [SerializeField] private GameObject bobaPrefab; // The boba seed prefab
        [SerializeField] private float minPower = 5f; // Minimum shooting power
        [SerializeField] private float maxPower = 30f; // Maximum shooting power
        [SerializeField] private float powerMultiplier = 0.05f; // How swipe distance converts to power
        
        [Header("Swipe Detection Settings")]
        [SerializeField] private float minSwipeDistance = 50f; // Minimum swipe distance in pixels
        [SerializeField] private float maxSwipeTime = 1f; // Maximum time for a valid swipe
        
        [Header("Touch Area Settings")]
        [SerializeField] private TouchAreaMode touchAreaMode = TouchAreaMode.MiddleRectangle;
        [SerializeField] private float touchAreaXMin = 0.3f; // Left boundary (0-1)
        [SerializeField] private float touchAreaXMax = 0.7f; // Right boundary (0-1)
        [SerializeField] private float touchAreaYMin = 0.3f; // Bottom boundary (0-1)
        [SerializeField] private float touchAreaYMax = 0.7f; // Top boundary (0-1)
        
        [Header("Shooting Arc Settings")]
        [SerializeField] private float shootAngle = 45f; // Angle for projectile arc
        [SerializeField] private bool useGravity = true; // Whether to use physics
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shootSound;
        
        [Header("Animation")]
        [SerializeField] private bool animateStraw = true;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private float recoilDistance = 0.1f;
        [SerializeField] private float scaleAmount = 0.1f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Events
        public UnityEvent<float> OnPowerChanged; // Called when power changes during swipe
        public UnityEvent<float> OnShoot; // Called when shooting with final power
        
        // Private variables
        private Vector2 swipeStartPos;
        private float swipeStartTime;
        private bool isSwipingDown = false;
        private float currentPower = 0f;
        private Camera mainCamera;
        
        // For mouse support in editor
        private bool isMouseDown = false;
        
        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main camera not found! Make sure your camera is tagged as 'MainCamera'");
            }
            
            // Validate references
            if (shootPoint == null)
            {
                shootPoint = transform;
                Debug.LogWarning("Shoot point not set, using transform position");
            }
            
            if (bobaPrefab == null)
            {
                Debug.LogError("Boba prefab not assigned!");
            }
            
            if (strawTransform.transform == null && animateStraw)
            {
                Debug.LogWarning("Straw Transform not assigned! Animation will be disabled.");
                animateStraw = false;
            }
        }
        
        private void Update()
        {
            // Handle touch input
            if (Input.touchCount > 0)
            {
                HandleTouch();
            }
            // Handle mouse input (for editor testing)
            else if (Application.isEditor || !Application.isMobilePlatform)
            {
                HandleMouse();
            }
        }
        
        private void HandleTouch()
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartSwipe(touch.position);
                    break;
                    
                case TouchPhase.Moved:
                    if (isSwipingDown)
                    {
                        UpdateSwipe(touch.position);
                    }
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isSwipingDown)
                    {
                        EndSwipe(touch.position);
                    }
                    break;
            }
        }
        
        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isMouseDown = true;
                StartSwipe(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && isMouseDown)
            {
                if (isSwipingDown)
                {
                    UpdateSwipe(Input.mousePosition);
                }
            }
            else if (Input.GetMouseButtonUp(0) && isMouseDown)
            {
                isMouseDown = false;
                if (isSwipingDown)
                {
                    EndSwipe(Input.mousePosition);
                }
            }
        }
        
        private void StartSwipe(Vector2 position)
        {
            // Check if swipe started in the valid touch area
            bool inValidArea = IsInTouchArea(position);
            
            if (inValidArea)
            {
                swipeStartPos = position;
                swipeStartTime = Time.time;
                isSwipingDown = true;
                currentPower = 0f;
                
                
                if (showDebugInfo)
                {
                    Debug.Log($"Swipe started at {position}");
                }
            }
        }
        
        private bool IsInTouchArea(Vector2 position)
        {
            float normalizedX = position.x / Screen.width;
            float normalizedY = position.y / Screen.height;
            
            switch (touchAreaMode)
            {
                case TouchAreaMode.FullScreen:
                    return true;
                    
                case TouchAreaMode.BottomHalf:
                    return normalizedY < 0.5f;
                    
                case TouchAreaMode.TopHalf:
                    return normalizedY > 0.5f;
                    
                case TouchAreaMode.MidToBottom:
                    return normalizedY < 0.7f;
                    
                case TouchAreaMode.MiddleRectangle:
                default:
                    return normalizedX >= touchAreaXMin && normalizedX <= touchAreaXMax &&
                           normalizedY >= touchAreaYMin && normalizedY <= touchAreaYMax;
            }
        }
        
        private void UpdateSwipe(Vector2 position)
        {
            // Calculate swipe distance
            float swipeDistanceY = swipeStartPos.y - position.y;
            
            // Only update power if we've swiped down at least a little
            // Don't cancel immediately if finger moves slightly up
            if (swipeDistanceY > 10) // Small threshold to prevent jitter
            {
                // Calculate power based on swipe distance
                float swipeDistance = swipeDistanceY;
                currentPower = Mathf.Clamp(swipeDistance * powerMultiplier, minPower, maxPower);
                
                
                OnPowerChanged?.Invoke(currentPower);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Current power: {currentPower:F2}");
                }
            }
            // Don't cancel unless we've moved significantly upward
            else if (swipeDistanceY < -50) // Give some tolerance
            {
                // Moved too far up, cancel
                CancelSwipe();
            }
        }
        
        private void EndSwipe(Vector2 position)
        {
            if (!isSwipingDown) return;
            
            float swipeTime = Time.time - swipeStartTime;
            float swipeDistanceY = swipeStartPos.y - position.y;
            
            // Validate swipe
            if (swipeTime <= maxSwipeTime && swipeDistanceY >= minSwipeDistance)
            {
                // Valid swipe - shoot!
                Shoot(currentPower);
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Invalid swipe - Time: {swipeTime:F2}s (max: {maxSwipeTime}), Distance: {swipeDistanceY:F2}px (min: {minSwipeDistance})");
                }
                
                // If we have some power built up, shoot anyway (more forgiving)
                if (currentPower > minPower)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"Shooting with reduced power: {currentPower:F2}");
                    }
                    Shoot(currentPower);
                }
            }
            
            CancelSwipe();
        }
        
        private void CancelSwipe()
        {
            isSwipingDown = false;
            currentPower = 0f;
            
        }
        
        private void Shoot(float power)
        {
            if (bobaPrefab == null)
            {
                Debug.LogError("Cannot shoot - boba prefab not assigned!");
                return;
            }
            
            // Spawn boba
            GameObject boba = Instantiate(bobaPrefab, shootPoint.position, shootPoint.rotation);
            
            // Calculate shooting direction
            Vector3 shootDirection = CalculateShootDirection();
            
            // Apply force
            Rigidbody rb = boba.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = useGravity;
                rb.linearVelocity = shootDirection * power;
            }
            else
            {
                // If no rigidbody, try to use a custom projectile script
                Debug.LogWarning("Boba prefab doesn't have a Rigidbody!");
            }
            
            OnShoot?.Invoke(power);
            
            // Play shoot sound
            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
            
            // Animate straw
            if (animateStraw)
            {
                StartCoroutine(AnimateStraw());
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Shot boba with power: {power:F2}");
            }
        }
        
        private Vector3 CalculateShootDirection()
        {
            // Calculate direction based on shoot angle
            float angleInRadians = shootAngle * Mathf.Deg2Rad;
            
            // Get forward direction of the barrel
            Vector3 forward = shootPoint.forward;
            Vector3 up = shootPoint.up;
            
            // Create direction vector with angle
            Vector3 direction = forward * Mathf.Cos(angleInRadians) + up * Mathf.Sin(angleInRadians);
            
            return direction.normalized;
        }
        
        // Public methods for external control
        public void SetShootAngle(float angle)
        {
            shootAngle = Mathf.Clamp(angle, 0f, 90f);
        }
        
        public float GetCurrentPower()
        {
            return currentPower;
        }
        
        public float GetPowerPercentage()
        {
            if (maxPower <= minPower) return 0f;
            return (currentPower - minPower) / (maxPower - minPower);
        }
        
        // Get touch area bounds for visualization
        public void GetTouchAreaBounds(out float xMin, out float xMax, out float yMin, out float yMax)
        {
            switch (touchAreaMode)
            {
                case TouchAreaMode.FullScreen:
                    xMin = 0f; xMax = 1f; yMin = 0f; yMax = 1f;
                    break;
                case TouchAreaMode.BottomHalf:
                    xMin = 0f; xMax = 1f; yMin = 0f; yMax = 0.5f;
                    break;
                case TouchAreaMode.TopHalf:
                    xMin = 0f; xMax = 1f; yMin = 0.5f; yMax = 1f;
                    break;
                case TouchAreaMode.MidToBottom:
                    xMin = 0f; xMax = 1f; yMin = 0f; yMax = 0.7f;
                    break;
                case TouchAreaMode.MiddleRectangle:
                default:
                    xMin = touchAreaXMin; xMax = touchAreaXMax; 
                    yMin = touchAreaYMin; yMax = touchAreaYMax;
                    break;
            }
        }
        
        private IEnumerator AnimateStraw()
        {
            // Check if straw transform is assigned
            if (strawTransform.transform == null)
            {
                Debug.LogWarning("Straw Transform not assigned for animation!");
                yield break;
            }
            
            // Store original position and scale
            Vector3 originalPosition = strawTransform.transform.localPosition;
            Vector3 originalScale = strawTransform.transform.localScale;
            
            // Calculate target values
            Vector3 recoilPosition = originalPosition - strawTransform.transform.forward * recoilDistance;
            Vector3 expandedScale = originalScale * (1f + scaleAmount);
            
            float elapsedTime = 0f;
            
            // Animate to recoil position (first half of animation)
            while (elapsedTime < animationDuration * 0.5f)
            {
                float t = elapsedTime / (animationDuration * 0.5f);
                
                // Smooth animation using ease out
                t = 1f - Mathf.Pow(1f - t, 3f);
                
                strawTransform.transform.localPosition = Vector3.Lerp(originalPosition, recoilPosition, t);
                strawTransform.transform.localScale = Vector3.Lerp(originalScale, expandedScale, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Reset elapsed time for return animation
            elapsedTime = 0f;
            
            // Animate back to original position (second half)
            while (elapsedTime < animationDuration * 0.5f)
            {
                float t = elapsedTime / (animationDuration * 0.5f);
                
                // Smooth animation using ease in
                t = Mathf.Pow(t, 3f);
                
                strawTransform.transform.localPosition = Vector3.Lerp(recoilPosition, originalPosition, t);
                strawTransform.transform.localScale = Vector3.Lerp(expandedScale, originalScale, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure we're back to original values
            strawTransform.transform.localPosition = originalPosition;
            strawTransform.transform.localScale = originalScale;
        }
    }
}