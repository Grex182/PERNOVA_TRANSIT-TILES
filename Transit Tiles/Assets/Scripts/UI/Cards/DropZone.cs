using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DropZone : MonoBehaviour, IDropHandler
{
    private List<CardType> usedCardTypes = new List<CardType>();

    public void OnDrop(PointerEventData eventData)
    {
        if (LevelManager.Instance.currState != MovementState.Card) return;
        Debug.Log("OnDrop to " + gameObject.name);

        CardsMovement c = eventData.pointerDrag.GetComponent<CardsMovement>();
        Cards card = eventData.pointerDrag.GetComponent<Cards>();

        if (c != null && card != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[3], false); // Play selection sound
            card.DoEffect();
            c.RemoveCard();
            Destroy(c.gameObject);
            Debug.Log("Card is Activated!");
        }
    }

    //public bool HasBeenDropped(Cards card)
    //{
    //    if (usedCardTypes.Contains(card.cardType))
    //    return true;
    //}

    private void ResetUsedCards()
    {
        usedCardTypes.Clear();
    }
}
