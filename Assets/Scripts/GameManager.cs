using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Score")]
    public int score = 0;
    public TextMeshProUGUI scoreTMP;

    public TriviaObjects[] questions;
    private int currentQuestionIndex = 0;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts; // UI Texts for baskets, match index to basket
    public BasketAnswer[] baskets;

    public GameObject correctUI;
    public GameObject wrongUI;
    public TextMeshProUGUI wrongUIText;
    public GameObject endTMP;

    public float delayBeforeNextQuestion = 2f;

    void Start()
    {
        LoadQuestion();
    }

    void LoadQuestion()
    {
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
            }
        }
    }

    public void BasketScored(BasketAnswer basket)
    {
        TriviaObjects q = questions[currentQuestionIndex];
        int index = basket.basketIndex;

        if (index < q.isCorrect.Length && q.isCorrect[index])
        {
            Debug.Log("Correct!");
            correctUI.SetActive(true);
            questionText.text = null;
            score += 1;
            UpdateScoreUI();
            for (int i = 0; i < baskets.Length; i++)
            {
                
             answerTexts[i].gameObject.SetActive(false);
             
            }

        }
        else
        {
            Debug.Log("Wrong!");
            ShowCorrectAnswers(q);
            wrongUI.SetActive(true);
            questionText.text = null;
            for (int i = 0; i < baskets.Length; i++)
            {

                answerTexts[i].gameObject.SetActive(false);

            }
        }

        StartCoroutine(NextQuestionDelay());
    }

    IEnumerator NextQuestionDelay()
    {
        yield return new WaitForSeconds(delayBeforeNextQuestion);
        correctUI.SetActive(false);
        wrongUI.SetActive(false);

        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Length)
        {
            LoadQuestion();
        }
        else
        {
            Debug.Log("All questions done!");
            endTMP.SetActive(true);
        }
    }

    void ShowCorrectAnswers(TriviaObjects q)
    {
        string correctMsg = "YOU ARE WRONG. " + "\n" + "Correct answer";
        int correctCount = 0;

        for (int i = 0; i < q.isCorrect.Length; i++)
        {
            if (q.isCorrect[i]) correctCount++;
        }

        correctMsg += (correctCount > 1) ? "s are: " : " is: ";

        for (int i = 0; i < q.answers.Length; i++)
        {
            if (q.isCorrect[i])
            {
                char label = (char)('A' + i); // turns 0->A, 1->B...
                correctMsg += $"{q.answers[i]}";
            }
        }

        correctMsg = correctMsg.TrimEnd(',', ' '); // clean up
        wrongUIText.text = correctMsg;
    }

    void UpdateScoreUI()
    {
        if (scoreTMP != null)
            scoreTMP.text = "Score: " + score;
    }
}
