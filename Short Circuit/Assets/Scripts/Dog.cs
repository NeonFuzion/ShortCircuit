using System.Collections;
using UnityEngine;

public class Dog : MonoBehaviour
{
    [SerializeField] float idleMin, idleMax, wanderMin, wanderMax, speed;

    DogState dogState;
    Vector2 startPosition, endPosition, wanderVector;
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

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

    // Update is called once per frame
    void Update()
    {
        switch (dogState)
        {
            case DogState.Wandering:
                Vector3 movement = wanderVector * speed * Time.deltaTime;
                if (movement.sqrMagnitude < ((Vector3)endPosition - transform.position).sqrMagnitude)
                    transform.position += movement;
                else
                    transform.position = endPosition;

                if (Vector2.Distance(transform.position, endPosition) > 0.1f) break;
                dogState = DogState.Idle;
                StartCoroutine(IdleCoroutine());
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
    
    public enum DogState { None, Idle, Wandering }
}
