using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float spinSpeed, spinRange, launchSpeed, minDistance, maxDistance, speed, maxHeight;
    [SerializeField] Transform target, spinner, bum, projectileShadow, projectileVisual;
    [SerializeField] GameObject prefabWire;
    [SerializeField] Battery battery;
    [SerializeField] Sprite aimableSprite, unAimableSprite;
    [SerializeField] AnimationCurve trajectoryCurve;
    [SerializeField] UnityEvent<List<LightBulb>> onEndGame;

    float totalDistance, groundDirection, lastDirection, currentAngle, currentDistance;
    bool active, shrinking, starting;

    new Rigidbody2D rigidbody;
    BoxCollider2D boxCollider;
    Vector2 startPosition, targetPosition, directionVector, input, spawnPosition;
    Vector3 newPosition;
    Wire currentWire;
    InputMode inputMode;
    SpriteRenderer aimRenderer;
    List<LightBulb> lightBulbs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        active = true;
        shrinking = false;
        starting = true;

        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        aimRenderer = target.GetComponent<SpriteRenderer>();
        lightBulbs = new();
        spawnPosition = battery.GetBatteryPositions()[0];
        transform.position = spawnPosition;
        bum.position = spawnPosition;
        inputMode = InputMode.Controlling;
        newPosition = Vector3.back;

        Reset();
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
                projectileVisual.eulerAngles = new(0, 0, currentAngle + lastDirection);
                projectileVisual.localScale = new(1, directionVector.x > 0 ? 1 : -1, 1);

                aimRenderer.sprite = IsPluggable() ? aimableSprite : unAimableSprite;
                break;
            case InputMode.Launching:
                ArcMovement();
                target.position = targetPosition;
                break;
        }
    }

    bool IsPluggable() => !Physics2D.OverlapCircle(target.position, 0.1f, LayerMask.GetMask("Unpluggable"));

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

    Vector2[] DetectComponentAtTarget()
    {
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(target.position, 0.6f))
        {
            Component script = collider.GetComponent<Component>();

            if (lightBulbs.Contains(script)) continue;
            if (!script) continue;
            return script.GetNearestPosition(transform.position);
        }
        return new Vector2[0];
    }

    void DetectComponent()
    {
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, 0.6f))
        {
            Component script = collider.GetComponent<Component>();

            if (lightBulbs.Contains(script)) continue;
            if (!script) continue;
            if (script as LightBulb) lightBulbs.Add(script as LightBulb);
            else if (script as Battery) DetectBattery();
        }
    }

    void ConnectBulbs()
    {
        if (newPosition.z < 0) return;
        if (active) transform.position = newPosition;
        newPosition = Vector3.back;
        lightBulbs[lightBulbs.Count - 1].AttachToCircuit();
    }

    void ArcMovement()
    {
        totalDistance = Vector2.Distance(startPosition, targetPosition);
        Vector3 currentMovement = (Vector3)(targetPosition - startPosition).normalized * speed * Time.deltaTime;
        transform.position += currentMovement;

        float distanceCovered = Vector2.Distance(transform.position, startPosition);
        float distanceProgress = Mathf.Clamp(distanceCovered / totalDistance, 0, 1);
        float trajectoryCurveValue = trajectoryCurve.Evaluate(distanceProgress);
        float projectileHeight = trajectoryCurveValue * maxHeight * totalDistance / 8;
        projectileVisual.localPosition = Vector2.up * projectileHeight;

        Vector2 differenceVector = targetPosition - (Vector2)transform.position;
        float radians = Mathf.Atan2(differenceVector.y, differenceVector.x);
        groundDirection = (radians > 0 ? radians : radians + 2 * Mathf.PI) * 180 / Mathf.PI % 360;

        float trajectoryAngle = (1 - trajectoryCurveValue) * (distanceProgress > 0.5f ? -1 : 1) * maxHeight * 20;
        projectileVisual.eulerAngles = Vector3.forward * (trajectoryCurveValue > 0.1f ? (groundDirection + trajectoryAngle) : groundDirection);
        projectileShadow.eulerAngles = Vector3.forward * groundDirection;

        bum.position = spawnPosition;

        if (distanceProgress < 1) return;
        Reset();
        DetectComponent();
        ConnectBulbs();

        if (!shrinking) return;
        EndGame();

        if (!starting) return;
        starting = false;
        bum.eulerAngles = new(0, 0, currentAngle);
    }

    void EndGame()
    {
        SpawnWire();
        startPosition = transform.position;
        targetPosition = battery.GetBatteryPositions()[1];
        inputMode = InputMode.Launching;
    }

    void DetectBattery()
    {
        onEndGame?.Invoke(lightBulbs);
        projectileVisual.eulerAngles = new();
        active = false;
        enabled = false;
    }

    void SpawnWire()
    {
        GameObject wire = Instantiate(prefabWire, transform.position, Quaternion.identity);
        currentWire = wire.GetComponent<Wire>();
        currentWire.StartWiring(projectileVisual, projectileShadow);
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
        if (!IsPluggable()) return;
        directionVector = target.localPosition;
        currentDistance = minDistance;
        currentAngle = 0;

        startPosition = transform.position;
        Vector2[] bulbPositions = DetectComponentAtTarget();
        if (bulbPositions.Length > 0)
        {
            targetPosition = bulbPositions[0];
            newPosition = bulbPositions[1];
        }
        else
        {
            targetPosition = startPosition + directionVector * currentDistance;
        }

        inputMode = InputMode.Launching;
        SpawnWire();
    }

    public void HandleShrink(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        EndGame();
    }
    
    public enum InputMode { None, Controlling, Launching }
}
