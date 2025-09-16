using System;
using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class LevelTimeData {
    public string levelName;
    public float completionTime;
}

public class TimerManager : MonoBehaviour {
    float currentTime;
    public TextMeshProUGUI currentTimeText;
    string currentLevelName;

    void Start() {
        currentTime = 0;
        currentLevelName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    void Update() {
        currentTime += Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(currentTime);
        currentTimeText.text = time.ToString(@"mm\:ss\:fff");
    }

    void OnDestroy() {
        SaveLevelTime();
        
    }

    void SaveLevelTime() {
        LevelTimeData data = new LevelTimeData {
            levelName = currentLevelName,
            completionTime = currentTime
        };

        string json = JsonUtility.ToJson(data, true);
        string filePath = Path.Combine(Application.persistentDataPath, $"{currentLevelName}_time.json");
        File.WriteAllText(filePath, json);

        Debug.Log($"Time saved for {currentLevelName}: {currentTime:F2}s");
    }
}