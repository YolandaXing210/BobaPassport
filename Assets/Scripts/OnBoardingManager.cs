using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class OnboardingManager : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public CanvasGroup onboardingCanvasGroup; // For fade effect
    public GameObject onboardingPanel; // The entire onboarding UI
    
    [Header("Transition Settings")]
    public float fadeOutDuration = 1f;
    public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public bool useScaleEffect = true;
    public float scaleEffectMultiplier = 0.95f;
    
    [Header("Target Game")]
    public GameManager triviaGameManager; // Reference to your trivia game manager
    public GameObject scoreGameUI; 
    public GameObject powerIndicatorUI; 
    
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip transitionSound;
    
    
    // Private variables
    private Vector3 originalScale;
    private bool isTransitioning = false;
    
    private void Start()
    {
        SetupOnboarding();
    }
    
    void SetupOnboarding()
    {
        // Setup button listener
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        
        // Store original scale
        if (onboardingPanel != null)
        {
            originalScale = onboardingPanel.transform.localScale;
        }
        
        // Ensure canvas group is set up
        if (onboardingCanvasGroup == null && onboardingPanel != null)
        {
            onboardingCanvasGroup = onboardingPanel.GetComponent<CanvasGroup>();
            if (onboardingCanvasGroup == null)
            {
                onboardingCanvasGroup = onboardingPanel.AddComponent<CanvasGroup>();
            }
        }
        
        // Initialize canvas group
        if (onboardingCanvasGroup != null)
        {
            onboardingCanvasGroup.alpha = 1f;
            onboardingCanvasGroup.interactable = true;
            onboardingCanvasGroup.blocksRaycasts = true;
        }
        
        // Hide trivia game UI initially
        if (scoreGameUI != null)
        {
            scoreGameUI.SetActive(false);
        } 
        if (powerIndicatorUI != null)
        {
            powerIndicatorUI.SetActive(false);
        }
        
        // Play onboarding audio if available
        PlaySound(null); // You can add an onboarding intro sound here
    }
    
    public void OnPlayButtonClicked()
    {
        if (isTransitioning) return;
        
        // Play button click sound
        PlaySound(buttonClickSound);
        
        
        // Start transition
        StartCoroutine(TransitionToGame());
    }
    
    IEnumerator TransitionToGame()
    {
        isTransitioning = true;
        
        // Disable button interactions
        if (onboardingCanvasGroup != null)
        {
            onboardingCanvasGroup.interactable = false;
        }
        
        // Play transition sound
        PlaySound(transitionSound);
        
        
        // Small delay for button feedback
        yield return new WaitForSeconds(0.1f);
        
        // Fade out effect
        yield return StartCoroutine(FadeOutOnboarding());
        
        // Switch to trivia game
        SwitchToTriviaGame();
        
        isTransitioning = false;
    }
    
    IEnumerator FadeOutOnboarding()
    {
        float elapsedTime = 0f;
        Vector3 startScale = originalScale;
        Vector3 targetScale = useScaleEffect ? originalScale * scaleEffectMultiplier : originalScale;
        
        while (elapsedTime < fadeOutDuration)
        {
            float t = elapsedTime / fadeOutDuration;
            float curveValue = fadeOutCurve.Evaluate(t);
            
            // Fade alpha
            if (onboardingCanvasGroup != null)
            {
                onboardingCanvasGroup.alpha = curveValue;
            }
            
            // Scale effect
            if (useScaleEffect && onboardingPanel != null)
            {
                onboardingPanel.transform.localScale = Vector3.Lerp(startScale, targetScale, 1f - curveValue);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final values
        if (onboardingCanvasGroup != null)
        {
            onboardingCanvasGroup.alpha = 0f;
            onboardingCanvasGroup.blocksRaycasts = false;
        }
        
        if (useScaleEffect && onboardingPanel != null)
        {
            onboardingPanel.transform.localScale = targetScale;
        }
    }
    
    void SwitchToTriviaGame()
    {
       
        
        // If using same scene transition
        // Hide onboarding
        if (onboardingPanel != null)
        {
            onboardingPanel.SetActive(false);
        }
        
        // Show trivia game UI
        if (scoreGameUI != null)
        {
            scoreGameUI.SetActive(true);
        } 
        
        // Show power game UI
        if (powerIndicatorUI != null)
        {
            powerIndicatorUI.SetActive(true);
        }
        
        
        // Start trivia game
        if (triviaGameManager != null)
        {
            // If you want to automatically show mode selection
            // triviaGameManager.ShowModeSelection(); // You'd need to add this method
            
            // Or just enable the game manager
            triviaGameManager.enabled = true;
        }
        
        Debug.Log("Transitioned to Trivia Game!");
    }
    
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Public methods for additional control
    public void SetFadeDuration(float duration)
    {
        fadeOutDuration = duration;
    }
    
    public void EnableScaleEffect(bool enable)
    {
        useScaleEffect = enable;
    }
    
    // Alternative method to call from UI button directly
    public void StartGameTransition()
    {
        OnPlayButtonClicked();
    }
    
    // Method to restart onboarding (for testing)
    [ContextMenu("Reset Onboarding")]
    public void ResetOnboarding()
    {
        if (onboardingPanel != null)
        {
            onboardingPanel.SetActive(true);
            onboardingPanel.transform.localScale = originalScale;
        }
        
        if (onboardingCanvasGroup != null)
        {
            onboardingCanvasGroup.alpha = 1f;
            onboardingCanvasGroup.interactable = true;
            onboardingCanvasGroup.blocksRaycasts = true;
        }
        
        if (powerIndicatorUI != null)
        {
            powerIndicatorUI.SetActive(false);
        } 
        
        if (scoreGameUI != null)
        {
            scoreGameUI.SetActive(false);
        }
        
        if (triviaGameManager != null)
        {
            triviaGameManager.enabled = false;
        }
        
        isTransitioning = false;
    }
}