using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelTimeDataManager : MonoBehaviour {
    public List<LevelTimeData> levelTimes = new List<LevelTimeData>();

    void Start() {
        LoadAllLevelTimes();
    }

    public void LoadAllLevelTimes() {
        levelTimes.Clear();
        string directoryPath = Application.persistentDataPath;
        string[] files = Directory.GetFiles(directoryPath, "*_time.json");

        foreach (string file in files) {
            try {
                string json = File.ReadAllText(file);
                LevelTimeData data = JsonUtility.FromJson<LevelTimeData>(json);
                levelTimes.Add(data);
                Debug.Log($"Loaded: {data.levelName} - {data.completionTime:F2}s");
            }
            catch (System.Exception e) {
                Debug.LogError($"Error loading file {file}: {e.Message}");
            }
        }
    }

    // Optional: Method to get a specific level's time
    public float GetLevelTime(string levelName) {
        foreach (LevelTimeData data in levelTimes) {
            if (data.levelName == levelName)
                return data.completionTime;
        }
        return -1; // Return -1 if not found
    }
}