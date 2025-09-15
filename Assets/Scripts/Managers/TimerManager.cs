using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TimerManager : MonoBehaviour {


    // Stopwatch
    bool stopwatchActive = false;
    float currentTime;
    public TextMeshProUGUI currentTimeText;

    // Score
    int score;
    public TextMeshProUGUI scoreText;
    public float multiplier = 5;

    void Start() {
        currentTime = 0;
        score = 0;
        scoreText.text = score.ToString(); // Initialize score text
    }

    void Update() {
        if (stopwatchActive == true) {
            currentTime = currentTime + Time.deltaTime;

            // Score Part 2
            score = Mathf.RoundToInt(currentTime * multiplier);
            scoreText.text = score.ToString();
        }

        TimeSpan time = TimeSpan.FromSeconds(currentTime);
        currentTimeText.text = time.ToString(@"mm\:ss\:fff");
    }

    public void StartStopwatch() {
        stopwatchActive = true;
    }

    public void StopStopwatch() {
        stopwatchActive = false;
    }

    public void ResetStopwatchAndScore() {
        currentTime = 0;
        stopwatchActive = false;
        score = 0;
        currentTimeText.text = "00:00:000"; // Reset time text
        scoreText.text = score.ToString(); // Reset score text
    }


}
