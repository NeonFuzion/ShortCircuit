using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreKeeper : MonoBehaviour
{
    [SerializeField] float countCooldown;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] LevelManager levelManager;
    [SerializeField] Player player;
    [SerializeField] UnityEvent<Transform> onTimeUp;
    [SerializeField] UnityEvent onStartLevel;

    float currentTime;
    int counter;

    List<CircuitComponent> currentCircuitComponents, allCircuitComponents;

    void Awake()
    {
        StartLevel();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RunTimer();
    }

    void StartLevel()
    {
        currentTime = levelManager.CurrentLevel.Time;
        allCircuitComponents = levelManager.CurrentCircuitComponents;
        currentCircuitComponents = new();
        player.SetBattery(levelManager.CurrentLevel.Battery);
        player.Initialize(allCircuitComponents.Count, levelManager.CurrentLevel.transform);
        onStartLevel?.Invoke();
    }

    void RunTimer()
    {
        if (currentCircuitComponents != null) return;
        if (timerText.text.Equals("Time's Up!")) return;
        timerText.SetText((Mathf.Round(currentTime * 1000) / 1000).ToString());
        currentTime -= Time.deltaTime;

        if (currentTime > 0) return;
        currentTime = 0;
        timerText.SetText("Time's Up!");
        onTimeUp?.Invoke(levelManager.transform);
    }

    void StartGame()
    {
        if (counter == allCircuitComponents.Count)
            levelManager.IncrementLevel();
        StartLevel();
    }

    IEnumerator GradingCoroutine()
    {
        yield return new WaitForSeconds(countCooldown);

        (currentCircuitComponents[0] as LightBulb)?.PowerBulb();
        currentCircuitComponents.RemoveAt(0);
        counter++;

        if (currentCircuitComponents.Count > 0)
        {
            StartCoroutine(GradingCoroutine());
        }
        else
        {
            yield return new WaitForSeconds(countCooldown);
            StartGame();
        }
    }

    public void GradeLevel(List<CircuitComponent> components)
    {
        currentCircuitComponents = components;
        counter = 0;
        StartCoroutine(GradingCoroutine());
    }
}