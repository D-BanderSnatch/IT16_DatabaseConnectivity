using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Button 1 = Level 1, Button 2 = Level 2, and so on")]
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private TMP_Text infoText;

    [Header("Player Info (optional)")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Settings")]
    [Tooltip("Scene to load for each button. Element 0 = first button, and so on.")]
    [SerializeField] private string[] levelScenes = { "Level1", "Level2", "Level3" };
    [SerializeField] private int startingScore = 1000;

    private string uid;

    private IEnumerator Start()
    {
        SetButtonsInteractable(false);

        if (AuthManager.Instance == null ||
            DatabaseManager.Instance == null ||
            AuthManager.Instance.CurrentUser == null)
        {
            SetInfo("Not logged in. Press Play from the LOGIN scene.");
            yield break;
        }

        uid = AuthManager.Instance.CurrentUser.UserId;

        SetInfo("Connecting...");
        while (!DatabaseManager.Instance.IsReady) yield return null;

        // Hook up each button to its level number
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int level = i + 1; // copy so each button keeps its own number
            levelButtons[i].onClick.AddListener(() => SelectLevel(level));
        }

        
        DatabaseManager.Instance.LoadPlayerData(uid, data =>
        {
            
            string name = (data != null && !string.IsNullOrEmpty(data.playerName))
                ? data.playerName
                : AuthManager.Instance.CurrentUser.DisplayName;
            int level = data != null ? data.level : 1;
            int score = data != null ? data.score : 0;

            if (playerNameText != null) playerNameText.text = "Player: " + name;
            if (levelText != null) levelText.text = "Current Level: " + level;
            if (scoreText != null) scoreText.text = "Current Score: " + score;

            SetInfo("Choose a level:");
            SetButtonsInteractable(true);
        });
    }

    private void SelectLevel(int level)
    {
        if (level < 1 || level > levelScenes.Length)
        {
            SetInfo("No scene assigned for level " + level);
            return;
        }

        SetButtonsInteractable(false);
        SetInfo("Saving level " + level + "...");

        // Wait for the save to finish BEFORE loading Gameplay,
        // otherwise Gameplay could load the old level.
        DatabaseManager.Instance.SetLevel(uid, level, startingScore, success =>
        {
            if (success)
            {
                SceneManager.LoadScene(levelScenes[level - 1]);
            }
            else
            {
                SetInfo("Couldn't save. Check your connection and try again.");
                SetButtonsInteractable(true);
            }
        });
    }

    private void SetButtonsInteractable(bool value)
    {
        foreach (var b in levelButtons)
            if (b != null) b.interactable = value;
    }

    private void SetInfo(string msg)
    {
        if (infoText != null) infoText.text = msg;
        Debug.Log(msg);
    }
}