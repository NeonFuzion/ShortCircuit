using System.Collections.Generic;
using UnityEngine;

public class LevelParent : MonoBehaviour
{
    [SerializeField] float time;
    [SerializeField] Transform componentParent, wireParent;
    [SerializeField] GameObject prefabWire;
    [SerializeField] Battery battery;

    List<CircuitComponent> circuitComponents;
    LineRenderer wireRenderer, shadowRenderer;
    GameObject currentWire;

    public float Time { get => time; }

    public Battery Battery { get => battery; }
    public LineRenderer WireRenderer { get => wireRenderer; }
    public LineRenderer ShadowRenderer { get => shadowRenderer; }
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

    public void CreateWire()
    {
        currentWire = Instantiate(prefabWire, wireParent);
        wireRenderer = currentWire.GetComponent<LineRenderer>();
        shadowRenderer = currentWire.transform.GetChild(0).GetComponent<LineRenderer>();
    }

    public List<Vector2> GetWirePoints()
    {
        List<Vector2> points = new();
        foreach (Transform child in wireParent)
        {
            LineRenderer lineRenderer = child.GetComponent<LineRenderer>();
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                points.Add(lineRenderer.GetPosition(i));
            }
        }
        return points;
    }

    public void ClearLevel()
    {
        for (int i = 0; i < wireParent.childCount; i++)
        {
            GameObject wire = wireParent.GetChild(i).gameObject;
            Destroy(wire);
        }
        for (int i = 0; i < componentParent.childCount; i++)
        {
            Transform component = componentParent.GetChild(i);
            CircuitComponent script = component.GetComponent<CircuitComponent>();
            script.DetachFromCircuit();

            (script as LightBulb)?.ResetLightBulb();
        }
    }
}
