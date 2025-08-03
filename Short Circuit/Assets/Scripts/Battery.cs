using UnityEngine;

public class Battery : CircuitComponent
{
    [SerializeField] bool rightSideRespawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector2[] GetBatteryPositions()
    {
        return rightSideRespawn ?
            new Vector2[2] { positiveTarget.position, negativeTarget.position } :
            new Vector2[2] { negativeTarget.position, positiveTarget.position };
    }
}
