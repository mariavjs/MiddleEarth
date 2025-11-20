// Assets/Scripts/InfernoConfig.cs
using UnityEngine;

public static class InfernoConfig
{
    private const string PREF_KEY = "InfernoDurationSeconds";
    private static float defaultDuration = 20f; // ajuste conforme design

    // valor acessível por todos
    public static float InfernoDurationSeconds { get; private set; }

    static InfernoConfig()
    {
        Load();
    }

    public static void Load()
    {
        InfernoDurationSeconds = PlayerPrefs.GetFloat(PREF_KEY, defaultDuration);
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat(PREF_KEY, InfernoDurationSeconds);
        PlayerPrefs.Save();
    }

    public static void SetInfernoDuration(float seconds)
    {
        InfernoDurationSeconds = Mathf.Max(0f, seconds);
        Save();
        Debug.Log("[InfernoConfig] SetInfernoDuration -> " + InfernoDurationSeconds);
    }

    public static void ReduceInfernoDuration(float seconds)
    {
        SetInfernoDuration(Mathf.Max(0f, InfernoDurationSeconds - seconds));
    }

    public static void ResetToDefault()
    {
        SetInfernoDuration(defaultDuration);
    }
}
