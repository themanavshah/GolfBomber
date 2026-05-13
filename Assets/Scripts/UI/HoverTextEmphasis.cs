using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTextEmphasis : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private float normalFontSize = 24f;
    [SerializeField] private float hoverFontSize = 36f;
    [SerializeField] private FontStyles normalStyle = FontStyles.Normal;
    [SerializeField] private FontStyles hoverStyle = FontStyles.Bold;

    void Awake()
    {
        if (targetText == null) targetText = GetComponentInChildren<TMP_Text>();
        ApplyNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetText == null) return;
        targetText.fontSize = hoverFontSize;
        targetText.fontStyle = hoverStyle;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetText == null) return;
        ApplyNormal();
    }

    void ApplyNormal()
    {
        targetText.fontSize = normalFontSize;
        targetText.fontStyle = normalStyle;
    }
}
