using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Cards : MonoBehaviour
{
    [SerializeField] private Sprite[] _cardImgs;
    [SerializeField] private TextMeshProUGUI _cardName;
    [SerializeField] private TextMeshProUGUI _cardFunction;
    [SerializeField] private Image _cardImage;

    private void Start()
    {
        CardsData.CardData randomCard = new CardsData.CardData();
        Initialize(randomCard);
    }

    private void Initialize(CardsData.CardData cardData)
    {
        _cardName.text = cardData.Name;
        _cardFunction.text = cardData.Function;
        _cardImage.sprite = _cardImgs[cardData.ImgIndex];
    }
}
