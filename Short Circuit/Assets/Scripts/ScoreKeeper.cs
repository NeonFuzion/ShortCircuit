using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScoreKeeper : MonoBehaviour
{
    public static ScoreKeeper Instance;

    [SerializeField] float resetTime, trackerSpeed;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] LevelManager levelManager;
    [SerializeField] LineRenderer powerRenderer;
    [SerializeField] Transform tracker, scoreParent;
    [SerializeField] GameObject prefabScoreIcon;
    [SerializeField] Player player;
    [SerializeField] Image resetScreen;
    [SerializeField] UnityEvent<Transform> onTimeUp;
    [SerializeField] UnityEvent onStartLevel, onSuccessfulLevel, onFailLevel;

    float currentTime;
    int scoreIndex;

    List<CircuitComponent> currentCircuitComponents, allCircuitComponents;
    List<Vector2> wirePoints;
    ScoreMode scoreMode;
    Animator animator;

    void Awake()
    {
        if (!Instance) Instance = this;

        currentCircuitComponents = new ();

        StartLevel();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        RunTimer();
        MoveTracker();
    }

    IEnumerator ChangeLevelCoroutine()
    {
        scoreMode = ScoreMode.Idling;
        yield return new WaitForSeconds(resetTime);
        StartGame();
    }

    void StartLevel()
    {
        scoreParent.gameObject.SetActive(false);
        scoreMode = ScoreMode.Timing;
        LevelParent level = levelManager.CurrentLevel;
        currentTime = level.Time;
        allCircuitComponents = levelManager.CurrentCircuitComponents;
        currentCircuitComponents.Clear();
        player.Initialize(allCircuitComponents.Count, level.transform, level.Battery);
        onStartLevel?.Invoke();
    }

    void ResetFailedLevel()
    {
        powerRenderer.positionCount = 0;
        scoreParent.gameObject.SetActive(false);
        levelManager.CurrentLevel.ClearLevel();
        currentCircuitComponents.Clear();
        onFailLevel?.Invoke();
        StartLevel();
    }

    void RunTimer()
    {
        if (scoreMode != ScoreMode.Timing) return;
        timerText.SetText(Math.Round(currentTime, 2) + "");
        currentTime -= Time.deltaTime;

        if (currentTime > 0) return;
        currentTime = 0;
        timerText.SetText("Time's Up!");
        onTimeUp?.Invoke(levelManager.transform);
    }

    void MoveTracker()
    {
        if (scoreMode != ScoreMode.Grading) return;
        int index = wirePoints.Count - 1;
        Vector3 target = wirePoints[index];
        Vector3 direction = target - tracker.position;
        Vector3 movement = direction.normalized * trackerSpeed * Time.deltaTime;

        if (direction.sqrMagnitude < movement.sqrMagnitude || Mathf.Abs(direction.sqrMagnitude - movement.sqrMagnitude) < 0.002f)
        {
            tracker.position = target;
            wirePoints.RemoveAt(index);
            index = powerRenderer.positionCount++;
            powerRenderer.SetPosition(index, tracker.position);

            if (wirePoints.Count > 0) return;
            IncrementLevel();
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
            script.ActivateComponent();
            currentCircuitComponents.Add(script);
            Transform scoreIcon = scoreParent.GetChild(scoreIndex++);
            scoreIcon.GetChild(0).gameObject.SetActive(false);
            scoreIcon.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            FailLevel();
        }
    }

    void StartGame()
    {
        if (currentCircuitComponents.Count == allCircuitComponents.Count)
        {
            levelManager.IncrementLevel();
            onSuccessfulLevel?.Invoke();
            StartLevel();
        }
        else
        {
            FailLevel();
        }
    }

    public void GradeLevel()
    {
        wirePoints = levelManager.CurrentLevel.GetWirePoints();
        wirePoints.Reverse();
        tracker.position = wirePoints[0];
        wirePoints.RemoveAt(wirePoints.Count - 1);
        powerRenderer.positionCount = 1;
        powerRenderer.SetPosition(0, tracker.position);
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

    public void IncrementLevel()
    {
        StartCoroutine(ChangeLevelCoroutine());
    }

    public void AddOnFailLevelListener(UnityAction unityAction)
    {
        onFailLevel?.AddListener(unityAction);
    }

    public void FailLevel()
    {
        scoreMode = ScoreMode.Idling;
        animator.CrossFade("FadeFull", 0, 0);
    }

    enum ScoreMode { None, Timing, Grading, Idling }
}