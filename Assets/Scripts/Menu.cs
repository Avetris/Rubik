using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Tooltip("Game Canvas")]
    public GameObject _menu, _settings, _gameSelection;
    public GameObject _sizeBtn;
    public GameObject _adsBtn;

    string _rewardedLoaded;

    Locale _locale;
    AdsManager _adsManager;
    LeaderboardManager _leaderboardManager;

    int _currentLanguagePos;

    // Game Config
    int _rubikSize;


    void Awake()
    {
        _adsManager = AdsManager.instance();
        _locale = Locale.instance(Application.systemLanguage);
        _leaderboardManager = LeaderboardManager.instance();

        _rubikSize = PlayerPrefs.GetInt(Constants.SHARED_PREFERENCES.RUBIK_SIZE.ToString(), 3);
        if (_rubikSize > Constants.MAX_RUBIK_SIZE) _rubikSize = Constants.MAX_RUBIK_SIZE;
        else if (_rubikSize < Constants.MIN_RUBIK_SIZE) _rubikSize = Constants.MIN_RUBIK_SIZE;
        applyLocation();
    }

    void applyLocation()
    {
        foreach(TextMeshProUGUI c in _menu.GetComponentsInChildren<TextMeshProUGUI>(true)){
            c.text = _locale.getWord(c.gameObject.name);
        }
        foreach (TextMeshProUGUI c in _settings.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (c.gameObject.name.Equals("languageBTN"))
            {
                c.text = _locale.getWord(_locale.getCurrentLocale().ToString().ToLower());
            }
            else
            {
                c.text = _locale.getWord(c.gameObject.name);
            }
        }
        foreach (TextMeshProUGUI c in _gameSelection.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (c.gameObject.name.Equals("rubikSizeBTN"))
            {
                c.text = _rubikSize.ToString() + "x" + _rubikSize.ToString();
            }
            else
            {
                c.text = _locale.getWord(c.gameObject.name);
            }
        }
        _adsManager.showBanner();
    }


    public void change(int type)
    {
        _menu.SetActive(false);
        _gameSelection.SetActive(false);
        _settings.SetActive(false);
        GameObject rubik = GameObject.FindGameObjectWithTag("Player");
        if(rubik != null)
            Destroy(rubik);
        switch (type)
        {
            case 0: _menu.SetActive(true); break;
            case 1: _gameSelection.SetActive(true); changeSize(false); break;
            case 2: _settings.SetActive(true); break;
            case 3:
                PlayerPrefs.SetInt(Constants.SHARED_PREFERENCES.RUBIK_SIZE.ToString(), _rubikSize);
                SceneManager.LoadScene("GameScene");
                break;
        }
    }

    public void quit()
    {
        Application.Quit();
    }

    public void changeLanguage()
    {
        _locale.changeCurrentLocale(_locale.getCurrentLocale() == Locale.LOCALES.ES ? Locale.LOCALES.EN : Locale.LOCALES.ES);
        applyLocation();
    }

    public void showLeaderboard()
    {
        _leaderboardManager.showLeaderboard();
    }

    public void removeAds()
    {
        if (_rewardedLoaded.Equals(AdsManager.REWARDED_LOADED.LOAD.ToString()))
        {
            _adsManager.showRewarded();
        }
    }

    private void FixedUpdate()
    {
        if (_settings.activeSelf)
        {
            _rewardedLoaded = _adsManager.isRewardedLoaded();

            _adsBtn.GetComponent<Image>().color = _rewardedLoaded.Equals(AdsManager.REWARDED_LOADED.LOAD.ToString()) ? Color.red : Color.gray;
            bool isRewarded = _rewardedLoaded.Equals(AdsManager.REWARDED_LOADED.LOAD.ToString()) || _rewardedLoaded.Equals(AdsManager.REWARDED_LOADED.NON_LOAD.ToString());
            _adsBtn.GetComponentInChildren<TextMeshProUGUI>().text = isRewarded ? _locale.getWord("remove_ads") : _rewardedLoaded;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_menu.activeSelf)
            {
                quit();
            }
            else
            {
                change(0);
            }
        }
    }

    void getRubik(GameObject rubik)
    {
        if (rubik != null)
        {
            Destroy(rubik);
        }
        rubik = new GameObject();
        rubik.AddComponent<Rubik>();
        rubik.tag = "Player";
        rubik.GetComponent<Rubik>().generateRubikPreview(_rubikSize, Screen.height - Screen.height / 6);
    }

    public void changeSize(bool change)
    {
        if (change)
        {
            if (_rubikSize >= Constants.MAX_RUBIK_SIZE)
            {
                _rubikSize = Constants.MIN_RUBIK_SIZE;
            }
            else
            {
                _rubikSize++;
            }
        }
        _sizeBtn.GetComponent<TextMeshProUGUI>().text = _rubikSize.ToString() + "x" + _rubikSize.ToString();

        GameObject gm = GameObject.FindGameObjectWithTag("Player");

        getRubik(gm);
    }
}
