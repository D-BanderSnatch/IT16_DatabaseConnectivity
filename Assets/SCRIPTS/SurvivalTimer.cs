using UnityEngine;
using TMPro;

public class SurvivalTimer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("Energy Manager")]
    public EnergyManager energyManager;

    [Header("Leaderboard")]
    public LeaderboardClient leaderboard;

    private float elapsedTime = 0f;
    private bool timerRunning = true;
    private bool scoreSubmitted = false;

    void Update()
    {
        if (energyManager == null || timerText == null)
        {
            return;
        }

        // Check if there is energy
        timerRunning = energyManager.currentEnergy > 0f;

        if (timerRunning)
        {
            // Only increase timer when energy is available
            elapsedTime += Time.deltaTime;
        }
        else if (!scoreSubmitted && elapsedTime > 0f)
        {
            // Energy just ran out: the run is over, submit once
            scoreSubmitted = true;

            if (leaderboard != null)
            {
                leaderboard.SubmitScore("Player", elapsedTime);
            }
            else
            {
                Debug.LogWarning("SurvivalTimer: Leaderboard is not assigned in the Inspector.");
            }
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        timerText.text =
            "TIME: " +
            minutes.ToString("00") +
            ":" +
            seconds.ToString("00");
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}