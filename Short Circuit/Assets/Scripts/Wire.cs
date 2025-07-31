using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [SerializeField] float linePointCooldown;
    [SerializeField] Transform mub, bum;
    [SerializeField] Color[] colors;

    Transform playerVisual;
    LineRenderer lineRenderer;

    float currentCooldown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!playerVisual) return;
        currentCooldown -= Time.deltaTime;
        mub.position = playerVisual.position;

        if (currentCooldown > 0) return;
        currentCooldown = linePointCooldown;
        int index = lineRenderer.positionCount++;
        lineRenderer.SetPosition(index, playerVisual.position);
        

        if (lineRenderer.positionCount < 2) return;
        bum.right = lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0);
        int endIndex = lineRenderer.positionCount - 1;
        mub.right = lineRenderer.GetPosition(endIndex) - lineRenderer.GetPosition(endIndex - 1);
    }

    public void StartWiring(Transform playerVisual)
    {
        this.playerVisual = playerVisual;
        lineRenderer = GetComponent<LineRenderer>();

        currentCooldown = 0;
        lineRenderer.SetPosition(0, playerVisual.position);

        Gradient gradient = new();
        GradientColorKey[] colorKeys = new GradientColorKey[1] { new(colors[Random.Range(0, colors.Length)], 0) };
        GradientAlphaKey[] alphaKeys = lineRenderer.colorGradient.alphaKeys;
        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }

    public void EndWiring()
    {
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, playerVisual.position);
        playerVisual = null;
    }
}
