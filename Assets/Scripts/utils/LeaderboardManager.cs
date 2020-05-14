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
    #if UNITY_ANDROID
        var config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        if(Application.isEditor || Debug.isDebugBuild)
            PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    #elif UNITY_IPHONE
            login(null);
    #endif
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
    #if UNITY_ANDROID || UNITY_IPHONE
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
        
    #if UNITY_ANDROID || UNITY_IPHONE
            string leaderboardString = "";

#if UNITY_ANDROID
        score = score * 1000;
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

#elif UNITY_IPHONE

        switch (leaderboard)
            {
                case LEADERBOARD.TWO:
                    leaderboardString = GPGSIds.iphone_leaderboard_2x2;
                    break;
                case LEADERBOARD.THREE:
                    leaderboardString = GPGSIds.iphone_leaderboard_3x3;
                    break;
                case LEADERBOARD.FOUR:
                    leaderboardString = GPGSIds.iphone_leaderboard_4x4;
                    break;
                case LEADERBOARD.FIVE:
                    leaderboardString = GPGSIds.iphone_leaderboard_5x5;
                    break;
            }

#endif

        Debug.Log("Leaderboard to insert: " + score);
        int lastScore = PlayerPrefs.GetInt(leaderboard.ToString(), score);
        Debug.Log("Last Leaderboard to insert: " + lastScore);
        if (lastScore > score)
        {
            Debug.Log("ENTERED");
            score = lastScore;
        }
        PlayerPrefs.SetInt(leaderboard.ToString(), score);
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
