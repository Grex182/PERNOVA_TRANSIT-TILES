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
            RerollShop();
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
        CardsData cardsData = CardsData.Instance;
        Debug.Log($"cardPositions.Count = {cardPositions.Count}");
        Debug.Log($"currentCardsList.Count = {cardsData.currentCardsList.Count}");

        CardsData.CardRarity[] rarityOrder = new[]
        {
        CardsData.CardRarity.Common,
        CardsData.CardRarity.Uncommon,
        CardsData.CardRarity.Rare,
        CardsData.CardRarity.Epic
    };

        int spawnCount = Mathf.Min(cardPositions.Count, cardsData.currentCardsList.Count);
        int slotIndex = 0;

        foreach (var rarity in rarityOrder)
        {
            var potential = cardsData.currentCardsList
                .Where(card => CardsData.CardData.GetCardsByRarity(rarity).Contains(card))
                .ToList();

            while (potential.Count > 0 && slotIndex < spawnCount)
            {
                int randIndex = Random.Range(0, potential.Count);
                var selectedCard = potential[randIndex];
                potential.RemoveAt(randIndex);
                cardsData.currentCardsList.Remove(selectedCard);

                Transform pos = cardPositions[slotIndex];

                var newCard = Instantiate(HandManager.Instance.CardPrefab(), pos);
                Button button = pos.GetComponentInChildren<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => PurchaseCard(selectedCard));
                newCard.GetComponent<CardsMovement>().enabled = false;
                newCard.GetComponent<Cards>().Initialize(selectedCard);

                Debug.Log($"Spawned card: {selectedCard.cardName} [{rarity}]");

                slotIndex++;
            }

            if (slotIndex >= spawnCount)
                break;
        }

        if (slotIndex < cardPositions.Count)
        {
            Debug.LogWarning($"Only filled {slotIndex} out of {cardPositions.Count} slots.");
        }
    }

    public void RerollShop()
    {
        CardsData cardsData = CardsData.Instance;

        // Reset the current list excluding purchased cards
        cardsData.currentCardsList = cardsData.originalCardsList
            .Where(card => !cardsData.purchasedCardsList.Contains(card))
            .ToList();

        SpawnCardsInShop();
    }

    public void PurchaseCard(CardsData.CardInfo cardInfo)
    {
        //CardsData.CardInfo cardInfo = CardsData.Instance.originalCardsList[index];
        CardsData cardsData = CardsData.Instance;

        // Track that this card has been purchased
        if (!cardsData.purchasedCardsList.Contains(cardInfo))
            cardsData.purchasedCardsList.Add(cardInfo);

        HandManager.Instance.DrawCard(cardInfo);
    }
}
