using UnityEngine;

public class WanderDog : Dog
{
    protected override void HandleDogState()
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
                StartIdle();
                break;
        }
    }
}
