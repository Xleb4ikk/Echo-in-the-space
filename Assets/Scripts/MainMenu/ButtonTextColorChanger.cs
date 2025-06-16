using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text text;
    public Color normalColor = Color.white;
    public Color highlightedColor = Color.yellow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = highlightedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }
}
