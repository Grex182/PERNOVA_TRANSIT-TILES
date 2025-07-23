using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LEDButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("------ Button Sprites ------")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite initialSprite;
    [SerializeField] private Sprite pressSprite;
    [SerializeField] private Material GlowMaterial;

    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Color colorInitial;
    [SerializeField] private Color colorHover;


    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.sprite = initialSprite;
        buttonImage.material = GlowMaterial;
        buttonText.color = colorHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.sprite = initialSprite;
        buttonImage.material = null;
        buttonText.color = colorInitial;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.sprite = pressSprite;
        buttonImage.material = null;
        buttonText.color = colorHover;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonImage.sprite = initialSprite;
        buttonImage.material = GlowMaterial;
        buttonText.color = colorHover;
    }
}
