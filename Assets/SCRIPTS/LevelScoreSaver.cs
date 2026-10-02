using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelScoreSaver : MonoBehaviour
{
    [SerializeField] private GameProgress progress;
    [SerializeField] private Button saveButton;
    [SerializeField] private TMP_Text statusText; // optional

    private void Start()
    {
        saveButton.onClick.AddListener(OnSaveClicked);
    }

    private void OnSaveClicked()
    {
        if (progress == null || !progress.IsLoaded ||
            AuthManager.Instance == null || AuthManager.Instance.CurrentUser == null ||
            DatabaseManager.Instance == null)
        {
            SetStatus("Not ready yet. Start from the LOGIN scene.");
            return;
        }

        string uid = AuthManager.Instance.CurrentUser.UserId;
        int level = progress.CurrentLevel;
        int score = progress.CurrentScore;

        saveButton.interactable = false;
        SetStatus("Saving...");

        DatabaseManager.Instance.SaveLevelScore(uid, level, score, success =>
        {
            SetStatus(success
                ? "Saved: Level " + level + " - " + score
                : "Couldn't save. Check your connection.");
            saveButton.interactable = true;
        });
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }
}