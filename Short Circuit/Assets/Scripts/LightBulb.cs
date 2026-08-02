using UnityEngine;

public class LightBulb : CircuitComponent
{
    [SerializeField] Sprite litSprite, unLitSprite, brokenSprite;

    // Update is called once per frame
    void Update()
    {

    }

    public override void ResetComponent()
    {
        base.ResetComponent();
        spriteRenderer.sprite = unLitSprite;
    }

    public override void ActivateComponent()
    {
        animator.CrossFade("Power", 0, 0);
        onPowered?.Invoke();
        spriteRenderer.sprite = litSprite;
    }

    public void BreakBulb()
    {
        spriteRenderer.sprite = brokenSprite;
    }
}
