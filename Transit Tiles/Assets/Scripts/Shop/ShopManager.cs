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
    [SerializeField] private Button rerollButton;

    [SerializeField] private RectTransform _claireRect;
    Vector2 claireInitialPos;
    Vector2 claireTargetPos;
    private readonly float _claireMoveSpeed = 0.5f;
    private Coroutine claireCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }

        Instance = this;

        rerollCost = 0;
        UpdateRerollCostText();
        if (rerollCost > TutorialManager.Instance.earnedStars)
        {
            rerollButton.interactable = false;
        }
        else
        {
            rerollButton.interactable = true;
        }
        claireInitialPos = new Vector2(695f, 0f); 
        _claireRect.anchoredPosition = claireInitialPos;
        claireTargetPos = new Vector2(50f, 0f);
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
                
                if (TutorialManager.Instance != null) { return; }

                if (claireCoroutine != null)
                {
                    StopCoroutine(claireCoroutine);
                }
                claireCoroutine = StartCoroutine(MoveClaireSmoothly(!isOpen));
                
                ShopCanvas.SetActive(!isOpen);
            }
        }
    }

    IEnumerator MoveClaireSmoothly(bool movingToTarget)
    {
        Vector3 startPos = _claireRect.anchoredPosition;
        Vector3 endPos = movingToTarget ? claireTargetPos : claireInitialPos;

        float duration = _claireMoveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            _claireRect.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure exact final position
        _claireRect.anchoredPosition = endPos;
        claireCoroutine = null;
    }

    public void SpawnCardsInShop()
    {
        Debug.Log("Spawning Cards in Shop");
        CardsData cardsData = CardsData.Instance;
        Debug.Log($"cardPositions.Count = {cardPositions.Count}");
        Debug.Log($"currentCardsList.Count = {cardsData.currentCardsList.Count}");

        List <CardType> rolledCards = new List <CardType>();

        //int spawnCount = Mathf.Min(cardPositions.Count, cardsData.currentCardsList.Count);

        for (int slotIndex = 0; slotIndex < 3;)
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

            List<CardsData.CardInfo> pool = CardsData.CardData.GetCardsByRarity(selectedRarity);
            /*
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
            */
            // Select random card from pool
            int randIndex = Random.Range(0, pool.Count);
            var selectedCard = pool[randIndex];

            //cardsData.currentCardsList.Remove(selectedCard);

            bool isCardDupe = false;
            foreach (var card in rolledCards)
            {
                if (card == selectedCard.cardType)
                {
                    isCardDupe = true;
                }
            }

            if (!isCardDupe)
            {
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
                slotIndex++;
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
                if (rerollCost > TutorialManager.Instance.earnedStars)
                {
                    rerollButton.interactable = false;
                }

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
                if (rerollCost > LevelManager.Instance.earnedStars)
                {
                    rerollButton.interactable = false;
                }

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

        CardsData.Instance.currentCardsList.Remove(cardInfo);
    }

    private void UpdateRerollCostText()
    {
        rerollCostText.text = (rerollCost > 1) ? $"{rerollCost} Stars" : $"{rerollCost} Star";
    }
}
