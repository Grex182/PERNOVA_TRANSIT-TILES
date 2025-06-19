using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsData : Singleton<CardsData>
{
    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    
    public class CardData
    {
        public CardRarity Rarity { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Function { get; private set; }
        public int ImgIndex { get; private set; }

        private static readonly (string _cardName, string _cardFunction, int _cardImgIndex)[] CommonData =
        {
            ("Floor Sweeper", "Passengers clean trash in adjacent tiles, improving mood.", 0), // \r\n
            ("Deodorant", "Neutralizes the “Stinky Winky” effect, stopping mood decay to surrounding passengers from bad odors.", 1)
        };

        private static readonly (string _cardName, string _cardFunction, int _cardImgIndex)[] UncommonData =
{
            ("Chill Beats", "Plays calming music, increasing nearby passengers' mood.", 2),
            ("Leg Day", "For one stop, passengers avoid sitting, freeing up seats for Priority Passengers.", 3),
            ("Patrolling Guard", "Silences noisy passengers (“Yappers”) and stops their mood decays to surrounding passengers.", 4)
        };

        private static readonly (string _cardName, string _cardFunction, int _cardImgIndex)[] RareData =
        {
            ("Filipino Time", "Extends door-open duration by X seconds for last-minute adjustments.", 5),
            ("Excuse me po", "Lets players drag passengers out of doors instantly.", 6)
        };

        private static readonly (string _cardName, string _cardFunction, int _cardImgIndex)[] EpicData =
        {
            ("Skinny Legend", "Grants diagonal movement for passengers for X seconds.", 7),
            ("Rush Hour Regulars", "Temporarily reduces the spawn rate of Priority Passengers (e.g., PWDs, elderly, pregnant women, etc.).", 8),
        };

        #region SET BASE CARD
        public CardData()
        {
            DrawCard(Random.Range(0f, 100f));
        }

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

        private void SetRandomCard((string, string, int)[] cardType)
        {
            int index = Random.Range(0, cardType.Length);

            Name = cardType[index].Item1;
            Function = cardType[index].Item2;
            ImgIndex = cardType[index].Item3;
        }
        #endregion
    }
}
