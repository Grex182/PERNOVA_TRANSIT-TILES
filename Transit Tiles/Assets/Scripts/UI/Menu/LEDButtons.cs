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
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite initialSprite;
    [SerializeField] private Sprite pressSprite;
    [SerializeField] private Material GlowMaterial;
    private bool isLocked = true;

    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Color colorInitial;
    [SerializeField] private Color colorHover;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        buttonImage.sprite = initialSprite;
        buttonImage.material = GlowMaterial;
        buttonText.color = colorHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;
        buttonImage.sprite = initialSprite;
        buttonImage.material = null;
        buttonText.color = colorInitial;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        buttonImage.sprite = pressSprite;
        buttonImage.material = null;
        buttonText.color = colorHover;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        buttonImage.sprite = initialSprite;
        buttonImage.material = GlowMaterial;
        buttonText.color = colorHover;
    }

    private void Update()
    {

        if (button.interactable != isLocked)
        {
            buttonImage.sprite = pressSprite;
            buttonImage.material = null;
            buttonText.color = colorHover;
            isLocked = false;
        }
        if (button.interactable && !isLocked) 
        {
            buttonImage.sprite = initialSprite;
            buttonImage.material = null;
            buttonText.color = colorInitial;
            isLocked = true; 
        }

    }
}
