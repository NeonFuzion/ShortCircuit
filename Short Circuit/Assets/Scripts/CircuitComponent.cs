using UnityEngine;
using UnityEngine.Events;

public class CircuitComponent : MonoBehaviour
{
    [SerializeField] protected bool isPolarized;
    [SerializeField] protected Transform positiveTarget, negativeTarget;
    [SerializeField] protected Sprite unpolarizedSprite;
    [SerializeField] protected UnityEvent onAttached, onPowered;

    protected bool attached, isPassable;

    protected SpriteRenderer spriteRenderer;
    protected Animator animator;

    public bool IsPassable { get => isPassable; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        ResetComponent();

        if (isPolarized) return;
        positiveTarget.GetComponent<SpriteRenderer>().sprite = unpolarizedSprite;
        negativeTarget.GetComponent<SpriteRenderer>().sprite = unpolarizedSprite;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (attached) return;
        Transform target = GetClosestTransform(collision.transform.position);
        target.localScale = Vector3.one * 1.5f;

        if (target == negativeTarget) target = positiveTarget;
        else target = negativeTarget;
        target.localScale = Vector3.one;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (attached) return;
        positiveTarget.localScale = Vector3.one;
        negativeTarget.localScale = Vector3.one;
    }

    void ResumeIdle()
    {
        animator.CrossFade("Idle", 0, 0);
    }

    Transform GetClosestTransform(Vector2 position)
    {
        float positiveDistance = Vector2.Distance(positiveTarget.position, position);
        float negativeDistance = Vector2.Distance(negativeTarget.position, position);
        return positiveDistance > negativeDistance ? negativeTarget : positiveTarget;
    }

    public virtual void ResetComponent()
    {
        attached = false;
        isPassable = !isPolarized;
    }

    public void AttachToCircuit()
    {
        attached = true;
        transform.localScale = Vector3.one * 1.2f;
        negativeTarget.localScale = Vector3.one;
        positiveTarget.localScale = Vector3.one;

        animator.CrossFade("Attach", 0, 0);
        onAttached?.Invoke();
    }

    public void DetachFromCircuit()
    {
        attached = false;
        transform.localScale = Vector3.one;
    }

    public Vector2 GetNearestPosition(Vector2 position)
    {
        Transform output = GetClosestTransform(position);
        if (output == negativeTarget) isPassable = true;
        return output.position;
    }

    public Vector2 GetFurtherPosition(Vector2 position)
    {
        bool check = GetClosestTransform(position) == positiveTarget;
        return check ? negativeTarget.position : positiveTarget.position;
    }
}
