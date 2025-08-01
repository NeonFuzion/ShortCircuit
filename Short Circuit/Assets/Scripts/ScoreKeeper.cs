using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreKeeper : MonoBehaviour
{
    [SerializeField] float countCooldown, trackerSpeed;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] LevelManager levelManager;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform tracker;
    [SerializeField] Player player;
    [SerializeField] UnityEvent<Transform> onTimeUp;
    [SerializeField] UnityEvent onStartLevel;

    float currentTime;
    bool grading;

    List<CircuitComponent> currentCircuitComponents, allCircuitComponents;
    List<Vector2> positions;

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

    void StartLevel()
    {
        grading = false;
        currentTime = levelManager.CurrentLevel.Time;
        allCircuitComponents = levelManager.CurrentCircuitComponents;
        currentCircuitComponents = new();
        LevelParent level = levelManager.CurrentLevel;
        player.Initialize(allCircuitComponents.Count, level.transform, level.Battery);
        onStartLevel?.Invoke();
    }

    void RunTimer()
    {
        if (grading) return;
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
        if (!grading) return;
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
            grading = false;
            StartGame();
        }
        else
        {
            Debug.Log(movement);
            tracker.position += movement;
        }

        Collider2D collider = Physics2D.OverlapCircle(tracker.position, 0.2f);

        if (!collider) return;
        CircuitComponent script = collider.GetComponent<CircuitComponent>();

        if (!allCircuitComponents.Contains(script)) return;
        if (currentCircuitComponents.Contains(script)) return;
        (script as LightBulb).PowerBulb();
        currentCircuitComponents.Add(script);
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

    public void GradeLevel(List<CircuitComponent> components)
    {
        grading = true;
        positions = levelManager.CurrentLevel.GetWirePoints();
        positions.Reverse();
        tracker.position = positions[0];
        positions.RemoveAt(positions.Count - 1);
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, tracker.position);
    }
}