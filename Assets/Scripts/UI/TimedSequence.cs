using System;
using UnityEngine;

public class TimedSequence : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        public GameObject panel;
        public float showAtSeconds = 0f;
        public float durationSeconds = 4f;
    }

    [SerializeField] private Step[] steps;
    [SerializeField] private bool startOnSceneLoad = true;

    float _startTime = -1f;

    void Start()
    {
        foreach (Step s in steps)
        {
            if (s.panel != null) s.panel.SetActive(false);
        }
        if (startOnSceneLoad) _startTime = Time.time;
    }

    void Update()
    {
        if (_startTime < 0f) return;

        float elapsed = Time.time - _startTime;

        foreach (Step s in steps)
        {
            if (s.panel == null) continue;
            bool shouldBeActive = elapsed >= s.showAtSeconds && elapsed < s.showAtSeconds + s.durationSeconds;
            if (s.panel.activeSelf != shouldBeActive)
            {
                s.panel.SetActive(shouldBeActive);
            }
        }
    }
}
