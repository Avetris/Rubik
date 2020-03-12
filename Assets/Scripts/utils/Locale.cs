using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locale
{
    public const string PP_KEY = "Locale";

    #region SINGLETON
    protected static Locale _instance = null;
    public static Locale instance(SystemLanguage systemLanguage) { 
        if(_instance == null){
            _instance = new Locale(systemLanguage);
        }
        return _instance; 
    }
    #endregion
        
    public enum LOCALES
    {
        EN,
        ES,
    }

    Dictionary<string, string> _dictionary = new Dictionary<string, string>();
    LOCALES _currentLocale;

    static public string[] getPossibleLocales()
    {
        return Enum.GetNames(typeof(LOCALES));
    }
    
    private Locale(SystemLanguage systemLanguage)
    {
        string current = PlayerPrefs.GetString(PP_KEY, null);
        if (current == null || !Enum.TryParse<LOCALES>(current, out _currentLocale))
        {
            switch (systemLanguage)
            {
                case SystemLanguage.English:
                    _currentLocale = LOCALES.EN;
                    break;
                case SystemLanguage.Spanish:
                    _currentLocale = LOCALES.ES;
                    break;
                default:
                    _currentLocale = LOCALES.EN;
                    break;
            }
        }
        loadDictionary();
    }

    public void changeCurrentLocale(LOCALES locales)
    {
        if (_currentLocale != locales || _dictionary == null || _dictionary.Count == 0)
        {
            PlayerPrefs.SetString(PP_KEY, locales.ToString());
            _currentLocale = locales;
            loadDictionary();
        }
    }

    public LOCALES getCurrentLocale()
    {
        return _currentLocale;
    }

    void loadDictionary()
    {
        string path = "Locale/" + _currentLocale.ToString().ToLower();

        TextAsset targetFile = Resources.Load<TextAsset>(path);
        
        loadJSONFromFile(targetFile);
    }

    void loadJSONFromFile(TextAsset targetFile)
    {
        if (targetFile != null && targetFile.text != null)
        {
            if (targetFile.text.Contains("\n"))
            {
                string[] lines = targetFile.text.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Contains(":"))
                    {
                        string[] words = line.Split(new char[] { ':' }, 2);
                        if (words.Length == 2)
                        {
                            string key = getWordFromQuations(words[0]);
                            string word = getWordFromQuations(words[1]);
                            if (key.Length > 0 && word.Length > 0)
                            {
                                _dictionary[key] = word;
                            }
                        }
                    }
                }
            }
        }
    }

    string getWordFromQuations(string quations)
    {
        string[] parts = quations.Split('"');
        string word = "";
        if(parts.Length >= 2)
        {
            for(int i = 1; i < parts.Length - 1; i++)
            {
                word += parts[i];
            }
        }
        return word;
    }

    public string getWord(string key)
    {
        string word;
        if (_dictionary != null && _dictionary.Count > 0)
        {
            if (!_dictionary.TryGetValue(key, out word))
            {
                throw new System.Exception("Word with key " + key + " not defined in " + _currentLocale.ToString());
            }
        }
        else
        {
            throw new System.Exception("No dictionary defined");
        }
        return word;
    }

    public int getCurrentLanguage()
    {
        int i = 0;
        foreach (string s in Enum.GetNames(typeof(LOCALES)))
        {
            if (s.Equals(_currentLocale))
            {
                return i;
            }
            i++;
        }
        return 0;
    }
}
