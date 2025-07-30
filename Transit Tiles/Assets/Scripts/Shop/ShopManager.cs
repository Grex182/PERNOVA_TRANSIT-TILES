using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    [SerializeField] private GameObject ShopCanvas;
    [SerializeField] private GameObject slidingDownPanel;
    [SerializeField] private Transform cardPositionsParent;
    [SerializeField] private List<GameObject> cardPositions = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI currStarMoneyText;
    [SerializeField] private TMP_Text rerollCostText;

    [SerializeField] private GameObject _cardPrefab;

    [SerializeField] private int rerollCost;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }

        Instance = this;

        rerollCost = 0;
        UpdateRerollCostText();
    }

    private void Update()
    {
    }

    public void TogglePanel() //Shop comes down for player to view
    {
        if (ShopCanvas != null)
        {
            if (LevelManager.Instance != null) currStarMoneyText.text = LevelManager.Instance.earnedStars.ToString();
            if (TutorialManager.Instance != null) currStarMoneyText.text = TutorialManager.Instance.earnedStars.ToString();
            
            foreach (var machine in cardPositions)
            {
                machine.GetComponent<CardMachine>().isAvailable = true;
            }

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
        Debug.Log("Spawning Cards in Shop");
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
                price = 4;
            }
            else
            {
                selectedRarity = CardsData.CardRarity.Epic;
                price = 6;
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

            CardMachine machine = cardPositions[slotIndex].GetComponent<CardMachine>();
            

            if (machine.isAvailable)
            {
                Transform pos = machine.cardPos;
                Button button = machine.buyButton;
                var newCard = Instantiate(_cardPrefab, pos);
                button.onClick.AddListener(() => PurchaseCard(selectedCard, price));
                newCard.GetComponent<CardsMovement>().enabled = false;
                machine.SetUpCardDisplay(selectedRarity, price);
                newCard.GetComponent<Cards>().Initialize(selectedCard);
            }


            /*
            var newCard = Instantiate(HandManager.Instance.CardPrefab(), pos);
            Button button = pos.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => PurchaseCard(selectedCard, price, pos.gameObject));
            button.GetComponentInChildren<TMP_Text>().text = $"Pay {price} Public Rating Stars";
            newCard.GetComponent<CardsMovement>().enabled = false;
            newCard.GetComponent<Cards>().Initialize(selectedCard);
            */
        }
    }

    public void RerollShop()
    {
        // Reset the current list excluding purchased cards
        if (TutorialManager.Instance != null)
        {
            //Reroll cost resets to 1 
            if (TutorialManager.Instance.isEndStation)
            {
                rerollCost = 0;
                UpdateRerollCostText();
            }

            if (rerollCost <= TutorialManager.Instance.earnedStars)
            {
                TutorialManager.Instance.ChangeEarnedStars(-rerollCost);
                currStarMoneyText.text = "Stars:" + TutorialManager.Instance.earnedStars.ToString();
                CardsData.Instance.currentCardsList = CardsData.Instance.originalCardsList.ToList();
                SpawnCardsInShop();
                rerollCost++;
                UpdateRerollCostText();
                Debug.Log("Added rerollCost to Shop");
            }
        }
        else if (LevelManager.Instance != null)
        {
            //Reroll cost resets to 1 
            if (LevelManager.Instance.isEndStation)
            {
                rerollCost = 0;
                UpdateRerollCostText();
            }

            // Reset the current list excluding purchased cards
            if (rerollCost <= LevelManager.Instance.earnedStars)
            {
                LevelManager.Instance.ChangeEarnedStars(-rerollCost);
                foreach (var machine in cardPositions)
                {
                    machine.GetComponent<CardMachine>().ClearOutCards();
                }
                currStarMoneyText.text = "Stars:" + LevelManager.Instance.earnedStars.ToString();
                CardsData.Instance.currentCardsList = CardsData.Instance.originalCardsList.ToList();
                SpawnCardsInShop();
                rerollCost++;
                UpdateRerollCostText();
                Debug.Log("Added rerollCost to Shop");
            }
        }

        Debug.Log("Shop Rerolled");
    }

    public void PurchaseCard(CardsData.CardInfo cardInfo, int price)
    {
        //CardsData.CardInfo cardInfo = CardsData.Instance.originalCardsList[index]; 
        if (LevelManager.Instance != null && price <= LevelManager.Instance.earnedStars)
        {
            HandManager.Instance.DrawCard(cardInfo);
            LevelManager.Instance.ChangeEarnedStars(-price);

            currStarMoneyText.text = "Stars:" + LevelManager.Instance.earnedStars.ToString();
            Debug.Log($"Current stars: {LevelManager.Instance.earnedStars}");
        }

        if (TutorialManager.Instance != null && price <= TutorialManager.Instance.earnedStars)
        {
            HandManager.Instance.DrawCard(cardInfo);
            TutorialManager.Instance.ChangeEarnedStars(-price);
            currStarMoneyText.text = "Stars:" + TutorialManager.Instance.earnedStars.ToString();
            Debug.Log($"Current stars: {TutorialManager.Instance.earnedStars}");
        }
    }

    private void UpdateRerollCostText()
    {
        rerollCostText.text = (rerollCost > 1) ? $"{rerollCost} Stars" : $"{rerollCost} Star";
    }
}
