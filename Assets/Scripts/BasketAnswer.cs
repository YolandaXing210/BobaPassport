using UnityEngine;
using System.Collections;
using TMPro;

public class BasketAnswer : MonoBehaviour
{
    [Header("Answer Settings")]
    public int basketIndex; // 0 to 3 (A, B, C, D) 
    
    [Header("Answer Display")]
    
    // Private variables
    private GameManager gameManager;
    
    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
           
           
            
            // Notify game manager
            gameManager.BasketScored(this);
        }
    }

    
}