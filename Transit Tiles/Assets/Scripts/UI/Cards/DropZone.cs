using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private List<CardType> usedCardTypes = new List<CardType>();
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image border;
    [SerializeField] private float _fadeInSpeed = 2f;
    [SerializeField] private float _fadeOutSpeed = 5f;

    public bool isActivated = false;
    public float alpha;
    private bool tutorialIsDone = false;


    private void Update()
    {
        MovementState moveState = LevelManager.Instance != null ? 
            LevelManager.Instance.currState : 
            TutorialManager.Instance.currState;
        if (moveState == MovementState.Travel)
        {
            ResetUsedCards();
        }

        if (isActivated) 
        {
            if (alpha < .48f)
            {
                alpha += Time.deltaTime * _fadeInSpeed;

                gameObject.GetComponent<Image>().color = DoFade(gameObject.GetComponent<Image>().color, alpha);
                text.color = DoFade(text.color, alpha);
                border.color = DoFade(border.color, alpha);
            }
        }
        else if (alpha > 0)
        {
            alpha -= Time.deltaTime * _fadeOutSpeed;

            gameObject.GetComponent<Image>().color = DoFade(gameObject.GetComponent<Image>().color, alpha);
            text.color = DoFade(text.color,alpha);
            border.color = DoFade(border.color, alpha);
        }
    }

    private Color DoFade(Color baseColor, float alpha)
    {
        Color newColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        return newColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        MovementState moveState = LevelManager.Instance != null ?
            LevelManager.Instance.currState :
            TutorialManager.Instance.currState;
        if (moveState != MovementState.Card) { return; }

        Debug.Log("OnDrop to " + gameObject.name);

        CardsMovement c = eventData.pointerDrag.GetComponent<CardsMovement>();
        Cards card = eventData.pointerDrag.GetComponent<Cards>();

        if (c != null && card != null)
        {
            if (HasBeenDropped(card))
            {
                return;
            }
            else
            {
                if (TutorialManager.Instance != null && !tutorialIsDone)
                {
                    TutorialManager.Instance.OnNextTutorialClicked();
                    tutorialIsDone = true;
                }
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[3], false); // Play selection sound
                card.DoEffect();
                usedCardTypes.Add(card.GetCardType());
                c.RemoveCard();
                Destroy(c.gameObject);
                Debug.Log("Card is Activated!");
            }
        }
    }

    

    public bool HasBeenDropped(Cards card)
    {
        if (usedCardTypes.Contains(card.GetCardType()))
        {
            return true;
        }
        return false;
    }

    private void ResetUsedCards()
    {
        if (usedCardTypes.Count == 0) { return; }

        for (int i = usedCardTypes.Count - 1; i >= 0; i--)
        {
            usedCardTypes.RemoveAt(i); // Safe to remove in reverse
        }
        usedCardTypes.Clear();
    }
}
