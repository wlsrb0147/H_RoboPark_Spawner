using System;
using System.IO;
using UnityEngine;

[Serializable]
public class Settings
{
    public string url;
}

public class JsonSaver : MonoBehaviour
{
    public Settings settings;
    public static JsonSaver Instance;
    
    private void Awake()
    {
        Instance = this;
        settings = LoadJsonData<Settings>("settings.json");
    }
    
    private T LoadJsonData<T>(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        filePath = filePath.Replace("\\", "/");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log("Loaded JSON: " + json); // JSON 문자열 출력
            return JsonUtility.FromJson<T>(json);
        }

        Debug.LogWarning("File does not exist!");
        return default;
    }
}
