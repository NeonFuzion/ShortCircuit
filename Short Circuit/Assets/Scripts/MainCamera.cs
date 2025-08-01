using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] float dampening, sizeScaleSpeed;
    [SerializeField] Vector3 offset;
    [SerializeField] Transform player;
    [SerializeField] List<Transform> focusTargets;

    float baseProjectionSize, targetProjectionSize, currentProjectionSize, sizeTime;

    Vector3 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        velocity = Vector2.zero;

        baseProjectionSize = Camera.main.orthographicSize;
        currentProjectionSize = baseProjectionSize;
        targetProjectionSize = baseProjectionSize;
        sizeTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        FollowTargets();
        ScaleProjectionSize();
    }

    private void FollowTargets()
    {
        float yAvg = 0;
        float xAvg = 0;
        focusTargets.ForEach(obj => { yAvg += obj.position.y; xAvg += obj.position.x; });
        Vector3 movePos = new Vector3(xAvg, yAvg) / focusTargets.Count + offset;
        transform.position = Vector3.SmoothDamp(transform.position, movePos, ref velocity, dampening);
    }

    private void ScaleProjectionSize()
    {
        if (currentProjectionSize == targetProjectionSize) return;
        sizeTime += Time.deltaTime * sizeScaleSpeed;
        Camera.main.orthographicSize = Mathf.Lerp(currentProjectionSize, targetProjectionSize, sizeTime);

        if (Mathf.Abs(targetProjectionSize - Camera.main.orthographicSize) > 0.1f) return;
        currentProjectionSize = targetProjectionSize;
    }

    public void SetProjectionSize(float newSize)
    {
        targetProjectionSize = newSize;
    }

    public void ResetProjectionSize()
    {
        SetProjectionSize(baseProjectionSize);
    }

    public void ClearFocusTargets()
    {
        focusTargets.Clear();
    }

    public void AddFocusTarget(Transform target)
    {
        if (focusTargets.Contains(target)) return;
        focusTargets.Add(target);
    }
}
