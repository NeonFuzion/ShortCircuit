using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Transform levels;
    [SerializeField] UnityEvent onFinishGame;

    int index;

    LevelParent currentLevel;
    List<CircuitComponent> currentCircuitComponents;
    GameObject[] levelParents;

    public LevelParent CurrentLevel { get => currentLevel; }
    public List<CircuitComponent> CurrentCircuitComponents { get => currentCircuitComponents; }

    void Awake()
    {
        index = 0;

        levelParents = new GameObject[levels.childCount];
        for (int i = 0; i < levelParents.Length; i++)
        {
            levelParents[i] = levels.GetChild(i).gameObject;

            if (i == 0) continue;
            levelParents[i].SetActive(false);
        }

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
        currentCircuitComponents = currentLevel.CircuitComponents.ToList();
    }

    public void IncrementLevel()
    {
        if (index >= levelParents.Length - 1)
        {
            onFinishGame?.Invoke();
        }
        else
        {
            index++;
            levelParents[index].SetActive(true);
            InitializeLevel();
        }
    }

    public void HideOldLevel()
    {
        if (index <= 0) return;
        levelParents[index - 1].SetActive(false);
    }

    public void ShowAllLevels()
    {
        foreach (Transform level in levels)
        {
            level.gameObject.SetActive(true);
        }
    }
}
