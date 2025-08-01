using System.Collections;
using UnityEngine;

public class Dog : MonoBehaviour
{
    [SerializeField] float idleMin, idleMax, wanderMin, wanderMax, speed;

    DogState dogState;

    Vector2 startPosition, endPosition, wanderVector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dogState = DogState.Idle;
        StartCoroutine(IdleCoroutine());
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
        yield return new WaitForSeconds(Random.value * (idleMax - idleMin) + idleMin);
        SetWanderDirection();
        dogState = DogState.Wandering;
    }

    void SetWanderDirection()
    {
        float angle = Random.value * 2 * Mathf.PI;
        float distance = Random.Range(wanderMin, wanderMax);
        wanderVector = new(Mathf.Cos(angle), Mathf.Sin(angle));
        startPosition = transform.position;
        endPosition = startPosition + wanderVector * distance;
    }
    
    public enum DogState { None, Idle, Wandering }
}
