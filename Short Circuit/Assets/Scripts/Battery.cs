using UnityEngine;

public class Battery : Component
{
    [SerializeField] bool rightSideRespawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector2[] GetBatteryPositions()
    {
        return rightSideRespawn ?
            new Vector2[2] { firstTarget.position, secondTarget.position } :
            new Vector2[2] { secondTarget.position, firstTarget.position };
    }
}
