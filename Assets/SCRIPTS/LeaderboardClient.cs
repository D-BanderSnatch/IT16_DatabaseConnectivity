using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardClient : MonoBehaviour
{
    const string BaseUrl = "http://localhost/game";

    [Serializable] public class ScoreEntry { public string name; public float time; }
    [Serializable] class ScoreList { public ScoreEntry[] scores; }

    public void SubmitScore(string playerName, float time)
    {
        StartCoroutine(Submit(playerName, time));
    }

    IEnumerator Submit(string playerName, float time)
    {
        var form = new WWWForm();
        form.AddField("name", playerName);
        form.AddField("time", time.ToString(CultureInfo.InvariantCulture));

        using var req = UnityWebRequest.Post(BaseUrl + "/submit_score.php", form);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError("Submit failed: " + req.error);
        else
            Debug.Log("Score submitted: " + req.downloadHandler.text);
    }

    public void LoadScores(Action<ScoreEntry[]> onDone)
    {
        StartCoroutine(Load(onDone));
    }

    IEnumerator Load(Action<ScoreEntry[]> onDone)
    {
        using var req = UnityWebRequest.Get(BaseUrl + "/get_scores.php");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Load failed: " + req.error);
            yield break;
        }

        var list = JsonUtility.FromJson<ScoreList>(req.downloadHandler.text);
        onDone?.Invoke(list.scores);
    }
}