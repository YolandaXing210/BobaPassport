using UnityEngine;

namespace BobaShooter
{
    /// <summary>
    /// Simple script to destroy boba seeds after a certain time to prevent memory issues
    /// </summary>
    public class BobaSeedDestroyer : MonoBehaviour
    {
        [Header("Destruction Settings")]
        [SerializeField] private float destroyTime = 5f; // Time in seconds before destroying
        [SerializeField] private bool destroyOnCollision = false; // Destroy when hitting something
        [SerializeField] private LayerMask destroyOnLayers = -1; // Which layers trigger destruction
        
        private void Start()
        {
            // Schedule destruction
            Destroy(gameObject, destroyTime);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!destroyOnCollision) return;
            
            // Check if we hit a layer we should destroy on
            if (((1 << collision.gameObject.layer) & destroyOnLayers) != 0)
            {
                Destroy(gameObject);
            }
        }
    }
}