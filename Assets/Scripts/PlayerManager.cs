using System.Collections.Generic;
using UnityEngine;


public static class PlayerManager
{
    private const string PLAYERS_KEY    = "Players";
    private const string SCORE_PREFIX   = "Score_";
    private const string CURRENT_PLAYER = "CurrentPlayer";

    

    public static void SetCurrentPlayer(string username)
    {
        PlayerPrefs.SetString(CURRENT_PLAYER, username);
        PlayerPrefs.Save();
    }

    public static string GetCurrentPlayer()
    {
        return PlayerPrefs.GetString(CURRENT_PLAYER, "");
    }

   

    public static int GetScore(string username)
    {
        return PlayerPrefs.GetInt(SCORE_PREFIX + username, 0);
    }

    public static void TrySaveScore(string username, int score)
    {
        int current = GetScore(username);
        if (score > current)
        {
            PlayerPrefs.SetInt(SCORE_PREFIX + username, score);
            PlayerPrefs.Save();
        }
    }


    public static List<string> GetAllPlayers()
    {
        string raw = PlayerPrefs.GetString(PLAYERS_KEY, "");
        if (string.IsNullOrEmpty(raw)) return new List<string>();
        return new List<string>(raw.Split(','));
    }

    public static void AddPlayer(string username)
    {
        List<string> players = GetAllPlayers();
        if (!players.Contains(username))
        {
            players.Add(username);
            PlayerPrefs.SetString(PLAYERS_KEY, string.Join(",", players));
            PlayerPrefs.Save();
        }
    }



    public static List<(string name, int score)> GetLeaderboard()
    {
        List<string> players = GetAllPlayers();
        List<(string, int)> leaderboard = new List<(string, int)>();

        foreach (string p in players)
            leaderboard.Add((p, GetScore(p)));

       
        leaderboard.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        return leaderboard;
    }
}
