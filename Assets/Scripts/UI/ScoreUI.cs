using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private string format = "Score: {0}";

    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
            HandleScoreChanged(ScoreManager.Instance.CurrentScore);
        }
        else
        {
            Debug.LogWarning($"{nameof(ScoreUI)}: no ScoreManager in scene.", this);
        }
    }

    void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }

    void HandleScoreChanged(int newScore)
    {
        if (scoreText != null) scoreText.text = string.Format(format, newScore);
    }
}
