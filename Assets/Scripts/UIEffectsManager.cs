using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIEffectsManager : MonoBehaviour
{
    [Header("Screen Effects")]
    public Camera mainCamera;
    public float screenShakeIntensity = 0.1f;
    public float screenShakeDuration = 0.3f;
    
    [Header("Score Animation")]
    public Transform scoreTransform;
    public float scorePopScale = 1.3f;
    public float scorePopDuration = 0.4f;
    public Color scoreHighlightColor = Color.yellow;
    
    [Header("Feedback Animations")]
    public Transform correctPanel;
    public Transform wrongPanel;
    public float feedbackPopScale = 1.2f;
    public float feedbackDuration = 0.5f;
    
    [Header("Timer Effects")]
    public Image timerBar;
    public Color timerNormalColor = Color.green;
    public Color timerWarningColor = Color.yellow;
    public Color timerCriticalColor = Color.red;
    public bool pulseOnLowTime = true;
    
    [Header("Particle Effects")]
    public ParticleSystem correctParticles;
    public ParticleSystem wrongParticles;
    public ParticleSystem confettiParticles; // For game completion
    
    [Header("Background Effects")]
    public Image backgroundOverlay;
    public Color correctBgColor = new Color(0, 1, 0, 0.1f);
    public Color wrongBgColor = new Color(1, 0, 0, 0.1f);
    public float bgFlashDuration = 0.5f;
    
    // Private variables
    private Vector3 originalCameraPos;
    private Vector3 originalScoreScale;
    private Color originalScoreColor;
    private TextMeshProUGUI scoreText;
    
    // Singleton for easy access
    public static UIEffectsManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Get references
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (scoreTransform != null)
        {
            originalScoreScale = scoreTransform.localScale;
            scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
            if (scoreText != null)
                originalScoreColor = scoreText.color;
        }
        
        if (mainCamera != null)
            originalCameraPos = mainCamera.transform.localPosition;
    }
    
    // Score effects
    public void AnimateScoreIncrease()
    {
        if (scoreTransform != null)
        {
            StartCoroutine(ScorePopAnimation());
        }
    }
    
    IEnumerator ScorePopAnimation()
    {
        Vector3 targetScale = originalScoreScale * scorePopScale;
        float elapsedTime = 0f;
        
        // Change color
        if (scoreText != null)
            scoreText.color = scoreHighlightColor;
        
        // Scale up
        while (elapsedTime < scorePopDuration * 0.5f)
        {
            float t = elapsedTime / (scorePopDuration * 0.5f);
            scoreTransform.localScale = Vector3.Lerp(originalScoreScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // Scale back down
        while (elapsedTime < scorePopDuration * 0.5f)
        {
            float t = elapsedTime / (scorePopDuration * 0.5f);
            scoreTransform.localScale = Vector3.Lerp(targetScale, originalScoreScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Reset
        scoreTransform.localScale = originalScoreScale;
        if (scoreText != null)
            scoreText.color = originalScoreColor;
    }
    
    // Answer feedback effects
    public void ShowCorrectAnswerEffects()
    {
        // Screen shake
        StartCoroutine(ScreenShake(screenShakeIntensity * 0.5f, screenShakeDuration * 0.5f));
        
        // Background flash
        StartCoroutine(BackgroundFlash(correctBgColor));
        
        // Particles
        if (correctParticles != null)
            correctParticles.Play();
        
        // Panel animation
        if (correctPanel != null)
            StartCoroutine(PanelPopAnimation(correctPanel));
        
        // Score animation
        AnimateScoreIncrease();
    }
    
    public void ShowWrongAnswerEffects()
    {
        // Screen shake (stronger)
        StartCoroutine(ScreenShake(screenShakeIntensity, screenShakeDuration));
        
        // Background flash
        StartCoroutine(BackgroundFlash(wrongBgColor));
        
        // Particles
        if (wrongParticles != null)
            wrongParticles.Play();
        
        // Panel animation
        if (wrongPanel != null)
            StartCoroutine(PanelPopAnimation(wrongPanel));
    }
    
    // Screen shake effect
    IEnumerator ScreenShake(float intensity, float duration)
    {
        if (mainCamera == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            Vector3 randomOffset = Random.insideUnitSphere * intensity;
            randomOffset.z = 0; // Keep camera depth
            
            mainCamera.transform.localPosition = originalCameraPos + randomOffset;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.localPosition = originalCameraPos;
    }
    
    // Background flash effect
    IEnumerator BackgroundFlash(Color flashColor)
    {
        if (backgroundOverlay == null) yield break;
        
        Color originalColor = backgroundOverlay.color;
        float elapsedTime = 0f;
        
        // Fade in flash
        while (elapsedTime < bgFlashDuration * 0.3f)
        {
            float t = elapsedTime / (bgFlashDuration * 0.3f);
            backgroundOverlay.color = Color.Lerp(originalColor, flashColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // Fade out flash
        while (elapsedTime < bgFlashDuration * 0.7f)
        {
            float t = elapsedTime / (bgFlashDuration * 0.7f);
            backgroundOverlay.color = Color.Lerp(flashColor, originalColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        backgroundOverlay.color = originalColor;
    }
    
    // Panel pop animation
    IEnumerator PanelPopAnimation(Transform panel)
    {
        Vector3 originalScale = panel.localScale;
        Vector3 targetScale = originalScale * feedbackPopScale;
        float elapsedTime = 0f;
        
        // Pop in
        panel.localScale = Vector3.zero;
        
        while (elapsedTime < feedbackDuration * 0.6f)
        {
            float t = elapsedTime / (feedbackDuration * 0.6f);
            t = 1f - Mathf.Pow(1f - t, 3f); // Ease out
            
            panel.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // Settle to normal
        while (elapsedTime < feedbackDuration * 0.4f)
        {
            float t = elapsedTime / (feedbackDuration * 0.4f);
            panel.localScale = Vector3.Lerp(targetScale, originalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        panel.localScale = originalScale;
    }
    
    // Timer effects
    public void UpdateTimerEffects(float timeRemaining, float totalTime)
    {
        if (timerBar == null) return;
        
        float timePercent = timeRemaining / totalTime;
        
        // Color changes
        if (timePercent > 0.5f)
        {
            timerBar.color = timerNormalColor;
        }
        else if (timePercent > 0.2f)
        {
            timerBar.color = timerWarningColor;
        }
        else
        {
            timerBar.color = timerCriticalColor;
            
            // Pulse effect when critical
            if (pulseOnLowTime)
            {
                StartCoroutine(PulseTimer());
            }
        }
    }
    
    IEnumerator PulseTimer()
    {
        if (timerBar == null) yield break;
        
        Vector3 originalScale = timerBar.transform.localScale;
        Vector3 pulseScale = originalScale * 1.1f;
        
        // Pulse up
        float elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            float t = elapsedTime / 0.2f;
            timerBar.transform.localScale = Vector3.Lerp(originalScale, pulseScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Pulse down
        elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            float t = elapsedTime / 0.2f;
            timerBar.transform.localScale = Vector3.Lerp(pulseScale, originalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        timerBar.transform.localScale = originalScale;
    }
    
    // Game completion effects
    public void ShowGameCompletionEffects()
    {
        // Confetti particles
        if (confettiParticles != null)
            confettiParticles.Play();
        
        // Screen shake celebration
        StartCoroutine(ScreenShake(screenShakeIntensity * 0.3f, 1f));
    }
    
    // Utility methods
    public void PlayHitEffect(Vector3 position)
    {
        // Could spawn a hit effect at the given position
        StartCoroutine(ScreenShake(screenShakeIntensity * 0.2f, 0.1f));
    }
}