using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [SerializeField] Transform battery;

    LineRenderer lineRenderer;
    PolygonCollider2D polygonCollider;

    List<Transform> positions;
    Vector2[] points;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();

        positions = new() { battery };
    }

    // Update is called once per frame
    void Update()
    {
        if (lineRenderer.positionCount < 3) return;
        points = new Vector2[lineRenderer.positionCount];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = lineRenderer.GetPosition(i);
        }
        polygonCollider.points = points;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Player>()) return;
        if (!LayerMask.LayerToName(other.gameObject.layer).Equals("LightBulb")) return;
        if (positions.Contains(other.transform)) return;
        positions.Add(other.transform);
    }

    public void Loop()
    {
        positions.Add(positions[0]);
        lineRenderer.SetPositions(positions.Select(x => x.position).ToArray());
    }
}
