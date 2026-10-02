using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class GameProgress : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Saving")]
    [Tooltip("Max seconds between saves while the score is changing")]
    [SerializeField] private float autoSaveInterval = 10f;

    // Other scripts can listen for this to know when saved data has arrived
    public event Action OnProgressLoaded;

    private PlayerData data;
    private string uid;
    private bool loaded;
    private bool dirty;
    private float timer;

    public bool IsLoaded => loaded;
    public int CurrentLevel => data != null ? data.level : 1;
    public int CurrentScore => data != null ? data.score : 0;

    private IEnumerator Start()
    {
        SetTexts("Level: -", "Energy: -");

        if (AuthManager.Instance == null ||
            DatabaseManager.Instance == null ||
            AuthManager.Instance.CurrentUser == null)
        {
            Debug.LogWarning("Not logged in. Press Play from the LOGIN or REGISTER scene.");
            yield break;
        }

        uid = AuthManager.Instance.CurrentUser.UserId;

        while (!DatabaseManager.Instance.IsReady) yield return null;

        DatabaseManager.Instance.LoadPlayerData(uid, loadedData =>
        {
            data = loadedData ?? new PlayerData
            {
                playerName = AuthManager.Instance.CurrentUser.DisplayName,
                score = 0,
                level = 1
            };
            loaded = true;
            Refresh();
            Debug.Log("Loaded: level " + data.level + ", score " + data.score);
            OnProgressLoaded?.Invoke();
        });
    }

    private void Update()
    {
        if (!loaded || !dirty) return;

        timer += Time.unscaledDeltaTime;
        if (timer >= autoSaveInterval) SaveNow();
    }

    // Call this whenever energy changes. It only updates memory and the UI;
    // the actual database write is throttled.
    public void SetScore(int newScore)
    {
        if (!loaded || newScore == data.score) return;
        data.score = newScore;
        dirty = true;
        Refresh();
    }

    // Call when the player finishes a stage.
    // startingScore = the energy the next stage begins with (e.g. 1000).
    public void CompleteLevel(int startingScore)
    {
        if (!loaded) return;
        data.level++;
        data.score = startingScore;
        Refresh();
        SaveNow(); // always save immediately on progression
    }

    public void SaveNow()
    {
        if (!loaded || DatabaseManager.Instance == null) return;
        DatabaseManager.Instance.SavePlayerData(uid, data);
        dirty = false;
        timer = 0f;
    }

    private void Refresh()
    {
        SetTexts("Level: " + data.level, "Energy: " + data.score);
    }

    private void SetTexts(string level, string score)
    {
        if (levelText != null) levelText.text = level;
        if (scoreText != null) scoreText.text = score;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && dirty) SaveNow();
    }

    private void OnDestroy()
    {
        if (dirty) SaveNow();
    }
}