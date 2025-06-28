using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Score & Questions")]
    public int score = 0;
    public TextMeshProUGUI scoreTMP;
    public TriviaObjects[] questions;
    private int currentQuestionIndex = 0;

    [Header("Question UI")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts; // UI Texts for baskets, match index to basket
    public BasketAnswer[] baskets;

    [Header("Feedback UI")]
    public GameObject correctUI;
    public GameObject wrongUI;
    public TextMeshProUGUI wrongUIText;
    public float delayBeforeNextQuestion = 2f;

    [Header("Timer Mode")]
    public bool enableTimerMode = false;
    public float gameTimeLimitInSec = 180f; // 3 minutes default
    public TextMeshProUGUI timerText;
    public Slider timerSlider;
    public GameObject timerUI;

    [Header("Results Screen")]
    public GameObject resultsPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI questionsAnsweredText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI timeUsedText;
    public Button retryButton;
    public Button closeButton;

    [Header("Mode Selection")]
    public GameObject modeSelectionPanel;
    public Button timerModeButton;
    public Button freeModeButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip resultSound;
    public AudioClip gameStartSound;

    // Private variables
    private float currentTime;
    private int questionsAnswered = 0;
    private int correctAnswers = 0;
    private bool gameStarted = false;
    private bool gameEnded = false;
    private float gameStartTime;

    void Start()
    {
        SetupUI();
        ShowModeSelection();
    }

    void Update()
    {
        if (enableTimerMode && gameStarted && !gameEnded)
        {
            UpdateTimer();
        }
    }

    void SetupUI()
    {
        // Setup buttons
        if (retryButton != null)
            retryButton.onClick.AddListener(RestartGame);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseResults);
        
        if (timerModeButton != null)
            timerModeButton.onClick.AddListener(() => StartGameMode(true));
        
        if (freeModeButton != null)
            freeModeButton.onClick.AddListener(() => StartGameMode(false));

        // Hide game UI initially
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (timerUI != null) timerUI.SetActive(false);
    }

    void ShowModeSelection()
    {
        if (modeSelectionPanel != null)
            modeSelectionPanel.SetActive(true);
    }

    public void StartGameMode(bool timerMode)
    {
        enableTimerMode = timerMode;
        
        // Hide mode selection
        if (modeSelectionPanel != null)
            modeSelectionPanel.SetActive(false);
        
        // Show/hide timer UI
        if (timerUI != null)
            timerUI.SetActive(enableTimerMode);
        
        // Initialize timer
        if (enableTimerMode)
        {
            currentTime = gameTimeLimitInSec;
            if (timerSlider != null)
            {
                timerSlider.maxValue = gameTimeLimitInSec;
                timerSlider.value = currentTime;
            }
        }

        // Audio feedback
        if (TriviaAudioManager.Instance != null)
            TriviaAudioManager.Instance.PlayGameStart();
        else
            PlaySound(gameStartSound);
        
        // Start game
        gameStarted = true;
        gameStartTime = Time.time;
        LoadQuestion();
    }

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        
        // Update UI
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Change color when time is low
            /*if (currentTime <= 30f)
                timerText.color = Color.red;
            else if (currentTime <= 60f)
                timerText.color = Color.yellow;
            else
                timerText.color = Color.white;*/
        }
        
        if (timerSlider != null)
            timerSlider.value = currentTime;
        
        // UI Effects for timer
        if (UIEffectsManager.Instance != null)
            UIEffectsManager.Instance.UpdateTimerEffects(currentTime, gameTimeLimitInSec);
        
        // Audio countdown for last 10 seconds
        if (TriviaAudioManager.Instance != null)
            TriviaAudioManager.Instance.StartCountdownAudio(currentTime);
        
        // Time's up!
        if (currentTime <= 0f)
        {
            EndGame();
        }
    }

    void LoadQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            EndGame();
            return;
        }

        TriviaObjects q = questions[currentQuestionIndex];
        questionText.text = q.questionText;

        for (int i = 0; i < baskets.Length; i++)
        {
            if (i < q.answers.Length)
            {
                answerTexts[i].gameObject.SetActive(true);
                answerTexts[i].transform.parent.gameObject.SetActive(true);
                answerTexts[i].text = q.answers[i];
            }
            else
            {
                answerTexts[i].gameObject.SetActive(false);
                answerTexts[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }

    public void BasketScored(BasketAnswer basket)
    {
        if (gameEnded) return;

        // Audio feedback
        if (TriviaAudioManager.Instance != null)
            TriviaAudioManager.Instance.PlayBallHit();
        else
            PlaySound(hitSound);

        TriviaObjects q = questions[currentQuestionIndex];
        int index = basket.basketIndex;

        questionsAnswered++;

        if (index < q.isCorrect.Length && q.isCorrect[index])
        {
            Debug.Log("Correct!");
            correctAnswers++;
            score += 1;
            UpdateScoreUI();
            
            // Audio and visual effects
            if (TriviaAudioManager.Instance != null)
                TriviaAudioManager.Instance.PlayCorrectAnswer();
            else
                PlaySound(correctSound);
            
            if (UIEffectsManager.Instance != null)
                UIEffectsManager.Instance.ShowCorrectAnswerEffects();
            
            correctUI.SetActive(true);
            questionText.text = null;
            HideAnswerTexts();
            
            // Show visual feedback on basket
        }
        else
        {
            Debug.Log("Wrong!");
            
            // Audio and visual effects
            if (TriviaAudioManager.Instance != null)
                TriviaAudioManager.Instance.PlayWrongAnswer();
            else
                PlaySound(wrongSound);
            
            if (UIEffectsManager.Instance != null)
                UIEffectsManager.Instance.ShowWrongAnswerEffects();
            
            ShowCorrectAnswers(q);
            wrongUI.SetActive(true);
            questionText.text = null;
            HideAnswerTexts();
            
            // Show visual feedback on basket
        }

        StartCoroutine(NextQuestionDelay());
    }

    void HideAnswerTexts()
    {
        for (int i = 0; i < baskets.Length; i++)
        {
            answerTexts[i].gameObject.SetActive(false);
        }
    }

    IEnumerator NextQuestionDelay()
    {
        yield return new WaitForSeconds(delayBeforeNextQuestion);
        correctUI.SetActive(false);
        wrongUI.SetActive(false);

        // Reset baskets for new question

        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Length && !gameEnded)
        {
            LoadQuestion();
        }
        else
        {
            EndGame();
        }
    }



    void ShowCorrectAnswers(TriviaObjects q)
    {
        string correctMsg = "WRONG! ";
        int correctCount = 0;

        for (int i = 0; i < q.isCorrect.Length; i++)
        {
            if (q.isCorrect[i]) correctCount++;
        }

        correctMsg += (correctCount > 1) ? "Correct answers are: " : "Correct answer is: ";

        bool first = true;
        for (int i = 0; i < q.answers.Length; i++)
        {
            if (q.isCorrect[i])
            {
                if (!first) correctMsg += ", ";
                char label = (char)('A' + i);
                correctMsg += $"{label}) {q.answers[i]}";
                first = false;
            }
        }

        wrongUIText.text = correctMsg;
    }

    void EndGame()
    {
        gameEnded = true;
        
        // Audio feedback
        if (TriviaAudioManager.Instance != null)
            TriviaAudioManager.Instance.PlayGameEnd();
        else
            PlaySound(resultSound);
        
        // Visual effects
        if (UIEffectsManager.Instance != null)
            UIEffectsManager.Instance.ShowGameCompletionEffects();
        
        // Calculate stats
        float gameTime = Time.time - gameStartTime;
        float accuracy = questionsAnswered > 0 ? (float)correctAnswers / questionsAnswered * 100f : 0f;
        
        // Show results
        ShowResults(gameTime, accuracy);
        
        // Reset baskets for new question
    }

    void ShowResults(float gameTime, float accuracy)
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            
            // Update result texts
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {score}";
            
            if (questionsAnsweredText != null)
                questionsAnsweredText.text = $"Questions Answered: {questionsAnswered}/{questions.Length}";
            
            if (accuracyText != null)
                accuracyText.text = $"Accuracy: {accuracy:F1}%";
            
            if (timeUsedText != null)
            {
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                timeUsedText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
        
        Debug.Log("Game completed!");
    }

    void UpdateScoreUI()
    {
        if (scoreTMP != null)
            scoreTMP.text = "Score: " + score;
    }

    // Button Methods
    public void RestartGame()
    {
        // Reset all variables
        score = 0;
        currentQuestionIndex = 0;
        questionsAnswered = 0;
        correctAnswers = 0;
        gameEnded = false;
        gameStarted = false;
        
        if (timerUI != null)
            timerUI.SetActive(false);
        
        // Reset UI
        UpdateScoreUI();
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (correctUI != null) correctUI.SetActive(false);
        if (wrongUI != null) wrongUI.SetActive(false);
        
        // Show mode selection again
        ShowModeSelection();
    }

    public void CloseResults()
    {
        if (resultsPanel != null) resultsPanel.SetActive(false);
        
        // Player can continue shooting in free mode
        gameEnded = true; // Keep game ended but allow free play
        
        Debug.Log("Free play mode - shoot away!");
    }

    // Audio helper
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Public methods for external control
    public bool IsGameActive()
    {
        return gameStarted && !gameEnded;
    }

    public void SetTimerMode(bool enabled)
    {
        enableTimerMode = enabled;
    }

    public float GetTimeRemaining()
    {
        return enableTimerMode ? currentTime : -1f;
    }

    public int GetCurrentQuestionIndex()
    {
        return currentQuestionIndex;
    }
}