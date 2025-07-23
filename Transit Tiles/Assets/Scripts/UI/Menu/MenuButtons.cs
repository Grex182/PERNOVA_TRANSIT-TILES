using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("------ Button Sprites ------")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite initialSprite;
    [SerializeField] private Sprite hoverSprite;

    [SerializeField] private Image ledIndicator;
    [SerializeField] private Sprite offLed;
    [SerializeField] private Sprite onLed;
    [SerializeField] private Material GlowLed;

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.sprite = hoverSprite;
        ledIndicator.sprite = onLed;
        ledIndicator.material = GlowLed;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.sprite = initialSprite;
        ledIndicator.sprite = offLed;
        ledIndicator.material = null;
    }
}
