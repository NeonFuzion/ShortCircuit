using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreKeeper : MonoBehaviour
{
    [SerializeField] float countCooldown, startTime;
    [SerializeField] int lightBulbPoints, timePoints;
    [SerializeField] TextMeshProUGUI scoreText, timerText;
    [SerializeField] UnityEvent onTimeUp;

    float currentTime;

    List<LightBulb> lightBulbs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTime = startTime;
    }

    // Update is called once per frame
    void Update()
    {
        RunTimer();
    }

    void RunTimer()
    {
        if (lightBulbs != null) return;
        if (timerText.text.Equals("Time's Up!")) return;
        timerText.SetText((Mathf.Round(currentTime * 1000) / 1000).ToString());
        currentTime -= Time.deltaTime;

        if (currentTime > 0) return;
        currentTime = 0;
        timerText.SetText("Time's Up!");
        onTimeUp?.Invoke();
    }

    IEnumerator ScoringCoroutine()
    {
        yield return new WaitForSeconds(countCooldown);

        int score = int.Parse(scoreText.text);
        int newScore = score + lightBulbPoints;
        scoreText.SetText(newScore.ToString());
        lightBulbs[0].PowerBulb();
        lightBulbs.RemoveAt(0);

        if (lightBulbs.Count > 0)
        StartCoroutine(ScoringCoroutine());
    }

    public void CountScore(List<LightBulb> lightBulbs)
    {
        this.lightBulbs = lightBulbs;
        scoreText.SetText(Mathf.RoundToInt(currentTime * timePoints).ToString());
        StartCoroutine(ScoringCoroutine());
    }
}
