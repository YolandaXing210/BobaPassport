using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class FadeSettings
{
    public float duration = 1f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public bool fadeScale = false;
    public Vector3 targetScale = Vector3.one * 0.9f;
    public bool fadePosition = false;
    public Vector3 targetPosition = Vector3.zero;
}

public class FadeTransition : MonoBehaviour
{
    [Header("Fade Target")]
    public CanvasGroup targetCanvasGroup;
    public bool autoFindCanvasGroup = true;
    
    [Header("Fade Settings")]
    public FadeSettings fadeInSettings;
    public FadeSettings fadeOutSettings;
    
    [Header("Audio")]
    public AudioClip fadeInSound;
    public AudioClip fadeOutSound;
    
    // Private variables
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private AudioSource audioSource;
    
    // Events
    public System.Action OnFadeInComplete;
    public System.Action OnFadeOutComplete;
    
    private void Awake()
    {
        // Auto-find canvas group if not assigned
        if (autoFindCanvasGroup && targetCanvasGroup == null)
        {
            targetCanvasGroup = GetComponent<CanvasGroup>();
            if (targetCanvasGroup == null)
            {
                targetCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Store original values
        if (targetCanvasGroup != null)
        {
            originalScale = targetCanvasGroup.transform.localScale;
            originalPosition = targetCanvasGroup.transform.localPosition;
        }
        
        // Get audio source
        audioSource = GetComponent<AudioSource>();
    }
    
    public void FadeIn()
    {
        StartCoroutine(FadeCoroutine(true, fadeInSettings));
    }
    
    public void FadeOut()
    {
        StartCoroutine(FadeCoroutine(false, fadeOutSettings));
    }
    
    public void FadeIn(float duration)
    {
        FadeSettings settings = fadeInSettings;
        settings.duration = duration;
        StartCoroutine(FadeCoroutine(true, settings));
    }
    
    public void FadeOut(float duration)
    {
        FadeSettings settings = fadeOutSettings;
        settings.duration = duration;
        StartCoroutine(FadeCoroutine(false, settings));
    }
    
    IEnumerator FadeCoroutine(bool fadeIn, FadeSettings settings)
    {
        if (targetCanvasGroup == null) yield break;
        
        // Play sound
        AudioClip soundToPlay = fadeIn ? fadeInSound : fadeOutSound;
        if (audioSource != null && soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }
        
        // Setup initial values
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        
        Vector3 startScale = fadeIn ? 
            (settings.fadeScale ? settings.targetScale : originalScale) : 
            originalScale;
        Vector3 endScale = fadeIn ? 
            originalScale : 
            (settings.fadeScale ? settings.targetScale : originalScale);
        
        Vector3 startPosition = fadeIn ? 
            (settings.fadePosition ? originalPosition + settings.targetPosition : originalPosition) : 
            originalPosition;
        Vector3 endPosition = fadeIn ? 
            originalPosition : 
            (settings.fadePosition ? originalPosition + settings.targetPosition : originalPosition);
        
        // Set initial state
        targetCanvasGroup.alpha = startAlpha;
        if (settings.fadeScale)
            targetCanvasGroup.transform.localScale = startScale;
        if (settings.fadePosition)
            targetCanvasGroup.transform.localPosition = startPosition;
        
        // Enable interactions for fade in
        if (fadeIn)
        {
            targetCanvasGroup.interactable = true;
            targetCanvasGroup.blocksRaycasts = true;
        }
        
        // Animate
        float elapsedTime = 0f;
        
        while (elapsedTime < settings.duration)
        {
            float t = elapsedTime / settings.duration;
            float curveValue = settings.fadeCurve.Evaluate(t);
            
            // Alpha
            targetCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            
            // Scale
            if (settings.fadeScale)
            {
                targetCanvasGroup.transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
            }
            
            // Position
            if (settings.fadePosition)
            {
                targetCanvasGroup.transform.localPosition = Vector3.Lerp(startPosition, endPosition, curveValue);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Set final values
        targetCanvasGroup.alpha = endAlpha;
        if (settings.fadeScale)
            targetCanvasGroup.transform.localScale = endScale;
        if (settings.fadePosition)
            targetCanvasGroup.transform.localPosition = endPosition;
        
        // Disable interactions for fade out
        if (!fadeIn)
        {
            targetCanvasGroup.interactable = false;
            targetCanvasGroup.blocksRaycasts = false;
        }
        
        // Invoke completion events
        if (fadeIn)
            OnFadeInComplete?.Invoke();
        else
            OnFadeOutComplete?.Invoke();
    }
    
    // Instant methods (no animation)
    public void ShowInstant()
    {
        if (targetCanvasGroup != null)
        {
            targetCanvasGroup.alpha = 1f;
            targetCanvasGroup.interactable = true;
            targetCanvasGroup.blocksRaycasts = true;
            targetCanvasGroup.transform.localScale = originalScale;
            targetCanvasGroup.transform.localPosition = originalPosition;
        }
    }
    
    public void HideInstant()
    {
        if (targetCanvasGroup != null)
        {
            targetCanvasGroup.alpha = 0f;
            targetCanvasGroup.interactable = false;
            targetCanvasGroup.blocksRaycasts = false;
        }
    }
    
    // Utility methods
    public bool IsVisible()
    {
        return targetCanvasGroup != null && targetCanvasGroup.alpha > 0f;
    }
    
    public bool IsFullyVisible()
    {
        return targetCanvasGroup != null && targetCanvasGroup.alpha >= 1f;
    }
    
    public void SetAlpha(float alpha)
    {
        if (targetCanvasGroup != null)
        {
            targetCanvasGroup.alpha = alpha;
        }
    }
}