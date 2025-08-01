using UnityEngine;

public class CircuitComponent : MonoBehaviour
{
    [SerializeField] protected Transform positiveTarget, negativeTarget;

    protected bool attached;

    protected SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        attached = false;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (attached) return;
        transform.localScale = Vector3.one * 1.2f;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (attached) return;
        transform.localScale = Vector3.one;
    }

    public void AttachToCircuit()
    {
        attached = true;
        transform.localScale = Vector3.one * 1.2f;
    }

    public void DetachFromCircuit()
    {
        attached = false;
        transform.localScale = Vector3.one;
    }

    public Vector2[] GetNearestPosition(Vector2 position)
    {
        float firstDistance = Vector2.Distance(positiveTarget.position, position);
        float secondDistance = Vector2.Distance(negativeTarget.position, position);
        return firstDistance > secondDistance ? new Vector2[2] { negativeTarget.position, positiveTarget.position } :
            new Vector2[2] { positiveTarget.position, negativeTarget.position };
    }
}
