// Assets/Scripts/InfernoConfig.cs
using UnityEngine;

public static class LivesConfig
{
    private const string PREF_KEY = "CurrentMaxLives";
    private static int defaultMaxLives = 3; // ajuste conforme design

    // valor acessível por todos
    public static int MaxLives { get; private set; }

    static LivesConfig()
    {
        Load();
    }

    public static void Load()
    {
        MaxLives = PlayerPrefs.GetInt(PREF_KEY, defaultMaxLives);
    }

    public static void Save()
    {
        PlayerPrefs.SetInt(PREF_KEY, MaxLives);
        PlayerPrefs.Save();
    }

    public static void SetMaximumLives(int lives)
    {
        MaxLives = lives;
        Save();
    }

    public static void AddMaximumLife()
    {
        SetMaximumLives(MaxLives + 1);
    }

    public static void ResetToDefault()
    {
        SetMaximumLives(defaultMaxLives);
    }
}
