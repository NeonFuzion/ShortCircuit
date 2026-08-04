using UnityEngine;

public class Battery : CircuitComponent
{
    [SerializeField] bool rightSideRespawn;

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
