using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float spinSpeed, launchSpeed, minDistance, maxDistance, speed, maxHeight, wirePointCooldown;
    [SerializeField] Transform target, spinner, bum, projectileShadow, projectileVisual;
    [SerializeField] Sprite aimableSprite, unAimableSprite;
    [SerializeField] AnimationCurve trajectoryCurve;
    [SerializeField] LevelManager levelManager;
    [SerializeField] UnityEvent onEndGame, onResetToWire, onLaunch;
    [SerializeField] UnityEvent<Transform> onStartGame;

    float totalDistance, groundDirection, currentAngle, currentDistance, currentWiringTime;
    bool shrinking, starting, foundBattery;
    int max, lastWireIndex;

    Vector2 startPosition, targetPosition, directionVector, input, spawnPosition;
    Vector3 newPosition;
    Transform levelTarget;
    Battery battery;
    Sprite oldCursor;
    PlayerState playerState;
    SpriteRenderer aimRenderer, mubRenderer;
    Animator animator;
    LineRenderer wireRenderer, shadowRenderer;
    List<CircuitComponent> attachedComponents;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shrinking = false;
        starting = true;

        animator = GetComponent<Animator>();
        aimRenderer = target.GetChild(0).GetComponent<SpriteRenderer>();
        mubRenderer = projectileVisual.GetChild(0).GetComponent<SpriteRenderer>();

        StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        switch (playerState)
        {
            case PlayerState.Controlling:
                currentAngle = (currentAngle - input.x * spinSpeed * Time.deltaTime) % 360;
                currentDistance = Mathf.Clamp(currentDistance + input.y * launchSpeed * Time.deltaTime, minDistance, maxDistance);

                float radians = currentAngle * Mathf.PI / 180;
                directionVector = new(Mathf.Cos(radians), Mathf.Sin(radians));
                target.localPosition = directionVector * currentDistance;
                projectileVisual.eulerAngles = new(0, 0, currentAngle);
                mubRenderer.flipY = directionVector.x < 0;

                if (IsPluggable())
                {
                    aimRenderer.sprite = aimableSprite;
                    animator.CrossFade("Selectable", 0, 0);
                }
                else
                {
                    aimRenderer.sprite = unAimableSprite;
                    animator.CrossFade("NonSelectable", 0, 0);
                }
        
                if (!shrinking) break;
                EndGame();
                break;
            case PlayerState.Launching:
                ArcMovement();
                target.position = targetPosition;
                break;
        }
    }

    bool IsPluggable() => !Physics2D.OverlapCircle(target.position, 0.1f, LayerMask.GetMask("Inaccessible"));

    void ResetPlayer()
    {
        currentAngle = Mathf.Atan2(directionVector.y, directionVector.x) * 180 / Mathf.PI;
        spinner.localEulerAngles = Vector3.zero;
        target.localPosition = Vector2.down;
        playerState = PlayerState.Controlling;
    }

    void DetectAfterLanding()
    {
        WireHandle(0);
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, 0.6f))
        {
            if (LayerMask.LayerToName(collider.gameObject.layer).Equals("Danger"))
            {
                onResetToWire?.Invoke();
                if (shadowRenderer.positionCount >= 2) shadowRenderer.positionCount -= 2;
                else shadowRenderer.positionCount = 0;
                wireRenderer.positionCount = lastWireIndex;
                transform.position = startPosition;
            }
            if (collider.GetComponent<CircuitComponent>() is CircuitComponent script)
            {
                if (script as Battery)
                {
                    DetectBattery();
                }
                else
                {
                    if (!attachedComponents.Contains(script)) attachedComponents.Add(script);
                }
            }
        }
    }

    void ConnectBulbs()
    {
        if (newPosition.z < 0) return;
        WireHandle(WiringPhase.ComponentConnecting);
        if (!foundBattery) transform.position = newPosition;
        newPosition = Vector3.back;
        if (attachedComponents.Count > 0) attachedComponents[attachedComponents.Count - 1].AttachToCircuit();
        WireHandle(WiringPhase.Resetting);
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

        float trajectoryAngle = (1 - trajectoryCurveValue) * -Mathf.Sign(distanceProgress / 2) * maxHeight * 20;
        projectileVisual.eulerAngles = Vector3.forward * (trajectoryCurveValue > 0.1f ? (groundDirection + trajectoryAngle) : groundDirection);
        projectileShadow.eulerAngles = Vector3.forward * groundDirection;

        WireHandle(WiringPhase.Jumping);

        if (distanceProgress < 1) return;
        DetectAfterLanding();
        if (wireRenderer) lastWireIndex = wireRenderer.positionCount - 1;
        ResetPlayer();
        ConnectBulbs();

        if (!starting) return;
        starting = false;
        bum.eulerAngles = new(0, 0, currentAngle);
    }

    void WireHandle(WiringPhase phase)
    {
        currentWiringTime -= Time.deltaTime;

        switch (phase)
        {
            case WiringPhase.Resetting:
                LevelParent level = levelManager.CurrentLevel;
                lastWireIndex = 0;
                level.CreateWire();
                wireRenderer = level.WireRenderer;
                shadowRenderer = level.ShadowRenderer;
                wireRenderer.SetPosition(0, projectileVisual.position);
                shadowRenderer.SetPosition(0, projectileShadow.position);
                break;
            case WiringPhase.StartJumping:
                int index = wireRenderer.positionCount++;
                wireRenderer.SetPosition(index, projectileVisual.position);
                index = shadowRenderer.positionCount++;
                shadowRenderer.SetPosition(index, projectileShadow.position);
                break;
            case WiringPhase.Jumping:
                if (currentWiringTime > 0) break;
                currentWiringTime = wirePointCooldown;
                index = wireRenderer.positionCount++;
                wireRenderer.SetPosition(index, projectileVisual.position);
                index = shadowRenderer.positionCount - 1;
                shadowRenderer.SetPosition(index, projectileShadow.position);
                break;
            case WiringPhase.ComponentConnecting:
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
        playerState = PlayerState.Launching;
    }

    void DetectBattery()
    {
        onEndGame?.Invoke();
        projectileVisual.eulerAngles = Vector3.zero;
        playerState = PlayerState.Waiting;
        shrinking = false;
    }

    public void SetShrink()
    {
        shrinking = true;
    }

    public void StartLevel()
    {
        foundBattery = false;
        
        attachedComponents = new();
        spawnPosition = battery.GetBatteryPositions()[0];
        transform.position = spawnPosition;
        bum.position = spawnPosition;
        playerState = PlayerState.Controlling;
        newPosition = Vector3.back;
        onStartGame?.Invoke(levelTarget);

        currentAngle = 0;
        directionVector = Vector2.zero;
        target.localPosition = Vector3.zero;
        spinner.eulerAngles = Vector3.zero;
        targetPosition = transform.position;
        startPosition = transform.position;

        WireHandle(WiringPhase.Resetting);
        ResetPlayer();
    }

    public void Initialize(int max, Transform levelTarget, Battery battery)
    {
        this.max = max;
        this.battery = battery;
        this.levelTarget = levelTarget;
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    public void HandleMousePosition(InputAction.CallbackContext context)
    {
        Vector2 position = context.ReadValue<Vector2>();
        Vector2 deltaPosition = Camera.main.ScreenToWorldPoint(position) - transform.position;
        currentAngle = Mathf.Atan2(deltaPosition.y, deltaPosition.x) * 180 / Mathf.PI;
        currentDistance = deltaPosition.magnitude;
    }

    public void HandleState(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (playerState != PlayerState.Controlling) return;
        if (!IsPluggable()) return;
        directionVector = target.localPosition;
        currentDistance = minDistance;
        currentAngle = 0;

        startPosition = transform.position;
        bool found = false;
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(target.position, 0.6f))
        {
            CircuitComponent script = collider.GetComponent<CircuitComponent>();

            if (attachedComponents.Contains(script)) continue;
            if (!script) continue;
            targetPosition = script.GetNearestPosition(target.position);
            newPosition = script.GetFurtherPosition(target.position);
            found = true;

            if (!(script as Battery)) break;
            if (Vector2.Distance(targetPosition, spawnPosition) < 0.01f)
                targetPosition = newPosition;
            foundBattery = true;
            break;
        } 
        if (!found) targetPosition = startPosition + directionVector * currentDistance;

        WireHandle(0);
        onLaunch?.Invoke();
        playerState = PlayerState.Launching;
    }
    
    enum PlayerState { None, Controlling, Launching, Waiting }
    enum WiringPhase { None, Resetting, StartJumping, Jumping, ComponentConnecting }
}
