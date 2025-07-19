using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CardsData : Singleton<CardsData>
{
    [SerializeField] public List<CardInfo> originalCardsList = new List<CardInfo>();
    [SerializeField] public List<CardInfo> currentCardsList = new List<CardInfo>();

    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    private void Start()
    {
        originalCardsList.AddRange(CardData.GetCardsByRarity(CardRarity.Common));
        originalCardsList.AddRange(CardData.GetCardsByRarity(CardRarity.Uncommon));
        originalCardsList.AddRange(CardData.GetCardsByRarity(CardRarity.Rare));
        originalCardsList.AddRange(CardData.GetCardsByRarity(CardRarity.Epic));

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.RerollShop();
        }
    }

    public class CardData
    {
        public CardRarity Rarity { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Function { get; private set; }
        public int ImgIndex { get; private set; }

        private static readonly List<CardInfo> CommonData = new List<CardInfo>
        {
            new CardInfo {cardName = "Floor Sweeper", cardFunction = "Passengers clean trash in adjacent tiles, improving mood.", cardImgIndex = 0 }, // \r\n
            new CardInfo {cardName = "Deodorant", cardFunction = "Neutralizes the “Stinky Winky” effect, stopping mood decay to surrounding passengers from bad odors.", cardImgIndex = 10 }
        };

        private static readonly List<CardInfo> UncommonData = new List<CardInfo>
{
            new CardInfo {cardName = "Chill Beats", cardFunction = "Plays calming music, increasing nearby passengers' mood.", cardImgIndex = 2},
            new CardInfo {cardName = "Leg Day", cardFunction = "For one stop, passengers avoid sitting, freeing up seats for Priority Passengers.", cardImgIndex = 3},
            new CardInfo {cardName = "Patrolling Guard", cardFunction = "Silences noisy passengers (“Yappers”) and stops their mood decays to surrounding passengers.", cardImgIndex = 4},
        };

        private static readonly List<CardInfo> RareData = new List<CardInfo>
        {
            new CardInfo {cardName = "Filipino Time", cardFunction = "Extends door-open duration by X seconds for last-minute adjustments.", cardImgIndex = 5},
            new CardInfo {cardName = "Excuse me po", cardFunction = "Lets players drag passengers out of doors instantly.", cardImgIndex = 6},
        };

        private static readonly List<CardInfo> EpicData = new List<CardInfo>
        {
            new CardInfo {cardName = "Skinny Legend", cardFunction = "Grants diagonal movement for passengers for X seconds.", cardImgIndex = 7},
            new CardInfo {cardName = "Rush Hour Regulars", cardFunction = "Temporarily reduces the spawn rate of Priority Passengers (e.g., PWDs, elderly, pregnant women, etc.).", cardImgIndex = 8},
        };

        #region SET BASE CARD
        public CardData()
        {
            DrawCard(Random.Range(0f, 100f));
        }

/*        public void DrawCard(float randNum)
        {
            if (randNum < 40f) // Common
            {
                this.Rarity = CardRarity.Common;
                SetRandomCard(CommonData);
            }
            else if (randNum < 70f) // Uncommon
            {
                this.Rarity = CardRarity.Uncommon;
                SetRandomCard(UncommonData);
            }
            else if (randNum < 90f) // Rare
            {
                this.Rarity = CardRarity.Rare;
                SetRandomCard(RareData);
            }
            else // Epic
            {
                this.Rarity = CardRarity.Epic;
                SetRandomCard(EpicData);
            }
        }*/

        public void DrawCard(float randNum)
        {
            if (randNum < 40f) // Common
            {
                this.Rarity = CardRarity.Common;
                SetRandomCard(CommonData);
            }
            else if (randNum < 70f) // Uncommon
            {
                this.Rarity = CardRarity.Uncommon;
                SetRandomCard(UncommonData);
            }
            else if (randNum < 90f) // Rare
            {
                this.Rarity = CardRarity.Rare;
                SetRandomCard(RareData);
            }
            else // Epic
            {
                this.Rarity = CardRarity.Epic;
                SetRandomCard(EpicData);
            }
        }

        public void SetCard(List<CardInfo> cardType, int index)
        {
            Name = cardType[index].cardName;
            Function = cardType[index].cardFunction;
            ImgIndex = cardType[index].cardImgIndex;
        }

        private void SetRandomCard(List<CardInfo> cardType)
        {
            int index = Random.Range(0, cardType.Count);

            Name = cardType[index].cardName;
            Function = cardType[index].cardFunction;
            ImgIndex = cardType[index].cardImgIndex;
        }
        #endregion

        public static List<CardInfo> GetCardsByRarity(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => CommonData,
                CardRarity.Uncommon => UncommonData,
                CardRarity.Rare => RareData,
                CardRarity.Epic => EpicData,
                _ => null,
            };
        }
    }

    [System.Serializable]
    public class CardInfo
    {
        public string cardName;
        public string cardFunction;
        public int cardImgIndex;
    }
}
