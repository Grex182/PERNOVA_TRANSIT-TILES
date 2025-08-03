using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
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

    [SerializeField] private GameObject _dialogueBox;
    [SerializeField] private string[] _claireSmallTalk;
    [SerializeField] private TextMeshProUGUI _claireText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }

        Instance = this;

        rerollCost = 0;
        UpdateRerollCostText();
        
        claireInitialPos = new Vector2(695f, 0f); 
        _claireRect.anchoredPosition = claireInitialPos;
        claireTargetPos = new Vector2(50f, 0f);
    }

    public void TogglePanel() //Shop comes down for player to view
    {
        if (ShopCanvas != null)
        {
            if (LevelManager.Instance != null)
            {
                currStarMoneyText.text = LevelManager.Instance.earnedStars.ToString();
                rerollButton.interactable = rerollCost <= LevelManager.Instance.earnedStars ? true : false;
            }
            if (TutorialManager.Instance != null)
            {
                currStarMoneyText.text = TutorialManager.Instance.earnedStars.ToString();
                rerollButton.interactable = rerollCost <= TutorialManager.Instance.earnedStars ? true : false;
            }
            
            foreach (var machine in cardPositions)
            {
                machine.GetComponent<CardMachine>().isAvailable = true;
            }


            SpawnCardsInShop();

            rerollButton.interactable = true;

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

                int randText = Random.Range(0, _claireSmallTalk.Length);
                _claireText.text = _claireSmallTalk[randText];


                if (!isOpen)
                {
                    _dialogueBox.SetActive(false);
                }

                //ShopCanvas.SetActive(!isOpen);
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
        ShopCanvas.SetActive(movingToTarget);
        _dialogueBox.SetActive(movingToTarget);
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
            
            // Select random card from pool
            int randIndex = Random.Range(0, pool.Count);
            var selectedCard = pool[randIndex];


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

            
        }
    }

    public void RerollShop()
    {
        foreach (var machine in cardPositions)
        {
            machine.GetComponent<CardMachine>().ClearOutCards();
        }

        SpawnCardsInShop();
        
        rerollButton.interactable = false;
        
    }

    public void PurchaseCard(CardsData.CardInfo cardInfo, int price)
    {
        //CardsData.CardInfo cardInfo = CardsData.Instance.originalCardsList[index]; 
        if (LevelManager.Instance != null && price <= LevelManager.Instance.earnedStars)
        {
            HandManager.Instance.DrawCard(cardInfo);
            LevelManager.Instance.ChangeEarnedStars(-price);
            rerollButton.interactable = rerollCost <= LevelManager.Instance.earnedStars ? true : false;


            currStarMoneyText.text = LevelManager.Instance.earnedStars.ToString();
            Debug.Log($"Current stars: {LevelManager.Instance.earnedStars}");
        }

        if (TutorialManager.Instance != null && price <= TutorialManager.Instance.earnedStars)
        {
            HandManager.Instance.DrawCard(cardInfo);
            TutorialManager.Instance.ChangeEarnedStars(-price);
            rerollButton.interactable = rerollCost <= TutorialManager.Instance.earnedStars ? true : false;

            currStarMoneyText.text = TutorialManager.Instance.earnedStars.ToString();
            Debug.Log($"Current stars: {TutorialManager.Instance.earnedStars}");
        }

        CardsData.Instance.currentCardsList.Remove(cardInfo);
    }

    private void UpdateRerollCostText()
    {
        rerollCostText.text = rerollCost.ToString();
    }
}
