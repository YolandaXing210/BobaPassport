using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BobaShooter
{
    public class PowerDisplayUI : MonoBehaviour
    {
        
        [Header("Fill Bar Settings")]
        [SerializeField] private UnityEngine.UI.Image fillImage;
        [SerializeField] private bool invertFill = false; // Set true if you want fill to decrease as power increases
        
        [Header("Visual Settings")]
        [SerializeField] private bool animateScale = true;
      
        
        private BobaShootingController shootingController;
        private Vector3 originalScale;
        
        private void Start()
        {
            // Find the shooting controller
            shootingController = FindFirstObjectByType<BobaShootingController>();
            
            if (shootingController == null)
            {
                Debug.LogError("BobaShootingController not found!");
                return;
            }
            
            
            // Validate fill image setup
            if (fillImage != null && fillImage.type != UnityEngine.UI.Image.Type.Filled)
            {
                Debug.LogWarning("Fill Image is not set to 'Filled' type. Please change Image Type to 'Filled' in the inspector.");
            }
            
            // Subscribe to power change events
            shootingController.OnPowerChanged.AddListener(UpdatePowerDisplay);
            shootingController.OnShoot.AddListener(OnShoot);
            
            // Initialize display
            UpdatePowerDisplay(0);
            
            // Initialize fill amount
            if (fillImage != null)
            {
                fillImage.fillAmount = invertFill ? 1f : 0f;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (shootingController != null)
            {
                shootingController.OnPowerChanged.RemoveListener(UpdatePowerDisplay);
                shootingController.OnShoot.RemoveListener(OnShoot);
            }
        }
        
        private void UpdatePowerDisplay(float power)
        {
            // Get normalized power (0-1)
            float normalizedPower = shootingController.GetPowerPercentage();
            
            // Update fill image
            if (fillImage != null)
            {
                fillImage.fillAmount = invertFill ? (1f - normalizedPower) : normalizedPower;
            }
            
        }
        
        private void OnShoot(float finalPower)
        {
            // Optional: Add a quick animation or effect when shooting
            if (animateScale)
            {
                // You could add a coroutine here for a "pop" effect
                StartCoroutine(ShootAnimation());
            }
        }
        
        private System.Collections.IEnumerator ShootAnimation()
        {
            // Reset after a delay
            yield return new WaitForSeconds(0.5f);
           
            // Reset fill amount
            if (fillImage != null)
            {
                fillImage.fillAmount = invertFill ? 1f : 0f;
            }
        }
    }
}