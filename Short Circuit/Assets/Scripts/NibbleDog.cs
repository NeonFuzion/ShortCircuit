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
                Vector3 movement = wanderVector * speed * Time.deltaTime;
                if (movement.sqrMagnitude < ((Vector3)endPosition - transform.position).sqrMagnitude)
                    transform.position += movement;
                else
                    transform.position = endPosition;

                if (Vector2.Distance(transform.position, endPosition) > 0.1f) break;
                dogState = DogState.Idle;
                StartIdle();
                break;
            case DogState.Busy:
                movement = wanderVector * speed * runSpeedMultiplier * Time.deltaTime;
                if (!transform.position.Equals(endPosition) && movement.sqrMagnitude < ((Vector3)endPosition - transform.position).sqrMagnitude)
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
        if (Vector2.Distance(player.position, transform.position) > playerDetectRadius) return;
        Collider2D[] allFound = Physics2D.OverlapCircleAll(transform.position, 40, LayerMask.GetMask("LightBulb"));

        if (allFound.Length == 0) return;
        Transform target = allFound.OrderBy(collider => Vector2.Distance(collider.transform.position, transform.position)).FirstOrDefault().transform;
        wanderVector = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, wanderVector, wanderVector.magnitude, boundaryLayer);

        if (hit) return;
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
