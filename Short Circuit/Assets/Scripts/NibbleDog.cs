using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class NibbleDog : Dog
{
    [SerializeField] float runSpeedMultiplier, playerDetectRadius, nibbleCooldown;
    [SerializeField] UnityEvent onBreakCircuit, onDetectPlayer;

    Transform player;
    LightBulb targetBulb;

    protected override void HandleDogState()
    {
        switch (dogState)
        {
            case DogState.Idle:
                DetectPlayer();
                break;
            case DogState.Wandering:
                DetectPlayer();
                Vector3 movement = wanderVector.normalized * speed * Time.deltaTime;
                if (movement.sqrMagnitude < ((Vector3)endPosition - transform.position).sqrMagnitude)
                    transform.position += movement;
                else
                    transform.position = endPosition;

                if (Vector2.Distance(transform.position, endPosition) > 0.1f) break;
                dogState = DogState.Idle;
                StartIdle();
                break;
            case DogState.Busy:
                movement = wanderVector.normalized * speed * runSpeedMultiplier * Time.deltaTime;
                if (movement.magnitude < Vector2.Distance(transform.position, endPosition))
                {
                    transform.position += movement;
                }
                else
                {
                    transform.position = endPosition;
                    animator.CrossFade("Bite", 0, 0);
                }
                break;
        }
    }

    void DetectPlayer()
    {
        if (!player)
        {
            Collider2D collision = Physics2D.OverlapCircle(transform.position, playerDetectRadius, LayerMask.GetMask("Player"));
            if (collision) player = collision.transform;
        }

        if (!player) return;
        Vector2 playerDistanceVector = player.position - transform.position;

        if (playerDistanceVector.magnitude > playerDetectRadius) return;
        if (Physics2D.Raycast(transform.position, playerDistanceVector, playerDistanceVector.magnitude, LayerMask.GetMask("Ground"))) return;
        Collider2D[] allFound = Physics2D.OverlapCircleAll(transform.position, 40, LayerMask.GetMask("LightBulb"));

        if (allFound.Length == 0) return;
        allFound = allFound.Where(target => {
                Vector2 lightbulbDistanceVector = target.transform.position - transform.position;
                return !Physics2D.Raycast(transform.position, lightbulbDistanceVector, lightbulbDistanceVector.magnitude, boundaryLayer);
            }).ToArray();

        if (allFound.Length == 0) return;
        Transform target = allFound
            .OrderBy(collider => Vector2.Distance(collider.transform.position, transform.position))
            .FirstOrDefault().transform;
        onDetectPlayer?.Invoke();
        endPosition = target.position;
        dogState = DogState.Busy;
        targetBulb = target.GetComponent<LightBulb>();
        StopAllCoroutines();
    }

    void Bite()
    {
        ScoreKeeper.Instance.FailLevel();
        targetBulb.BreakBulb();
        onBreakCircuit?.Invoke();
        dogState = DogState.None;
    }
}
