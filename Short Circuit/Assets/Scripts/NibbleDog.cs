using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public class NibbleDog : MonoBehaviour
{
    [SerializeField] float idleMin, idleMax, wanderMin, wanderMax, speed, runSpeedMultiplier, playerDetectRadius;
    [SerializeField] UnityEvent onBreakCircuit;

    DogState dogState;
    Vector2 startPosition, endPosition, wanderVector;
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        StartDogging();
    }

    // Update is called once per frame
    void Update()
    {
        switch (dogState)
        {
            case DogState.Idle:
                DetectPlayer();
                break;
            case DogState.Wandering:
                DetectPlayer();
                Vector3 movement = wanderVector * speed * Time.deltaTime;
                if (movement.sqrMagnitude < ((Vector3)endPosition - transform.position).sqrMagnitude)
                    transform.position += movement;
                else
                    transform.position = endPosition;

                if (Vector2.Distance(transform.position, endPosition) > 0.1f) break;
                dogState = DogState.Idle;
                StartCoroutine(IdleCoroutine());
                break;
            case DogState.Running:
                movement = wanderVector * speed * runSpeedMultiplier * Time.deltaTime;
                if (movement.sqrMagnitude < ((Vector3)endPosition - transform.position).sqrMagnitude)
                    transform.position += movement;
                else
                    transform.position = endPosition;

                if (Vector2.Distance(transform.position, endPosition) > 0.1f) break;
                dogState = DogState.Biting;
                animator.CrossFade("Bite", 0, 0);
                break;
        }
    }

    IEnumerator IdleCoroutine()
    {
        animator.CrossFade("Idle", 0, 0);
        yield return new WaitForSeconds(Random.value * (idleMax - idleMin) + idleMin);
        SetWanderDirection();
        dogState = DogState.Wandering;
    }

    void SetWanderDirection()
    {
        float angle = Random.value * 2 * Mathf.PI;
        float distance = Random.Range(wanderMin, wanderMax);

        while (true)
        {
            wanderVector = new(Mathf.Cos(angle), Mathf.Sin(angle));
            RaycastHit2D hit = Physics2D.Raycast(transform.position, wanderVector, distance, LayerMask.GetMask("Unpluggable"));

            if (!hit) break;
            distance = Mathf.Clamp(Vector2.Distance(transform.position, hit.point) - 1, 0, wanderMax);

            if (distance > 1) break;
        }

        startPosition = transform.position;
        endPosition = startPosition + wanderVector * distance;
        transform.GetChild(0).localScale = new(wanderVector.x > 0 ? 1 : -1, transform.localScale.y, transform.localScale.z);
        animator.CrossFade("Walk", 0, 0);
    }

    void DetectPlayer()
    {
        bool found = false;
        foreach (Collider2D collision in Physics2D.OverlapCircleAll(transform.position, playerDetectRadius))
        {
            Player player = collision.GetComponent<Player>();

            if (!player) continue;
            if (!player.Active) return;
            found = true;
            break;
        }

        if (!found) return;
        float shortestDistance = 0;
        Transform target = null;
        foreach (Collider2D collision in Physics2D.OverlapCircleAll(transform.position, 40, LayerMask.GetMask("LightBulb")))
        {
            float distance = Vector2.Distance(collision.transform.position, transform.position);

            if (distance > shortestDistance && shortestDistance != 0) continue;
            shortestDistance = distance;
            target = collision.transform;
        }

        if (!target) return;
        wanderVector = (target.position - transform.position).normalized;
        startPosition = transform.position;
        endPosition = target.position;
        dogState = DogState.Running;
    }

    void Bite()
    {
        onBreakCircuit?.Invoke();

        Collider2D collider = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("LightBulb"));

        if (!collider) return;
        LightBulb lightBulb = collider.GetComponent<LightBulb>();

        if (!lightBulb) return;
        lightBulb.BreakBulb();
    }

    void StartDogging()
    {
        if (Random.value > 0.5f)
        {
            dogState = DogState.Idle;
            StartCoroutine(IdleCoroutine());
        }
        else
        {
            dogState = DogState.Wandering;
            SetWanderDirection();
        }
    }
    
    public enum DogState { None, Idle, Wandering, Running, Biting }
}
