using UnityEngine;

public class CircuitComponent : MonoBehaviour
{
    [SerializeField] protected Transform firstTarget, secondTarget;

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

    public Vector2[] GetNearestPosition(Vector2 position)
    {
        float firstDistance = Vector2.Distance(firstTarget.position, position);
        float secondDistance = Vector2.Distance(secondTarget.position, position);
        return firstDistance > secondDistance ? new Vector2[2] { secondTarget.position, firstTarget.position } :
            new Vector2[2] { firstTarget.position, secondTarget.position };
    }
}
