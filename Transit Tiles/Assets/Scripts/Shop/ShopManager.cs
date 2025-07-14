using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShopManager : Singleton<ShopManager>
{
    [SerializeField] private GameObject slidingDownPanel;
    [SerializeField] private Transform cardPositionsParent;
    [SerializeField] private List<Transform> cardPositions = new List<Transform>();

    private void Awake()
    {
        foreach (Transform child in cardPositionsParent)
        {
            cardPositions.Add(child);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) //This refreshes shop, which could be used for the rerolls you know what im sayin'?
        {
            CardsData.Instance.currentCardsList = CardsData.Instance.originalCardsList.ToList();
            SpawnCardsInShop();
        }
    }

    public void TogglePanel()
    {
        if (slidingDownPanel != null)
        {
            Animator anim = slidingDownPanel.GetComponent<Animator>();

            if (anim != null)
            {
                bool isOpen = anim.GetBool("open");

                anim.SetBool("open", !isOpen);
            }
        }
    }

    public void SpawnCardsInShop()
    {
        Debug.Log($"cardPositions.Count = {cardPositions.Count}");
        Debug.Log($"cardsList.Count = {CardsData.Instance.currentCardsList.Count}");

        int spawnCount = Mathf.Min(cardPositions.Count, CardsData.Instance.currentCardsList.Count);

        for (int i = 0; i < spawnCount; i++) //for how many positions there are inside cardPositions
        {
            Transform pos = cardPositions[i];
            int index = Random.Range(0, CardsData.Instance.currentCardsList.Count); //a random index inside cardsList List
            var selectedCard = CardsData.Instance.currentCardsList[index];

            var newCard = Instantiate(HandManager.Instance.CardPrefab(), pos);
            Button button = pos.GetComponentInChildren<Button>();

            int capturedIndex = index;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => PurchaseCard(selectedCard)); //Now this will be the purchasing person thingy or something ok gbye
            newCard.GetComponent<CardsMovement>().enabled = false;
            newCard.GetComponent<Cards>().Initialize(selectedCard);

            CardsData.Instance.currentCardsList.RemoveAt(index);

            Debug.Log("spawned card");
        }
    }

    public void PurchaseCard(CardsData.CardInfo cardInfo)
    {
        //CardsData.CardInfo cardInfo = CardsData.Instance.originalCardsList[index];

        HandManager.Instance.DrawCard(cardInfo);
    }

/*    public void DrawRandomCard()
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
    }*/
}
