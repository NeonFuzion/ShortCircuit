using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreKeeper : MonoBehaviour
{
    [SerializeField] float resetTime, trackerSpeed;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] LevelManager levelManager;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform tracker, scoreParent;
    [SerializeField] GameObject prefabScoreIcon;
    [SerializeField] Player player;
    [SerializeField] UnityEvent<Transform> onTimeUp;
    [SerializeField] UnityEvent onStartLevel;

    float currentTime;
    int scoreIndex;

    List<CircuitComponent> currentCircuitComponents, allCircuitComponents;
    List<Vector2> positions;
    ScoreMode scoreMode;

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
        MoveTracker();
    }

    IEnumerator ResetCoroutine()
    {
        scoreMode = ScoreMode.Idling;
        yield return new WaitForSeconds(resetTime);
        lineRenderer.positionCount = 1;
        StartGame();
    }

    void StartLevel()
    {
        scoreParent.gameObject.SetActive(false);
        scoreMode = ScoreMode.Timing;
        currentTime = levelManager.CurrentLevel.Time;
        allCircuitComponents = levelManager.CurrentCircuitComponents;
        currentCircuitComponents = new();
        LevelParent level = levelManager.CurrentLevel;
        player.Initialize(allCircuitComponents.Count, level.transform, level.Battery);
        onStartLevel?.Invoke();
    }

    void RunTimer()
    {
        if (scoreMode != ScoreMode.Timing) return;
        if (timerText.text.Equals("Time's Up!")) return;
        timerText.SetText((Mathf.Round(currentTime * 1000) / 1000).ToString());
        currentTime -= Time.deltaTime;

        if (currentTime > 0) return;
        currentTime = 0;
        timerText.SetText("Time's Up!");
        onTimeUp?.Invoke(levelManager.transform);
    }

    void MoveTracker()
    {
        if (scoreMode != ScoreMode.Grading) return;
        int index = positions.Count - 1;
        Vector3 target = positions[index];
        Vector3 direction = target - tracker.position;
        Vector3 movement = direction.normalized * trackerSpeed * Time.deltaTime;

        if (direction.sqrMagnitude < movement.sqrMagnitude || Mathf.Abs(direction.sqrMagnitude - movement.sqrMagnitude) < 0.002f)
        {
            tracker.position = target;
            positions.RemoveAt(index);
            index = lineRenderer.positionCount++;
            lineRenderer.SetPosition(index, tracker.position);

            if (positions.Count > 0) return;
            ResetLevel();
        }
        else
        {
            tracker.position += movement;
        }

        Collider2D collider = Physics2D.OverlapCircle(tracker.position, 0.2f);

        if (!collider) return;
        CircuitComponent script = collider.GetComponent<CircuitComponent>();

        if (!allCircuitComponents.Contains(script)) return;
        if (currentCircuitComponents.Contains(script)) return;
        if (script.IsPassable)
        {
            (script as LightBulb).PowerBulb();
            currentCircuitComponents.Add(script);
            Transform scoreIcon = scoreParent.GetChild(scoreIndex++);
            scoreIcon.GetChild(0).gameObject.SetActive(false);
            scoreIcon.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            ResetLevel();
        }
    }

    void StartGame()
    {
        if (currentCircuitComponents.Count == allCircuitComponents.Count)
        {
            levelManager.IncrementLevel();
        }
        else
        {
            levelManager.CurrentLevel.ClearLevel();
            currentCircuitComponents.Clear();
        }
        StartLevel();
    }

    public void GradeLevel()
    {
        positions = levelManager.CurrentLevel.GetWirePoints();
        positions.Reverse();
        tracker.position = positions[0];
        positions.RemoveAt(positions.Count - 1);
        lineRenderer.SetPosition(0, tracker.position);
        scoreIndex = 0;

        foreach (Transform icon in scoreParent)
        {
            Destroy(icon.gameObject);
        }
        foreach (CircuitComponent component in allCircuitComponents)
        {
            Instantiate(prefabScoreIcon, scoreParent);
        }
        scoreParent.gameObject.SetActive(true);
        scoreMode = ScoreMode.Grading;
    }

    public void ResetLevel()
    {
        StartCoroutine(ResetCoroutine());
    }

    enum ScoreMode { None, Timing, Grading, Idling }
}