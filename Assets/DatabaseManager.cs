using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

[Serializable]
public class PlayerData
{
    public string playerName;
    public int score;
    public int level;
    public long registeredAt; 
    public string registeredAtText; 

    public DateTime RegisteredDate =>
        DateTimeOffset.FromUnixTimeMilliseconds(registeredAt).LocalDateTime;
}

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    
    [SerializeField] private string databaseUrl = "https://tlw-databasetest-default-rtdb.asia-southeast1.firebasedatabase.app";

    private DatabaseReference dbRef;

    public bool IsReady { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbRef = FirebaseDatabase.GetInstance(databaseUrl).RootReference;
                IsReady = true;
                Debug.Log("Firebase ready");
            }
            else
            {
                Debug.LogError("Firebase dependencies not available: " + task.Result);
            }
        });
    }

   
    public void CreatePlayer(string userId, string username)
    {
        if (!IsReady) { Debug.LogWarning("Firebase not ready yet"); return; }

        var data = new Dictionary<string, object>
        {
            { "playerName", username },
            { "score", 0 },
            { "level", 1 },
            { "registeredAt", ServerValue.Timestamp } 
        };

        dbRef.Child("players").Child(userId).SetValueAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted) Debug.LogError("Create player failed: " + task.Exception);
                else
                {
                    Debug.Log("Player created");
                    WriteReadableRegistrationTime(userId);
                }
            });
    }

    
    private void WriteReadableRegistrationTime(string userId)
    {
        DatabaseReference userRef = dbRef.Child("players").Child(userId);

        userRef.Child("registeredAt").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled || !task.Result.Exists) return;

            long ms = Convert.ToInt64(task.Result.Value);
            DateTimeOffset local = DateTimeOffset.FromUnixTimeMilliseconds(ms).ToLocalTime();

            string text = local.ToString("MMM dd, yyyy  hh:mm:ss tt", CultureInfo.InvariantCulture)
                          + " (UTC" + local.ToString("zzz", CultureInfo.InvariantCulture) + ")";

            userRef.Child("registeredAtText").SetValueAsync(text);
        });
    }

    
    public void SavePlayerData(string userId, PlayerData data)
    {
        if (!IsReady) { Debug.LogWarning("Firebase not ready yet"); return; }

        var updates = new Dictionary<string, object>
        {
            { "playerName", data.playerName },
            { "score", data.score },
            { "level", data.level }
        };

        dbRef.Child("players").Child(userId).UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted) Debug.LogError("Save failed: " + task.Exception);
                else Debug.Log("Save complete");
            });
    }

    public void SetLevel(string userId, int level, int startingScore, Action<bool> onDone)
    {
        if (!IsReady) { onDone?.Invoke(false); return; }

        var updates = new Dictionary<string, object>
        {
            { "level", level },
            { "score", startingScore }
        };

        dbRef.Child("players").Child(userId).UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Set level failed: " + task.Exception);
                    onDone?.Invoke(false);
                }
                else
                {
                    onDone?.Invoke(true);
                }
            });
    }

    
    public void SaveLevelScore(string userId, int level, int score, Action<bool> onDone)
    {
        if (!IsReady) { onDone?.Invoke(false); return; }

        var updates = new Dictionary<string, object>
        {
            { "levelScores/level_" + level, score }
        };

        dbRef.Child("players").Child(userId).UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Save level score failed: " + task.Exception);
                    onDone?.Invoke(false);
                }
                else
                {
                    onDone?.Invoke(true);
                }
            });
    }

    public void LoadPlayerData(string userId, Action<PlayerData> onLoaded)
    {
        if (!IsReady) { Debug.LogWarning("Firebase not ready yet"); return; }

        dbRef.Child("players").Child(userId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Load failed: " + task.Exception);
                    onLoaded?.Invoke(null);
                    return;
                }

                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    onLoaded?.Invoke(null);
                    return;
                }

                PlayerData data = JsonUtility.FromJson<PlayerData>(snapshot.GetRawJsonValue());
                onLoaded?.Invoke(data);
            });
    }
}