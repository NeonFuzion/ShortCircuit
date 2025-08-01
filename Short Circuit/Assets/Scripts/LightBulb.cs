using UnityEngine;

public class LightBulb : Component
{
    [SerializeField] Sprite litSprite, unLitSprite;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
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
