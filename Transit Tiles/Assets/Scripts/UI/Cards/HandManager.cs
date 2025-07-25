using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static CardsData;

public class HandManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static HandManager Instance;

    private RectTransform rectTransform;
    [SerializeField] private float goalWidth;
    [SerializeField] private float duration;
    private float initialWidth;

    [SerializeField] private List<GameObject> _cardSlots = new List<GameObject>();
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private bool isHovered;
    [SerializeField] private int[] coordsClose = new int[6];
    [SerializeField] private int[] coordsOpen = new int[6];

    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // Destroy any duplicates
        }

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
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(AnimateWidth(initialWidth, duration));
        isHovered = false;
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

    public void DrawRandomCard() // For Testing Purposes
    {
        foreach (GameObject slot in _cardSlots)
        {
            #region Draw Random Card
            if (slot.transform.childCount == 0)
            {
                var cardData = new CardsData.CardData();
                cardData.DrawCard(Random.Range(0f, 100f));

                List<CardsData.CardInfo> cardsByRarity = CardsData.CardData.GetCardsByRarity(cardData.Rarity);

                CardsData.CardInfo cardInfo = cardsByRarity.FirstOrDefault(c => c.cardName == cardData.Name);
                if (cardInfo == null)
                {
                    Debug.LogWarning("CardInfo not found for: " + cardData.Name);
                    return;
                }

                GameObject newCard = Instantiate(_cardPrefab, slot.transform);
                newCard.GetComponent<Cards>().Initialize(cardInfo);

                CardsMovement movementScript = newCard.GetComponent<CardsMovement>();
                movementScript.SetSlot(slot);
                return;
            }
            #endregion

            //if (slot.transform.childCount == 0)
            //{
            //    // Step 1: Get only rare cards
            //    List<CardsData.CardInfo> epicCards = CardsData.CardData.GetCardsByRarity(CardsData.CardRarity.Epic);

            //    if (epicCards == null || epicCards.Count == 0)
            //    {
            //        Debug.LogWarning("No rare cards found!");
            //        return;
            //    }

            //    // Step 2: Pick one at random
            //    CardsData.CardInfo selectedCard = epicCards[Random.Range(0, epicCards.Count)];

            //    // Step 4: Instantiate the card and initialize it
            //    GameObject newCard = Instantiate(_cardPrefab, slot.transform);
            //    newCard.GetComponent<Cards>().Initialize(selectedCard);

            //    // Step 5: Set its slot
            //    CardsMovement movementScript = newCard.GetComponent<CardsMovement>();
            //    movementScript.SetSlot(slot);
            //    return;
            //}
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
