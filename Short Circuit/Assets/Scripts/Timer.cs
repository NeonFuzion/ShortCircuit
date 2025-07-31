using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    [SerializeField] float startTime;
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] UnityEvent onTimeUp;

    float currentTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTime = startTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTime == -1) return;
        tmp.SetText((Mathf.Round(currentTime * 1000) / 1000).ToString());
        currentTime -= Time.deltaTime;

        if (currentTime > 0) return;
        currentTime = -1;
        onTimeUp?.Invoke();
    }
}
