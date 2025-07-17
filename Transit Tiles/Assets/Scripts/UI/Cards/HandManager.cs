using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandManager : Singleton<HandManager>, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    [SerializeField] private float goalWidth;
    [SerializeField] private float duration;
    private float initialWidth;


    [SerializeField] private List<GameObject> _cardSlots = new List<GameObject>();
    [SerializeField] private GameObject _cardPrefab;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialWidth = rectTransform.rect.width;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(AnimateWidth(goalWidth, duration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(AnimateWidth(initialWidth, duration));
    }

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

    public void DrawCard(CardsData.CardInfo cardInfo)
    {
        foreach (GameObject slot in _cardSlots)
        {
            if (slot.transform.childCount == 0)
            {
                var newCard = Instantiate(_cardPrefab, slot.transform);
                newCard.GetComponent<Cards>().Initialize(cardInfo);

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

    public GameObject CardPrefab()
    {
        return _cardPrefab;
    }
}
