using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelParent : MonoBehaviour
{
    [SerializeField] float time;
    [SerializeField] Transform componentParent, wireParent;
    [SerializeField] GameObject prefabWire;
    [SerializeField] Battery battery;

    CircuitComponent[] circuitComponents;
    Dog[] dogs;
    LineRenderer wireRenderer, shadowRenderer;
    GameObject currentWire;

    public float Time { get => time; }

    public Battery Battery { get => battery; }
    public LineRenderer WireRenderer { get => wireRenderer; }
    public LineRenderer ShadowRenderer { get => shadowRenderer; }
    public CircuitComponent[] CircuitComponents { get => circuitComponents; }

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
        circuitComponents = GetComponentsInChildren<CircuitComponent>().Where(component => !(component as Battery)).ToArray();
        dogs = GetComponentsInChildren<Dog>();
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
        wireParent.GetComponentsInChildren<LineRenderer>().ToList().ForEach(wire => Destroy(wire.gameObject));
        circuitComponents.ToList().ForEach(component =>
        {
            component.DetachFromCircuit();
            component.ResetComponent();
        });
        dogs.ToList().ForEach(dog => dog.ResetDog());
    }
}
