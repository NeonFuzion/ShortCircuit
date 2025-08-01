using System.Collections.Generic;
using UnityEngine;

public class LevelParent : MonoBehaviour
{
    [SerializeField] float time;
    [SerializeField] Transform componentParent;
    [SerializeField] Battery battery;

    List<CircuitComponent> circuitComponents;

    public float Time { get => time; }
    
    public Battery Battery { get => battery; }
    public List<CircuitComponent> CircuitComponents { get => circuitComponents; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize()
    {
        circuitComponents = new();
        foreach (Transform child in componentParent)
        {
            circuitComponents.Add(child.GetComponent<CircuitComponent>());
        }
    }
}
