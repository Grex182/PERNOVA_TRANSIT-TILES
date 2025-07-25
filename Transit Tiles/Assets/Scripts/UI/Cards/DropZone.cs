using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private List<CardType> usedCardTypes = new List<CardType>();

    private void Update()
    {
        if (LevelManager.Instance.currState == MovementState.Travel)
        {
            ResetUsedCards();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (LevelManager.Instance.currState != MovementState.Card) { return; }

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
        foreach (CardType cardType in usedCardTypes)
        {
            usedCardTypes.Remove(cardType);
        }
        usedCardTypes.Clear();
    }
}
