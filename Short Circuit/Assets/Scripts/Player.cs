using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float spinSpeed, spinRange, launchSpeed, minDistance, maxDistance, speed, maxHeight;
    [SerializeField] Transform battery, target, spinner, spinCenter, projectileShadow, projectileVisual;
    [SerializeField] GameObject prefabWirePoint;
    [SerializeField] AnimationCurve trajectoryCurve;

    int index, spinDirection;
    float totalDistance, groundDirection, lastDirection;
    bool active, moving;

    new Rigidbody2D rigidbody;
    BoxCollider2D boxCollider;
    Vector2 startPosition, targetPosition;
    InputMode inputMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        index = 0;
        active = true;

        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        switch (inputMode)
        {
            case InputMode.Spinning:
                float currentAngle = spinner.localEulerAngles.z;
                float newDirection = currentAngle + spinDirection * spinSpeed * Time.deltaTime;
                spinner.localEulerAngles = new(0, 0, Mathf.Clamp(newDirection, -spinRange, spinRange) + 180);

                if (newDirection < spinRange && newDirection > -spinRange) break;
                spinDirection *= -1;
                break;
            case InputMode.Stretching:
                float currentDistance = -target.localPosition.y;
                float newDistance = currentDistance + launchSpeed * Time.deltaTime * spinDirection;
                target.localPosition = new(0, -Mathf.Clamp(newDistance, minDistance, maxDistance));

                if (newDistance < maxDistance && newDistance > minDistance) break;
                spinDirection *= -1;
                break;
            case InputMode.Launching:
                ArcMovement();
                break;
        }
    }

    void Reset()
    {
        lastDirection = spinner.eulerAngles.z;
        spinCenter.localEulerAngles = new(0, 0, lastDirection);
        spinner.localEulerAngles = new();
        target.localPosition = Vector2.down;
        spinDirection = 1;
        inputMode = InputMode.Spinning;
    }

    void ArcMovement()
    {
        totalDistance = Vector2.Distance(startPosition, targetPosition);
        transform.position += (Vector3)(targetPosition - startPosition).normalized * launchSpeed * Time.deltaTime;

        float distanceCovered = Vector2.Distance(transform.position, startPosition);
        float distanceProgress = distanceCovered / totalDistance;
        float trajectoryCurveValue = trajectoryCurve.Evaluate(distanceProgress);
        float projectileHeight = trajectoryCurveValue * maxHeight * totalDistance / 8;
        projectileVisual.transform.localPosition = Vector2.up * projectileHeight;

        Vector2 differenceVector = targetPosition - (Vector2)transform.position;
        float radians = Mathf.Atan2(differenceVector.y, differenceVector.x);
        groundDirection = (radians > 0 ? radians : radians + 2 * Mathf.PI) * 180 / Mathf.PI % 360;

        float trajectoryAngle = (1 - trajectoryCurveValue) * (distanceProgress > 0.5f ? -1 : 1) * maxHeight * 20;
        projectileVisual.transform.eulerAngles = Vector3.forward * (groundDirection + trajectoryAngle);
        projectileShadow.transform.eulerAngles = Vector3.forward * groundDirection;

        if (distanceProgress < 1) return;
        Reset();
    }

    public void Shrink()
    {
        boxCollider.isTrigger = true;
        transform.position = battery.position;
        active = false;
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (inputMode != InputMode.Spinning) return;
            inputMode = InputMode.Stretching;
        }
        if (context.canceled)
        {
            if (inputMode != InputMode.Stretching) return;
            inputMode = InputMode.Launching;
            startPosition = transform.position;
            targetPosition = startPosition + (Vector2)target.position;
        }
    }

    public void HandleShrink(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Shrink();
    }
    
    public enum InputMode { None, Spinning, Stretching, Launching }
}
