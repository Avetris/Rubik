using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System;

public class LeaderboardManager
{

    public enum LEADERBOARD
    {
        NONE,
        TWO,
        THREE,
        FOUR,
        FIVE
    }

    static LeaderboardManager _instance;

    private LeaderboardManager()
    {
        var config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        if(Application.isEditor || Debug.isDebugBuild)
            PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        login(null);
    }

    void login(System.Action<int, LEADERBOARD> callback, int score, LEADERBOARD leaderboard)
    {
        Social.localUser.Authenticate((bool success) => {
            if (success)
            {
                callback?.Invoke(score, leaderboard);
            }
        });
    }

    void login(System.Action callback)
    {
        Social.localUser.Authenticate((bool success) => {
            if (success)
            {
                callback?.Invoke();
            }
        });
    }

    public static LeaderboardManager instance()
    {
        if (_instance == null)
        {
            _instance = new LeaderboardManager();
        }
        return _instance;
    }

    public void showLeaderboard()
    {
        #if UNITY_ANDROID
            if (Social.localUser.authenticated)
            {
                Social.ShowLeaderboardUI();
            }
            else
            {
                login(showLeaderboard);
            }
        #endif
    }


    public void setPuntuation(int score, LEADERBOARD leaderboard)
    {
        int lastScore = PlayerPrefs.GetInt(leaderboard.ToString(), score);
        if (lastScore < score)
        {
            score = lastScore;
        }
        PlayerPrefs.SetInt(leaderboard.ToString(), score);

        #if UNITY_ANDROID
            string leaderboardString = "";
            switch (leaderboard)
            {
                case LEADERBOARD.TWO:
                    leaderboardString = GPGSIds.leaderboard_2x2;
                    break;
                case LEADERBOARD.THREE:
                    leaderboardString = GPGSIds.leaderboard_3x3;
                    break;
                case LEADERBOARD.FOUR:
                    leaderboardString = GPGSIds.leaderboard_4x4;
                    break;
                case LEADERBOARD.FIVE:
                    leaderboardString = GPGSIds.leaderboard_5x5;
                    break;
            }
            if (leaderboardString.Length > 0)
            {
                if (Social.localUser.authenticated)
                {
                    Social.ReportScore(
                        score, leaderboardString,
                        (bool success) =>
                        {
                            Debug.Log("(" + leaderboardString + ")Leaderboard update success: " + score);
                        });
                }
                else
                {
                    login(setPuntuation, score, leaderboard);
                }
            }
        #endif
    }
}
