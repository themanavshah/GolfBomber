using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DestructionLogUI : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;
    [SerializeField] private string header = "Maksad achieved:";
    [SerializeField, TextArea] private string entryFormat = "<b><color=#FFD700>{0}</color></b> <color=white>{1}</color>";
    [SerializeField] private string emptyText = "  (none yet)";

    readonly StringBuilder _sb = new StringBuilder(256);

    void Start()
    {
        if (DestructionTracker.Instance != null)
        {
            DestructionTracker.Instance.OnChanged += Rebuild;
            Rebuild();
        }
        else
        {
            Debug.LogWarning($"{nameof(DestructionLogUI)}: no DestructionTracker in scene.", this);
        }
    }

    void OnDestroy()
    {
        if (DestructionTracker.Instance != null)
        {
            DestructionTracker.Instance.OnChanged -= Rebuild;
        }
    }

    void Rebuild()
    {
        if (logText == null) return;

        _sb.Clear();
        if (!string.IsNullOrEmpty(header)) _sb.AppendLine(header);

        IReadOnlyList<DestructionTracker.Event> events = DestructionTracker.Instance.Events;
        if (events.Count == 0)
        {
            _sb.Append(emptyText);
        }
        else
        {
            foreach (DestructionTracker.Event e in events)
            {
                _sb.AppendLine(string.Format(entryFormat, e.Points, e.Type));
            }
        }

        logText.text = _sb.ToString();
    }
}
