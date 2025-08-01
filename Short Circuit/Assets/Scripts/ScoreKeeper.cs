using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreKeeper : MonoBehaviour
{
    [SerializeField] float startTime;
    [SerializeField] TextMeshProUGUI timerText;
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
}
