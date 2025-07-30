using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardMachine : MonoBehaviour
{
    public bool isAvailable;
    public Transform cardPos;
    [SerializeField] private Image _machineSign;
    [SerializeField] private Image _machineLight;
    [SerializeField] private TextMeshProUGUI _topSign;
    [SerializeField] private TextMeshProUGUI _price;
    private int _priceInt;
    [SerializeField] public Button buyButton;

    [SerializeField] private Color[] machineColor = new Color[5];
    

    

    public void BuyOutCard()
    {
        ClearOutCards();
        buyButton.interactable = false;
        isAvailable= false;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[5], false);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[2], false);
        
        _machineSign.color = machineColor[0];
        _machineLight.color = new Color(0f, 0f, 0f, 0f);
        _price.text = "";
        _topSign.text = "Out of Service";
        _priceInt = 0;
    }

    public void SetUpCardDisplay(CardsData.CardRarity rarity, int price)
    {
        if (cardPos.transform.GetChild(cardPos.transform.childCount-1) != null)
        {
            Color signColor = machineColor[0];
            switch(rarity)
            {
                case CardsData.CardRarity.Common:
                default:
                    signColor = machineColor[1];
                    break;
                case CardsData.CardRarity.Uncommon:
                    signColor = machineColor[2];
                    break;
                case CardsData.CardRarity.Rare:
                    signColor = machineColor[3];
                    break;
                case CardsData.CardRarity.Epic:
                    signColor = machineColor[4];
                    break;
            }
            _machineSign.color = signColor;
            _machineLight.color = new Color(1f, 1f, 1f, 1f);
            _topSign.text = "In Service Mode";
            buyButton.interactable = true;
            _price.text = price.ToString();
            _priceInt = price;

        }

        
    }

    public void ClearOutCards()
    {
        foreach (Transform child in cardPos)
        {
            Destroy(child.gameObject);
        }
        _machineSign.color = machineColor[0];
        _machineLight.color = new Color(0f, 0f, 0f, 0f);
        _price.text = "";
        _topSign.text = "Out of Service";
        _priceInt = 0;
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => BuyOutCard());
    }

    public void Update()
    {
        int cash = LevelManager.Instance != null ? LevelManager.Instance.earnedStars : TutorialManager.Instance.earnedStars;

        if (_priceInt <= cash && _priceInt != 0)
        {
            buyButton.interactable = true;
        }
        else
        {
            buyButton.interactable = false;
        }
    }
}
