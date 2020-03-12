using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    enum Texts
    {
        TITLE,
        PLAY,
        LEADERBOARD,
        SETTINGS,
        EXIT,
        LANGUAGE_LABEL,
        LANGUAGE_SELECTION,
        REMOVE_ADS,
        FACE_LABEL,
        FACE_SLIDER,
        GAME_PLAY,
        BACK,
    }
    
    enum MenuState
    {
        MENU,
        SETTINGS,
        GAME_SELECTION
    }

    public GUIStyle _style;
    GUIStyle _titleStyle;
    Locale _locale;
    AdsManager _adsManager;
    MenuState _current = MenuState.MENU;

    Dictionary<Texts, Rect> _rects = new Dictionary<Texts, Rect>();

    Vector2 resolution;

    int _currentLanguagePos;

    // Game Config
    int _rubikSize;

    void Awake()
    {
        _adsManager = AdsManager.instance();
        _locale = Locale.instance(Application.systemLanguage);

        _titleStyle = new GUIStyle(_style);
        _titleStyle.fontSize = 200;
        
        _style.onActive.textColor = Color.red;
        _style.active.textColor = Color.red;

        _rubikSize = PlayerPrefs.GetInt(Constants.SHARED_PREFERENCES.RUBIK_SIZE.ToString(), 3);
        if (_rubikSize > Constants.MAX_RUBIK_SIZE) _rubikSize = Constants.MAX_RUBIK_SIZE;
        else if (_rubikSize < Constants.MIN_RUBIK_SIZE) _rubikSize = Constants.MIN_RUBIK_SIZE;
    }

    void calculateRects()
    {
        resolution = new Vector2(Screen.width, Screen.height);

        float topHeight = Screen.height / 4;

        float height = (Screen.height - topHeight) / 10;
        float width = Screen.width / 4;

        // Menu
        _rects[Texts.TITLE] = new Rect(0, 0, Screen.width, topHeight);
        _rects[Texts.PLAY] = new Rect(0.75f * width, topHeight, 2.5f * width, 3 * height);
        _rects[Texts.LEADERBOARD] = new Rect(0.75f * width, topHeight +  2 * height, 2.5f * width, 3 * height);
        _rects[Texts.SETTINGS] = new Rect(0.75f * width, topHeight + 4 * height, 2.5f * width, 3 * height);
        _rects[Texts.EXIT] = new Rect(0.75f * width, topHeight + 6 * height, 2.5f * width, 3 * height);

        // Settings
        float widthTwoColumns = Screen.width / 9;
        _rects[Texts.LANGUAGE_LABEL] = new Rect(0.25f * widthTwoColumns, topHeight + height, 4f * widthTwoColumns, 2 * height);
        _rects[Texts.LANGUAGE_SELECTION] = new Rect(4.5f * widthTwoColumns, topHeight + height, 4f * widthTwoColumns, 2 * height);        
        
        _rects[Texts.REMOVE_ADS] = new Rect(0.5f * width, topHeight + 3 * height, 3f * width, 3 * height);
        _rects[Texts.BACK] = new Rect(0.75f * width, topHeight + 6 * height, 2.5f * width, 2 * height);


        // Game
        _rects[Texts.FACE_LABEL] = new Rect(0.25f * widthTwoColumns, topHeight + height, 6f * widthTwoColumns, 2 * height);
        _rects[Texts.FACE_SLIDER] = new Rect(6.5f * widthTwoColumns, topHeight + height, 2 * widthTwoColumns, 2 * height);

        _rects[Texts.GAME_PLAY] = new Rect(0.75f * width, topHeight + 4 * height, 2.5f * width, 2 * height);

        _adsManager.requestBanner();
        _adsManager.showBanner();
    }

    private void Update()
    {
        if (resolution.x != Screen.width || resolution.y != Screen.height )
        {
            calculateRects();
        }   
    }

    private void OnGUI()
    {
        switch (_current)
        {
            case MenuState.MENU:
                makeMenu();
                break;
            case MenuState.SETTINGS:
                makeSettings();
                break;
            case MenuState.GAME_SELECTION:
                makeGameSelection();
                break;
        }
    }

    void makeMenu()
    {
        GUI.backgroundColor = Color.clear;
        GUI.Label(_rects[Texts.TITLE], _locale.getWord("rubik"), _titleStyle);

        GUI.backgroundColor = Color.red;
        if (GUI.Button(_rects[Texts.PLAY], _locale.getWord("play"), _style))
        {
            _current = MenuState.GAME_SELECTION;
        }
        GUI.backgroundColor = Color.yellow;
        if (GUI.Button(_rects[Texts.LEADERBOARD], _locale.getWord("leaderboard"), _style))
        {
            //_current = MenuState.GAME_SELECTION;
        }
        GUI.backgroundColor = Color.blue;
        if (GUI.Button(_rects[Texts.SETTINGS], _locale.getWord("settings"), _style))
        {
            _current = MenuState.SETTINGS;
            _currentLanguagePos = _locale.getCurrentLanguage();
        }
        GUI.backgroundColor = Color.green;
        if (GUI.Button(_rects[Texts.EXIT], _locale.getWord("exit"), _style))
        {
            Application.Quit();
        }
    }

    void makeSettings()
    {
        GUI.backgroundColor = Color.clear;
        GUI.Label(_rects[Texts.TITLE], _locale.getWord("settings").ToUpper(), _titleStyle);

        GUI.backgroundColor = Color.clear;
        GUI.Label(_rects[Texts.LANGUAGE_LABEL], _locale.getWord("language"), _style);

        GUI.backgroundColor = Color.green;
        if (GUI.Button(_rects[Texts.LANGUAGE_SELECTION], _locale.getWord(_locale.getCurrentLocale().ToString().ToLower()), _style))
        {
            _locale.changeCurrentLocale(_locale.getCurrentLocale() == Locale.LOCALES.ES ? Locale.LOCALES.EN : Locale.LOCALES.ES);
        }

        string rewardedLoaded = _adsManager.isRewardedLoaded();
        GUI.backgroundColor = rewardedLoaded.Equals(AdsManager.REWARDED_LOADED.LOAD.ToString()) ?  Color.red : Color.gray;
        bool isRewarded = rewardedLoaded.Equals(AdsManager.REWARDED_LOADED.LOAD.ToString()) || rewardedLoaded.Equals(AdsManager.REWARDED_LOADED.NON_LOAD.ToString());
        int fontSize = _style.fontSize;
        _style.fontSize = 70;
        if (GUI.Button(_rects[Texts.REMOVE_ADS], isRewarded ? _locale.getWord("remove_ads") : rewardedLoaded, _style))
        {
            if (rewardedLoaded.Equals(AdsManager.REWARDED_LOADED.LOAD.ToString()))
            {
                _adsManager.showRewarded();
            }
        }
        _style.fontSize = fontSize;

        GUI.backgroundColor = Color.blue;
        if (GUI.Button(_rects[Texts.BACK], _locale.getWord("back"), _style))
        {
            _current = MenuState.MENU;
        }
    }

    void getRubik(GameObject rubik, bool changed)
    {
        if (rubik == null || changed)
        {
            if (changed)
            {
                Destroy(rubik);
            }
            rubik = new GameObject();
            rubik.AddComponent<Rubik>();
            rubik.tag = "Player";
            rubik.GetComponent<Rubik>().generateRubikPreview(_rubikSize, Screen.height - (_rects[Texts.FACE_LABEL].y / 2));
        }
    }

    void makeGameSelection()
    {
        GameObject rubik = GameObject.FindGameObjectWithTag("Player");
        getRubik(rubik, false);
        GUI.backgroundColor = Color.clear;
        GUI.Label(_rects[Texts.FACE_LABEL], _locale.getWord("rubik_size"), _style);

        GUI.backgroundColor = Color.green;
        if (GUI.Button(_rects[Texts.FACE_SLIDER], _rubikSize.ToString() + "x" + _rubikSize.ToString(), _style))
        {
            if (_rubikSize >= Constants.MAX_RUBIK_SIZE)
            {
                _rubikSize = Constants.MIN_RUBIK_SIZE;
            }
            else
            {
                _rubikSize++;
            }
            getRubik(rubik, true);
        }


        GUI.backgroundColor = Color.red;
        if (GUI.Button(_rects[Texts.GAME_PLAY], _locale.getWord("play"), _style))
        {
            PlayerPrefs.SetInt(Constants.SHARED_PREFERENCES.RUBIK_SIZE.ToString(), _rubikSize);
            SceneManager.LoadScene("GameScene");
        }

        GUI.backgroundColor = Color.blue;
        if (GUI.Button(_rects[Texts.BACK], _locale.getWord("back"), _style))
        {
            _current = MenuState.MENU;
            Destroy(rubik);
        }

    }
}
