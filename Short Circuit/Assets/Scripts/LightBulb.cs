using UnityEngine;

public class LightBulb : MonoBehaviour
{
    [SerializeField] Sprite litSprite, unLitSprite;

    SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PowerBulb()
    {
        spriteRenderer.sprite = litSprite;
    }
}
