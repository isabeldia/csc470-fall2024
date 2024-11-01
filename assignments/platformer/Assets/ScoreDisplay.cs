using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Text scoreText;
    private static int currentScore = 0;

    void Start()
    {
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    // Static methods to modify score from other scripts
    public static void AddPoints(int points)
    {
        currentScore += points;
        if (currentScore < 0)
            currentScore = 0;
            
        // Find and update all score displays in the scene
        ScoreDisplay[] displays = FindObjectsOfType<ScoreDisplay>();
        foreach (ScoreDisplay display in displays)
        {
            display.UpdateScoreDisplay();
        }
    }

    public static int GetCurrentScore()
    {
        return currentScore;
    }

    public static void ResetScore()
    {
        currentScore = 0;
        ScoreDisplay[] displays = FindObjectsOfType<ScoreDisplay>();
        foreach (ScoreDisplay display in displays)
        {
            display.UpdateScoreDisplay();
        }
    }
}