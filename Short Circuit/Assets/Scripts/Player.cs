using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float speed, maxHeight;
    [SerializeField] Transform battery, wireManager, projectileShadow, projectileVisual;
    [SerializeField] GameObject prefabShrinkPoint;
    [SerializeField] AnimationCurve trajectoryCurve;

    int index;
    float totalDistance, groundDirection;
    bool active, moving;

    new Rigidbody2D rigidbody;
    LineRenderer wireLineRenderer;
    BoxCollider2D boxCollider;
    Vector2 startPosition, targetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        index = 0;
        active = true;

        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        wireLineRenderer = wireManager.GetComponent<LineRenderer>();
        transform.position = battery.position;

        PlacePeg();
    }

    // Update is called once per frame
    void Update()
    {
        wireLineRenderer.SetPosition(index, transform.position);
    }

    void PlacePeg()
    {
        wireLineRenderer.positionCount++;
        wireLineRenderer.SetPosition(index++, transform.position);
    }

    void ArcMovement()
    {
        totalDistance = Vector2.Distance(startPosition, targetPosition);
        transform.position += (Vector3)(targetPosition - startPosition).normalized * speed * Time.deltaTime;

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
    }

    public void Shrink()
    {
        boxCollider.isTrigger = true;
        transform.position = battery.position;
        active = false;
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        if (!active) return;
        Vector2 input = context.ReadValue<Vector2>();

        rigidbody.linearVelocity = input * speed;
    }

    public void HandlePlacing(InputAction.CallbackContext context)
    {
        if (!active) return;
        if (!context.started) return;
        PlacePeg();
    }

    public void HandleShrink(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Shrink();
    }
}
