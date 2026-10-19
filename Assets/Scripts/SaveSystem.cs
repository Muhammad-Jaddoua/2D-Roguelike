using UnityEngine;

/// <summary>
/// Data that is kept between play sessions. Serialized to JSON with JsonUtility.
/// </summary>
[System.Serializable]
public class SaveData
{
    [SerializeField] private int m_BestLevel;
    [SerializeField] private int m_GamesPlayed;

    public int BestLevel { get { return m_BestLevel; } set { m_BestLevel = value; } }
    public int GamesPlayed { get { return m_GamesPlayed; } set { m_GamesPlayed = value; } }
}

/// <summary>
/// Bonus feature: Save &amp; Load. The SaveData object is converted to a JSON string
/// and stored in PlayerPrefs (which also works in WebGL builds, unlike file I/O).
/// </summary>
public static class SaveSystem
{
    private const string Key = "RoguelikeSaveData";

    public static SaveData Load()
    {
        try
        {
            if (PlayerPrefs.HasKey(Key))
            {
                SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key));
                if (data != null)
                {
                    return data;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not read save data, starting fresh: " + e.Message);
        }

        return new SaveData();
    }

    public static void Save(SaveData data)
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static void RecordLevelReached(int level)
    {
        SaveData data = Load();
        if (level > data.BestLevel)
        {
            data.BestLevel = level;
            Save(data);
        }
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
