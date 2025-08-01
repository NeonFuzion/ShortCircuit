using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] GameObject[] levelParents;

    int index;

    LevelParent currentLevel;
    List<CircuitComponent> currentCircuitComponents;

    public LevelParent CurrentLevel { get => currentLevel; }
    public List<CircuitComponent> CurrentCircuitComponents { get => currentCircuitComponents; }

    void Awake()
    {
        index = 0;

        InitializeLevel();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void InitializeLevel()
    {
        GameObject newLevel = levelParents[index];
        currentLevel = newLevel.GetComponent<LevelParent>();
        currentLevel.Initialize();
        currentCircuitComponents = currentLevel.CircuitComponents;
    }

    public void IncrementLevel()
    {
        levelParents[index++].SetActive(false);
        levelParents[index].SetActive(true);
        InitializeLevel();
    }
}
