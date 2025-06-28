using UnityEngine;

[CreateAssetMenu(fileName = "TriviaObjects", menuName = "Scriptable Objects/TriviaObjects")]
public class TriviaObjects : ScriptableObject
{
    [TextArea] public string questionText;
    public string[] answers; // Always match basket count (2 or 4)
    public bool[] isCorrect; // Same length as answers
}
