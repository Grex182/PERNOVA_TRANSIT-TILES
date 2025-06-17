using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    [SerializeField] private float goalWidth;
    [SerializeField] private float duration;
    private float initialWidth;


    [SerializeField] private List<GameObject> _cardSlots = new List<GameObject>();
    [SerializeField] private GameObject _cardPrefab;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialWidth = rectTransform.rect.width;
    }

    //private void Update()
    //{
    //    for (int i = 0; i < _cardSlots.Count - 1; i++)
    //    {
    //        if (_cardSlots[i].transform.childCount == 0 && _cardSlots[i + 1].transform.childCount > 0)
    //        {
    //            // Move the next card into the empty slot
    //            Transform card = _cardSlots[i + 1].transform.GetChild(0);
    //            card.SetParent(_cardSlots[i].transform);
    //            card.localPosition = Vector3.zero;
    //            card.GetComponent<CardsMovement>().SetSlot(_cardSlots[i]);
    //        }
    //    }
    //}

    public void OnPointerEnter(PointerEventData eventData) => StartCoroutine(AnimateWidth(goalWidth, duration));

    public void OnPointerExit(PointerEventData eventData) => StartCoroutine(AnimateWidth(initialWidth, duration));

    private IEnumerator AnimateWidth(float targetWidth, float duration)
    {
        float startWidth = rectTransform.rect.width;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float newWidth = Mathf.Lerp(startWidth, targetWidth, t);

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);

            yield return null;
        }

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }

    public void DrawRandomCard()
    {
        foreach (GameObject slot in _cardSlots)
        {
            if (slot.transform.childCount == 0)
            {
                var newCard = Instantiate(_cardPrefab, slot.transform);
                CardsMovement movementScript = newCard.GetComponent<CardsMovement>();

                movementScript.SetSlot(slot);
                return;
            }
        }
    }

    public void OnCardRemoved(GameObject removedSlot)
    {
        int removedIndex = _cardSlots.IndexOf(removedSlot);
        if (removedIndex == -1) return;

        // Shift all cards after the removed slot forward
        for (int i = removedIndex; i < _cardSlots.Count - 1; i++)
        {
            GameObject currentSlot = _cardSlots[i];
            GameObject nextSlot = _cardSlots[i + 1];

            if (nextSlot.transform.childCount > 0)
            {
                Transform card = nextSlot.transform.GetChild(0);
                card.SetParent(currentSlot.transform);
                card.localPosition = Vector3.zero;
                card.GetComponent<CardsMovement>().SetSlot(currentSlot);
            }
        }
    }
}
