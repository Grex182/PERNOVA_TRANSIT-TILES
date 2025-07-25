using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    [SerializeField] private GameObject ShopCanvas;
    [SerializeField] private GameObject slidingDownPanel;
    [SerializeField] private Transform cardPositionsParent;
    [SerializeField] private List<Transform> cardPositions = new List<Transform>();
    [SerializeField] private TextMeshProUGUI currStarMoneyText;
    [SerializeField] private TMP_Text rerollCostText;

    [SerializeField] private int rerollCost;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }

        Instance = this;

        foreach (Transform child in cardPositionsParent)
        {
            cardPositions.Add(child);
        }

        rerollCost = 1;
    }

    private void Update()
    {
    }

    public void TogglePanel() //Shop comes down for player to view
    {
        if (ShopCanvas != null)
        {
            currStarMoneyText.text = "Stars:" + LevelManager.Instance.totalStars.ToString();
            RerollShop();

            Animator anim = ShopCanvas.GetComponentInChildren<Animator>();

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

        int spawnCount = Mathf.Min(cardPositions.Count, cardsData.currentCardsList.Count);

        for (int slotIndex = 0; slotIndex < spawnCount; slotIndex++)
        {
            // Randomly choose rarity based on percentage
            float rand = Random.Range(0f, 100f);
            CardsData.CardRarity selectedRarity;
            int price = 0;

            if (rand < 40f)
            {
                selectedRarity = CardsData.CardRarity.Common;
                price = 1;
            }
            else if (rand < 70f)
            {
                selectedRarity = CardsData.CardRarity.Uncommon;
                price = 2;
            }
            else if (rand < 90f)
            {
                selectedRarity = CardsData.CardRarity.Rare;
                price = 3;
            }
            else
            {
                selectedRarity = CardsData.CardRarity.Epic;
                price = 4;
            }

            Debug.Log($"The selected rarity for the card is {selectedRarity}");

            // Get valid cards in current pool of that rarity
            List<CardsData.CardInfo> pool = cardsData.currentCardsList
                .Where(card => CardsData.CardData.GetCardsByRarity(selectedRarity).Contains(card))
                .ToList();

            // If pool is empty for that rarity, fallback to first available rarity
            if (pool.Count == 0)
            {
                pool = cardsData.currentCardsList.ToList();
                if (pool.Count == 0) break; // Safety net if no cards available
            }

            // Select random card from pool
            int randIndex = Random.Range(0, pool.Count);
            var selectedCard = pool[randIndex];

            cardsData.currentCardsList.Remove(selectedCard);

            Transform pos = cardPositions[slotIndex];

            if (pos.gameObject.activeSelf == false)
                pos.gameObject.SetActive(true);

            var newCard = Instantiate(HandManager.Instance.CardPrefab(), pos);
            Button button = pos.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => PurchaseCard(selectedCard, price, pos.gameObject));
            button.GetComponentInChildren<TMP_Text>().text = $"Pay {price} Public Rating Stars";
            newCard.GetComponent<CardsMovement>().enabled = false;
            newCard.GetComponent<Cards>().Initialize(selectedCard);
        }
    }

    public void RerollShop()
    {
        LevelManager levelManager = LevelManager.Instance;
        CardsData cardsData = CardsData.Instance;

        //FOR WHEN THE TRAIN FINISHES ONE LOOP, AND SHOP SPAWNS AGAIN IG, REROLLCOST RESETS BACK TO 1
/*        if (trainHasFinishedOneLoop)
        {
            rerollCost = 1;
        }*/

        // Reset the current list excluding purchased cards
        if (rerollCost <= levelManager.totalStars)
        {
            cardsData.currentCardsList = cardsData.originalCardsList.ToList();
            SpawnCardsInShop();
            rerollCost++;
        }
        else
        {
            Debug.Log("Sorry not enough money to reroll.");
        }
    }

    public void PurchaseCard(CardsData.CardInfo cardInfo, int price, GameObject transformObject)
    {
        //CardsData.CardInfo cardInfo = CardsData.Instance.originalCardsList[index]; 
        LevelManager levelManager = LevelManager.Instance;

        if (price <= levelManager.totalStars)
        {
            HandManager.Instance.DrawCard(cardInfo);
            levelManager.totalStars -= price;
            transformObject.SetActive(false);
            currStarMoneyText.text = "Stars:" + LevelManager.Instance.totalStars.ToString();
            Debug.Log($"Current stars: {levelManager.totalStars}");
        }
        else
        {
            Debug.Log("Not enough stars.");
        }
    }
}
