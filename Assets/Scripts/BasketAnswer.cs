using UnityEngine;
using System.Collections;
using TMPro;

public class BasketAnswer : MonoBehaviour
{
    [Header("Answer Settings")]
    public int basketIndex; // 0 to 3 (A, B, C, D)
    
    [Header("Visual Feedback")]
    public GameObject hitEffect; // Particle system or visual effect
    public Material defaultMaterial;
    public Material correctMaterial;
    public Material wrongMaterial;
    public bool animateOnHit = true;
    public float animationDuration = 0.5f;
    public float bounceScale = 1.2f;
    
    [Header("Answer Display")]
    public TextMeshPro answerLabel; // Shows A, B, C, D
    
    // Private variables
    private GameManager gameManager;
    private Renderer basketRenderer;
    private Vector3 originalScale;
    private bool hasBeenHit = false;
    
    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        basketRenderer = GetComponent<Renderer>();
        originalScale = transform.localScale;
        
        // Set answer label
        if (answerLabel != null)
        {
            char letter = (char)('A' + basketIndex);
            answerLabel.text = letter.ToString();
        }
        
        // Set default material
        if (basketRenderer != null && defaultMaterial != null)
        {
            basketRenderer.material = defaultMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !hasBeenHit && gameManager.IsGameActive())
        {
            hasBeenHit = true;
            
            // Visual effects
            ShowHitEffect();
            
            if (animateOnHit)
            {
                StartCoroutine(AnimateHit());
            }
            
            // Notify game manager
            gameManager.BasketScored(this);
        }
    }

    private void ShowHitEffect()
    {
        // Show hit particle effect
        if (hitEffect != null)
        {
            hitEffect.SetActive(true);
            
            // Auto-hide after a delay
            StartCoroutine(HideEffectAfterDelay(2f));
        }
    }

    private IEnumerator HideEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hitEffect != null)
        {
            hitEffect.SetActive(false);
        }
    }

    private IEnumerator AnimateHit()
    {
        Vector3 targetScale = originalScale * bounceScale;
        float elapsedTime = 0f;
        
        // Bounce up
        while (elapsedTime < animationDuration * 0.5f)
        {
            float t = elapsedTime / (animationDuration * 0.5f);
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease out
            
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // Bounce back
        while (elapsedTime < animationDuration * 0.5f)
        {
            float t = elapsedTime / (animationDuration * 0.5f);
            t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f); // Ease in
            
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
    }

    // Called by GameManager to show feedback
    public void ShowCorrectFeedback()
    {
        if (basketRenderer != null && correctMaterial != null)
        {
            basketRenderer.material = correctMaterial;
        }
    }

    public void ShowWrongFeedback()
    {
        if (basketRenderer != null && wrongMaterial != null)
        {
            basketRenderer.material = wrongMaterial;
        }
    }

    public void ResetForNewQuestion()
    {
        hasBeenHit = false;
        
        // Reset visual state
        if (basketRenderer != null && defaultMaterial != null)
        {
            basketRenderer.material = defaultMaterial;
        }
        
        if (hitEffect != null)
        {
            hitEffect.SetActive(false);
        }
        
        transform.localScale = originalScale;
    }

    // Get answer letter for display
    public string GetAnswerLetter()
    {
        return ((char)('A' + basketIndex)).ToString();
    }
}