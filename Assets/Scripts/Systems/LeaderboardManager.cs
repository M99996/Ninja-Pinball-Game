using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LeaderboardEntry
{
    public string name;
    public float time;

    public LeaderboardEntry(string name, float time)
    {
        this.name = name;
        this.time = time;
    }
}

[Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public static class LeaderboardManager
{
    private const string PLAYER_PREFS_KEY = "LeaderboardData";
    private const int MAX_ENTRIES = 100;

    public static void AddEntry(string name, float time)
    {
        if (string.IsNullOrEmpty(name) || time <= 0f) return;

        var data = LoadData();
        data.entries.Add(new LeaderboardEntry(name, time));
        
        // Sort by time (ascending - lower is better)
        data.entries = data.entries.OrderBy(e => e.time).ToList();
        
        // Keep only top entries
        if (data.entries.Count > MAX_ENTRIES)
        {
            data.entries = data.entries.Take(MAX_ENTRIES).ToList();
        }

        SaveData(data);
    }

    public static List<LeaderboardEntry> GetTop(int count)
    {
        var data = LoadData();
        return data.entries.Take(count).ToList();
    }

    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    public static void ClearLeaderboard()
    {
        PlayerPrefs.DeleteKey(PLAYER_PREFS_KEY);
        PlayerPrefs.Save();
    }

    private static LeaderboardData LoadData()
    {
        string json = PlayerPrefs.GetString(PLAYER_PREFS_KEY, "");
        
        if (string.IsNullOrEmpty(json))
        {
            return new LeaderboardData();
        }

        try
        {
            return JsonUtility.FromJson<LeaderboardData>(json);
        }
        catch
        {
            return new LeaderboardData();
        }
    }

    private static void SaveData(LeaderboardData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PLAYER_PREFS_KEY, json);
        PlayerPrefs.Save();
    }
}

