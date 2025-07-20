using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Cards : MonoBehaviour
{
    [SerializeField] private Sprite[] _rarityImgs;
    [SerializeField] private Sprite[] _cardImgs;
    [SerializeField] private Color[] _rarityColors;

    [SerializeField] private TextMeshProUGUI _cardRarity;
    [SerializeField] private Image _rarityImage;
    [SerializeField] private Image _accentColor;

    [SerializeField] private TextMeshProUGUI _cardName;
    [SerializeField] private TextMeshProUGUI _cardFunction;
    [SerializeField] private Image _cardImage;



    /*  private void Start()
        {
            CardsData.CardData randomCard = new CardsData.CardData();
            Initialize(randomCard);

            CardsData.Instance.cardsList.Remove(randomCard);
        }
    */

    public void Initialize(CardsData.CardInfo cardInfo)
    {
        _cardRarity.text = cardInfo.cardRarity;
        _rarityImage.sprite = _rarityImgs[cardInfo.rarityImgIndex];
        _accentColor.color = _rarityColors[cardInfo.rarityImgIndex];
        _cardName.text = cardInfo.cardName;
        _cardFunction.text = cardInfo.cardFunction;
        _cardImage.sprite = _cardImgs[cardInfo.cardImgIndex];
    }

    
}
