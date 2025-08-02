using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] float dampening, sizeScaleSpeed, baseProjectionSize;
    [SerializeField] Vector3 offset;
    [SerializeField] List<Transform> focusTargets;

    float targetProjectionSize, currentProjectionSize, sizeTime;

    Vector3 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        velocity = Vector2.zero;

        if (baseProjectionSize == 0) baseProjectionSize = Camera.main.orthographicSize;
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
        //Debug.Log($"{Camera.main.orthographicSize}, {currentProjectionSize}, {targetProjectionSize}");
        Camera.main.orthographicSize = Mathf.Lerp(currentProjectionSize, targetProjectionSize, sizeTime);

        if (Mathf.Abs(targetProjectionSize - Camera.main.orthographicSize) > 0.1f) return;
        currentProjectionSize = targetProjectionSize;
        Camera.main.orthographicSize = currentProjectionSize;
    }

    public void SetProjectionSize(float newSize)
    {
        targetProjectionSize = newSize;
        currentProjectionSize = Camera.main.orthographicSize;
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
