using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float spinSpeed, spinRange, launchSpeed, minDistance, maxDistance, speed, maxHeight;
    [SerializeField] Transform battery, target, spinner, projectileShadow, projectileVisual;
    [SerializeField] GameObject prefabWire;
    [SerializeField] AnimationCurve trajectoryCurve;

    float totalDistance, groundDirection, lastDirection, currentAngle, currentDistance;
    bool active, shrinking;

    new Rigidbody2D rigidbody;
    BoxCollider2D boxCollider;
    Vector2 startPosition, targetPosition, directionVector, input;
    Wire currentWire;
    InputMode inputMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        active = true;
        shrinking = false;

        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        transform.position = battery.position;
        Reset();
        inputMode = InputMode.Controlling;
    }

    // Update is called once per frame
    void Update()
    {
        switch (inputMode)
        {
            case InputMode.Controlling:
                currentAngle = Mathf.Clamp(currentAngle - input.x * spinSpeed * Time.deltaTime, -spinRange, spinRange);
                float radians = (currentAngle + lastDirection) * Mathf.PI / 180;
                directionVector = new(Mathf.Cos(radians), Mathf.Sin(radians));

                currentDistance = Mathf.Clamp(currentDistance + input.y * launchSpeed * Time.deltaTime, minDistance, maxDistance);
                target.localPosition = directionVector * currentDistance;
                break;
            case InputMode.Launching:
                ArcMovement();
                target.position = targetPosition;
                break;

        }
    }

    void Reset()
    {
        lastDirection = Mathf.Atan2(directionVector.y, directionVector.x) * 180 / Mathf.PI;
        spinner.localEulerAngles = new();
        target.localPosition = Vector2.down;
        inputMode = InputMode.Controlling;

        if (!currentWire) return;
        currentWire.EndWiring();
        currentWire = null;
    }

    void DetectBulbs()
    {
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, 0.6f))
        {
            LightBulb script = collider.GetComponent<LightBulb>();

            if (!script) continue;
            script.PowerBulb();
        }
    }

    void ArcMovement()
    {
        totalDistance = Vector2.Distance(startPosition, targetPosition);
        transform.position += (Vector3)(targetPosition - startPosition).normalized * launchSpeed * Time.deltaTime;

        float distanceCovered = Vector2.Distance(transform.position, startPosition);
        float distanceProgress = distanceCovered / totalDistance;
        float trajectoryCurveValue = trajectoryCurve.Evaluate(distanceProgress);
        float projectileHeight = trajectoryCurveValue * maxHeight * totalDistance / 8;
        projectileVisual.localPosition = Vector2.up * projectileHeight;

        Vector2 differenceVector = targetPosition - (Vector2)transform.position;
        float radians = Mathf.Atan2(differenceVector.y, differenceVector.x);
        groundDirection = (radians > 0 ? radians : radians + 2 * Mathf.PI) * 180 / Mathf.PI % 360;

        float trajectoryAngle = (1 - trajectoryCurveValue) * (distanceProgress > 0.5f ? -1 : 1) * maxHeight * 20;
        projectileVisual.eulerAngles = Vector3.forward * (groundDirection + trajectoryAngle);
        projectileShadow.eulerAngles = Vector3.forward * groundDirection;

        if (distanceProgress < 1) return;
        Reset();
        DetectBulbs();

        if (!shrinking) return;
        Shrink();
    }

    void Shrink()
    {
        SpawnWire();
        startPosition = transform.position;
        targetPosition = battery.position;
        inputMode = InputMode.Launching;
    }

    void SpawnWire()
    {
        GameObject wire = Instantiate(prefabWire, transform.position, Quaternion.identity);
        currentWire = wire.GetComponent<Wire>();
        currentWire.StartWiring(projectileVisual);
    }

    public void SetShrink()
    {
        shrinking = true;
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    public void HandleState(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (inputMode != InputMode.Controlling) return;
        directionVector = target.localPosition;
        currentDistance = minDistance;
        currentAngle = 0;
        startPosition = transform.position;
        targetPosition = startPosition + directionVector * currentDistance;
        inputMode = InputMode.Launching;
        SpawnWire();
    }

    public void HandleShrink(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Shrink();
    }
    
    public enum InputMode { None, Controlling, Launching }
}
