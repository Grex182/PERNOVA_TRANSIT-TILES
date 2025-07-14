using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            newCard.GetComponent<CardsMovement>().enabled = false;
            newCard.GetComponent<Cards>().Initialize(selectedCard);

            CardsData.Instance.currentCardsList.RemoveAt(index);

            Debug.Log("spawned card");
        }
    }
}
