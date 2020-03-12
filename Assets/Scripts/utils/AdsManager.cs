using GoogleMobileAds.Api;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdsManager : MonoBehaviour
{
    public const string PP_LAST_TIME_KEY = "LastAdsTime";
    public const string PP_TIME_KEY = "AdsTime";
    public const string PP_GAMES_TO_AD = "GamesToAd";

    const int MAX_GAMES = 10;

    public enum REWARDED_LOADED
    {
        LOAD,
        NON_LOAD
    }

    public const bool TEST = true;
    #region SINGLETON
    protected static AdsManager _instance = null;
    public static AdsManager instance()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject();
            _instance = go.AddComponent<AdsManager>();
        }
        PlayerPrefs.SetString(PP_LAST_TIME_KEY, getTime(DateTime.UtcNow));
        return _instance;
    }
    #endregion

    BannerView _bannerView;
    InterstitialAd _interstitial;
    RewardedAd _rewardedAd;
    int _rewardedAmount = 0;
    bool _rewardedLoad = false;

    private void Awake()
    {
        #if UNITY_ANDROID
                string appId = TEST ? "ca-app-pub-3940256099942544/6300978111" : "ca-app-pub-4136978037886351~7457426577";
        #elif UNITY_IPHONE
               string appId = TEST ? "ca-app-pub-3940256099942544/2934735716" :  "ca-app-pub-4136978037886351~9568584599";
        #else
               string appId = "unexpected_platform";
        #endif

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);
        requestInterstical();
        requestRewarded();
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (_rewardedAmount > 0)
        {
            Debug.Log("[LOG] - Amount: " + _rewardedAmount);
            try
            {
                Debug.Log("[LOG] - PP_LAST_TIME_KEY");
                PlayerPrefs.SetString(PP_LAST_TIME_KEY, getTime(DateTime.UtcNow));
                Debug.Log("[LOG] - Add seconds");
                DateTime dateTime = DateTime.UtcNow.AddSeconds(_rewardedAmount);
                Debug.Log("[LOG] - PP_TIME_KEY");
                PlayerPrefs.SetString(PP_TIME_KEY, getTime(dateTime));
            }
            catch (Exception e)
            {
                Debug.Log("[LOG] - Exception ---> " + e.Message);
            }
            hideBanner();

            _rewardedAmount = 0;
        }
    }

    public void requestBanner()
    {
        #if UNITY_ANDROID
            string adUnitId = TEST ? "ca-app-pub-3940256099942544/6300978111" : "ca-app-pub-4136978037886351/5259666315";
        #elif UNITY_IPHONE
            string adUnitId =  TEST ? "ca-app-pub-3940256099942544/6300978111" : "ca-app-pub-4136978037886351/8993869520";
        #else
            string adUnitId = "unexpected_platform";
        #endif

        this._bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        this._bannerView.LoadAd(request);

        if (getRemainTime().Equals("0"))
        {
            this._bannerView.Show();
        }
    }

    public void showBanner()
    {
        if (getRemainTime().Equals("0"))
        {
            if (this._bannerView == null)
            {
                requestBanner();
            }
            else
            {
                this._bannerView.Show();
            }
        }
    }

    public void hideBanner()
    {
        if (this._bannerView != null)
        {
            this._bannerView.Hide();
        }
    }


    private void requestInterstical()
    {
        #if UNITY_ANDROID
            string adUnitId = TEST ? "ca-app-pub-3940256099942544/1033173712" : "ca-app-pub-4136978037886351/7572976646";
        #elif UNITY_IPHONE
            string adUnitId =  TEST ? "ca-app-pub-3940256099942544/4411468910" : "ca-app-pub-4136978037886351/9582054701";
        #else
            string adUnitId = "unexpected_platform";
        #endif

        this._interstitial = new InterstitialAd(adUnitId);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this._interstitial.LoadAd(request);
    }

    public void showInterstical()
    {
        if (getRemainTime().Equals("0"))
        {
            if (PlayerPrefs.GetInt(PP_GAMES_TO_AD, MAX_GAMES) <= 0)
            {
                PlayerPrefs.SetInt(PP_GAMES_TO_AD, MAX_GAMES);
                if (this._interstitial != null && this._interstitial.IsLoaded())
                {
                    this._interstitial.Show();
                }
                else
                {
                    requestInterstical();
                    this._interstitial.Show();
                }
            }
            else
            {
                PlayerPrefs.SetInt(PP_GAMES_TO_AD, PlayerPrefs.GetInt(PP_GAMES_TO_AD, MAX_GAMES) - 1);
            }
        }
    }

    private void requestRewarded()
    {
        _rewardedLoad = false;
        #if UNITY_ANDROID
        string adUnitId = TEST ? "ca-app-pub-3940256099942544/5224354917" : "ca-app-pub-4136978037886351/3946584643";
        #elif UNITY_IPHONE
             string adUnitId = TEST ? "ca-app-pub-3940256099942544/1712485313" : "ca-app-pub-4136978037886351/9537343742";
        #else
             string adUnitId = "unexpected_platform";
        #endif

        this._rewardedAd = new RewardedAd(adUnitId);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this._rewardedAd.LoadAd(request);

        // Called when the user should be rewarded for interacting with the ad.
        this._rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        this._rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        this._rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        this._rewardedAd.OnAdClosed += HandleOnClosed;
    }


    public void showRewarded()
    {
        this._rewardedAd.Show();
    }

    public string isRewardedLoaded()
    {
        string loaded = getRemainTime();
        if (loaded.Equals("0"))
        {
            if(this._rewardedAd != null && this._rewardedAd.IsLoaded() && _rewardedLoad)
            {
                return REWARDED_LOADED.LOAD.ToString();
            }
            else
            {
                return REWARDED_LOADED.NON_LOAD.ToString();
            }
        }
        else
        {
            return loaded;
        }
    }
    public string getRemainTime()
    {
        string lastTime = PlayerPrefs.GetString(PP_LAST_TIME_KEY, null);
        string nextTime = PlayerPrefs.GetString(PP_TIME_KEY, null);
        if (lastTime == null || lastTime.Length == 0 || nextTime == null || nextTime.Length == 0)
        {
            return "0";
        }
        long lastTimeDouble = long.Parse(lastTime);
        long nextTimeDouble = long.Parse(nextTime);
        long currentTimeDouble = long.Parse(getTime(DateTime.UtcNow));
        if (currentTimeDouble < lastTimeDouble || lastTimeDouble == 0 || nextTimeDouble == 0)
        {
            return "0";
        }
        long time = nextTimeDouble - currentTimeDouble;
        if (time <= 0)
        {
            return "0";
        }
        int minutes = Mathf.RoundToInt(time / 60);
        int hours = Mathf.RoundToInt(minutes / 60);
        minutes -= hours * 60;
        int seconds = (int) time - ((minutes * 60) + (hours * 3600));

        string timeString = "";
        if (hours > 0)
        {
            timeString = hours.ToString("D2") + " : ";
        }
        if (minutes > 0)
        {
            timeString += minutes.ToString("D2") + " : ";
        }
        timeString += seconds.ToString("D2");
        return timeString;
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        _rewardedLoad = true;
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        requestRewarded();
    }

    public void HandleOnClosed(object sender, EventArgs args)
    {
        requestRewarded();
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        _rewardedAmount = args.Amount < 1000 ? 7200 : (int) args.Amount;
    }

    private void OnApplicationQuit()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }
        if (_interstitial != null)
        {
            _interstitial.Destroy();
        }
        if (SceneManager.GetActiveScene().name.Equals("GameScene"))
        {
            PlayerPrefs.SetInt(PP_GAMES_TO_AD, PlayerPrefs.GetInt(PP_GAMES_TO_AD, MAX_GAMES) - 1);
        }
    }

    static string getTime(DateTime date)
    {
        DateTime baseDate = new DateTime(1970, 1, 1);
        TimeSpan diff = date - baseDate;
        return Math.Round(diff.TotalSeconds).ToString();
    }
}
