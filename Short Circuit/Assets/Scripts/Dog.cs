using System.Collections;
using UnityEngine;

public abstract class Dog : MonoBehaviour
{
    [SerializeField] protected float idleMin, idleMax, wanderMin, wanderMax, speed;

    protected Animator animator;
    protected DogState dogState;
    protected LayerMask boundaryLayer;
    protected Vector2 endPosition, wanderVector, resetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        animator = GetComponent<Animator>();
        boundaryLayer = LayerMask.GetMask("Inaccessible") + LayerMask.GetMask("DogPen");
        resetPosition = transform.position;

        if (!gameObject.activeInHierarchy) return;
        StartDogging();
    }

    // Update is called once per frame
    protected void Update()
    {
        HandleDogState();
    }

    IEnumerator IdleCoroutine()
    {
        animator.CrossFade("Idle", 0, 0);
        yield return new WaitForSeconds(Random.Range(idleMin, idleMax));
        SetWanderDirection();
        dogState = DogState.Wandering;
    }

    protected void StartIdle()
    {
        StartCoroutine(IdleCoroutine());
    }

    protected void StartDogging()
    {
        if (Random.value > 0.5f)
        {
            dogState = DogState.Idle;
            StartIdle();
        }
        else
        {
            dogState = DogState.Wandering;
            SetWanderDirection();
        }
    }

    protected void SetWanderDirection()
    {
        float angle = Random.value * 2 * Mathf.PI;
        float distance = Random.Range(wanderMin, wanderMax);

        wanderVector = new(Mathf.Cos(angle), Mathf.Sin(angle));
        if (Physics2D.Raycast(transform.position, wanderVector, distance, boundaryLayer) is RaycastHit2D hit)
        {
            distance = Mathf.Clamp(Vector2.Distance(transform.position, hit.point) - 1, 0, distance);
        }

        endPosition = (Vector2)transform.position + wanderVector * distance;
        transform.GetChild(0).localScale = new(Mathf.Sign(wanderVector.x), transform.localScale.y, transform.localScale.z);
        animator.CrossFade("Walk", 0, 0);
    }

    protected abstract void HandleDogState();

    public void ResetDog()
    {
        transform.position = resetPosition;
        StartDogging();
    }

    protected enum DogState { None, Idle, Wandering, Busy }
}