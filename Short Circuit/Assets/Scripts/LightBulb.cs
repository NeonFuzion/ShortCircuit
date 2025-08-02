using UnityEngine;

public class LightBulb : CircuitComponent
{
    [SerializeField] Sprite litSprite, unLitSprite, brokenSprite;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void ResetComponent()
    {
        base.ResetComponent();
        spriteRenderer.sprite = unLitSprite;
    }

    public void PowerBulb()
    {
        spriteRenderer.sprite = litSprite;
    }

    public void BreakBulb()
    {
        spriteRenderer.sprite = brokenSprite;
    }
}
