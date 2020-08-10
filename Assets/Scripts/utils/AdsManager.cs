﻿using GoogleMobileAds.Api;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdsManager : MonoBehaviour
{
    public const string PP_GAMES_TO_AD = "GamesToAd";

    const int MAX_GAMES = 5;
    
    #region SINGLETON
    protected static AdsManager _instance = null;
    public static AdsManager instance()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject();
            _instance = go.AddComponent<AdsManager>();
        }
        return _instance;
    }
    #endregion

    BannerView _bannerView;
    InterstitialAd _interstitial;

    bool showRealAds()
    {
        return !Application.isEditor && !Debug.isDebugBuild;
    }

    private void Awake()
    {
        #if UNITY_ANDROID
                string appId = !showRealAds() ? "ca-app-pub-3940256099942544/6300978111" : "ca-app-pub-4136978037886351~7457426577";
        #elif UNITY_IPHONE
               string appId = !showRealAds()? "ca-app-pub-3940256099942544/2934735716" :  "ca-app-pub-4136978037886351~9568584599";
        #else
               string appId = "unexpected_platform";
        #endif

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);
        requestInterstical();
        DontDestroyOnLoad(this.gameObject);
    }

    public void requestBanner()
    {
        #if UNITY_ANDROID
            string adUnitId = !showRealAds() ? "ca-app-pub-3940256099942544/6300978111" : "ca-app-pub-4136978037886351/5259666315";
        #elif UNITY_IPHONE
            string adUnitId =  !showRealAds() ? "ca-app-pub-3940256099942544/6300978111" : "ca-app-pub-4136978037886351/8993869520";
        #else
            string adUnitId = "unexpected_platform";
        #endif

        this._bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        this._bannerView.LoadAd(request);
        
        this._bannerView.Show();
    }

    public void showBanner()
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
            string adUnitId = !showRealAds() ? "ca-app-pub-3940256099942544/1033173712" : "ca-app-pub-4136978037886351/7572976646";
        #elif UNITY_IPHONE
            string adUnitId =  !showRealAds() ? "ca-app-pub-3940256099942544/4411468910" : "ca-app-pub-4136978037886351/9582054701";
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
