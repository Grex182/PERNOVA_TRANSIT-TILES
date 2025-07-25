using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (LevelManager.Instance.currState != MovementState.Card) return;
        Debug.Log("OnDrop to " + gameObject.name);

        CardsMovement c = eventData.pointerDrag.GetComponent<CardsMovement>();
        if (c != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[3], false); // Play selection sound
            c.RemoveCard();
            Destroy(c.gameObject);
            Debug.Log("Card is Activated!");
        }
    }
}
