using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float spinSpeed, spinRange, launchSpeed, minDistance, maxDistance, speed, maxHeight, wirePointCooldown;
    [SerializeField] Transform target, spinner, bum, projectileShadow, projectileVisual;
    [SerializeField] Sprite aimableSprite, unAimableSprite;
    [SerializeField] AnimationCurve trajectoryCurve;
    [SerializeField] LevelManager levelManager;
    [SerializeField] UnityEvent onEndGame;
    [SerializeField] UnityEvent<Transform> onStartGame;

    float totalDistance, groundDirection, lastDirection, currentAngle, currentDistance, currentWiringTime;
    bool active, shrinking, starting, foundBattery;
    int max;

    Vector2 startPosition, targetPosition, directionVector, input, spawnPosition;
    Vector3 newPosition;
    Transform levelTarget;
    Battery battery;
    InputMode inputMode;
    SpriteRenderer aimRenderer;
    LineRenderer wireRenderer, shadowRenderer;
    List<LightBulb> lightBulbs;

    public bool Active { get => active; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shrinking = false;
        starting = true;

        aimRenderer = target.GetComponent<SpriteRenderer>();
        StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        if (!active) return;

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
        
                if (!shrinking) break;
                EndGame();
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
    }

    void DetectAfterLanding()
    {
        WireHandle(0);
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, 0.6f))
        {
            if (LayerMask.LayerToName(collider.gameObject.layer).Equals("Danger"))
            {
                transform.position = startPosition;
            }

            CircuitComponent script = collider.GetComponent<CircuitComponent>();
            if (script)
            {
                if (!lightBulbs.Contains(script) && script as LightBulb) lightBulbs.Add(script as LightBulb);
                else if ((script as Battery && max == lightBulbs.Count) || shrinking) DetectBattery();
            }
        }
    }

    void ConnectBulbs()
    {
        if (newPosition.z < 0) return;
        if (!active) return;
        WireHandle(2);
        if (!foundBattery) transform.position = newPosition;
        newPosition = Vector3.back;
        if (lightBulbs.Count > 0) lightBulbs[lightBulbs.Count - 1].AttachToCircuit();
        WireHandle(-1);
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

        WireHandle(1);

        if (distanceProgress < 1) return;
        DetectAfterLanding();
        Reset();
        ConnectBulbs();

        if (!starting) return;
        starting = false;
        bum.eulerAngles = new(0, 0, currentAngle);
    }

    void WireHandle(int phase)
    {
        currentWiringTime -= Time.deltaTime;

        switch (phase)
        {
            case -1:
                LevelParent level = levelManager.CurrentLevel;
                level.CreateWire();
                wireRenderer = level.WireRenderer;
                shadowRenderer = level.ShadowRenderer;
                wireRenderer.SetPosition(0, projectileVisual.position);
                shadowRenderer.SetPosition(0, projectileShadow.position);
                break;
            case 0:
                int index = wireRenderer.positionCount++;
                wireRenderer.SetPosition(index, projectileVisual.position);
                index = shadowRenderer.positionCount++;
                shadowRenderer.SetPosition(index, projectileShadow.position);
                break;
            case 1:
                if (currentWiringTime > 0) break;
                currentWiringTime = wirePointCooldown;
                index = wireRenderer.positionCount++;
                wireRenderer.SetPosition(index, projectileVisual.position);
                index = shadowRenderer.positionCount - 1;
                shadowRenderer.SetPosition(index, projectileShadow.position);
                break;
            case 2:
                index = wireRenderer.positionCount++;
                wireRenderer.SetPosition(index, projectileVisual.position);
                index = shadowRenderer.positionCount++;
                shadowRenderer.SetPosition(index, projectileShadow.position);
                wireRenderer = null;
                shadowRenderer = null;
                break;
        }
    }

    void EndGame()
    {
        startPosition = transform.position;
        targetPosition = battery.GetBatteryPositions()[1];
        inputMode = InputMode.Launching;
    }

    void DetectBattery()
    {
        onEndGame?.Invoke();
        projectileVisual.eulerAngles = new();
        active = false;
        shrinking = false;
    }

    public void SetShrink()
    {
        shrinking = true;
    }

    public void StartLevel()
    {
        active = true;
        foundBattery = false;
        
        lightBulbs = new();
        spawnPosition = battery.GetBatteryPositions()[0];
        transform.position = spawnPosition;
        bum.position = spawnPosition;
        inputMode = InputMode.Controlling;
        newPosition = Vector3.back;
        onStartGame?.Invoke(levelTarget);
        lastDirection = 0;

        WireHandle(-1);
        Reset();
    }

    public void Initialize(int max, Transform levelTarget, Battery battery)
    {
        this.max = max;
        this.battery = battery;
        this.levelTarget = levelTarget;
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        if (!active) return;
        input = context.ReadValue<Vector2>();
    }

    public void HandleState(InputAction.CallbackContext context)
    {
        if (!active) return;
        if (!context.started) return;
        if (inputMode != InputMode.Controlling) return;
        if (!IsPluggable()) return;
        directionVector = target.localPosition;
        currentDistance = minDistance;
        currentAngle = 0;

        startPosition = transform.position;
        bool found = false;
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(target.position, 0.6f))
        {
            CircuitComponent script = collider.GetComponent<CircuitComponent>();

            if (lightBulbs.Contains(script)) continue;
            if (!script) continue;
            targetPosition = script.GetNearestPosition(target.position);
            newPosition = script.GetFurtherPosition(target.position);
            found = true;

            if (!(script as Battery)) break;
            foundBattery = true;
            break;
        } 
        if (!found) targetPosition = startPosition + directionVector * currentDistance;

        WireHandle(0);
        inputMode = InputMode.Launching;
    }

    public void HandleShrink(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        EndGame();
    }
    
    public enum InputMode { None, Controlling, Launching }
}
