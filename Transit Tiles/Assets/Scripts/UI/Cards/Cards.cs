using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Cards : MonoBehaviour
{
    [SerializeField] private Sprite[] _rarityImgs;
    [SerializeField] private Sprite[] _cardImgs;
    [SerializeField] private Color[] _rarityColors;

    [SerializeField] private CardType _cardType;
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
        _cardType = cardInfo.cardType;
        _cardRarity.text = cardInfo.cardRarity;
        _rarityImage.sprite = _rarityImgs[cardInfo.rarityImgIndex];
        _accentColor.color = _rarityColors[cardInfo.rarityImgIndex];
        _cardName.text = cardInfo.cardName;
        _cardFunction.text = cardInfo.cardFunction;
        _cardImage.sprite = _cardImgs[cardInfo.cardImgIndex];
    }

    public void DoEffect()
    {
        switch (_cardType)
        {
            case CardType.FloorSweeper:
                ApplyFloorSweeper(); break;

            case CardType.CaffeineHit:
                ApplyCaffeineHit(); break;

            case CardType.FilipinoTime:
                ApplyFilipinoTime(); break;

            case CardType.Deodorant:
                ApplyDeodorant(); break;

            case CardType.PatrollingGuard:
                ApplyPatrollingGuard(); break;

            case CardType.ChillBeats:
                ApplyChillBeats(); break;

            case CardType.Sukistar:
                ApplySukiStar(); break;

            case CardType.ExcuseMePo:
                ApplyExcuseMePo(); break;

            case CardType.RushHourRegulars:
                ApplyRushHourRegulars(); break;
        }
    }

    #region CARD EFFECTS
    private void ApplyFloorSweeper()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ClearTrash();
        }
        Debug.Log("Floor Sweeper Activated");
    }

    private void ApplyCaffeineHit()
    {
        Transform trainParent = PassengerSpawner.Instance.trainParent.transform;
        Debug.Log($"Train Parent: {trainParent.name}");
        for (int i = 0; i < trainParent.childCount; i++)
        {
            Transform child = trainParent.GetChild(i);
            PassengerData data = child.GetComponent<PassengerData>();

            if (data.traitType == PassengerTrait.Sleepy)
            {
                data.animator.SetBool("IsSleepy", false);
                data.sleepyEffectRig.SetActive(false);
                data.isAsleep = false;
                data.hasCaffeine = true;
            }
        }

        Debug.Log("Caffeine Hit Activated");
    }

    private void ApplyFilipinoTime()
    {
        if (LevelManager.Instance != null )
        {
            LevelManager.Instance.hasFilipinoTimeEffect = true;
        }
        
        Debug.Log("Filipino Time Activated");
    }

    private void ApplyDeodorant()
    {
        Transform trainParent = PassengerSpawner.Instance.trainParent.transform;

        for (int i = 0; i < trainParent.childCount; i++)
        {
            Transform child = trainParent.GetChild(i);
            PassengerData data = child.GetComponent<PassengerData>();

            if (data.traitType == PassengerTrait.Stinky)
            {
                data.hasNegativeAura = false;
                data.stinkyEffectRig.SetActive(false);
            }
        }

        Debug.Log("Deodorant Activated");
    }

    private void ApplyPatrollingGuard()
    {
        Transform trainParent = PassengerSpawner.Instance.trainParent.transform;

        for (int i = 0; i < trainParent.childCount; i++)
        {
            Transform child = trainParent.GetChild(i);
            PassengerData data = child.GetComponent<PassengerData>();

            if (data.traitType == PassengerTrait.Noisy)
            {
                data.hasNegativeAura = false;
                data.noisyEffectRig.SetActive(false);
            }
        }

        Debug.Log("Patrolling Guard Activated");
    }

    private void ApplyChillBeats()
    {
        Transform trainParent = PassengerSpawner.Instance.trainParent.transform;

        for (int i = 0; i < trainParent.childCount; i++)
        {
            Transform child = trainParent.GetChild(i);
            PassengerData data = child.GetComponent<PassengerData>();

            data.ChangeMoodValue(1);
        }

        Debug.Log("Chill Beats Activated");
    }

    private void ApplySukiStar()
    {
        LevelManager.Instance.DoSukiStar(2);
        Debug.Log("Suki Star Activated");
    }

    private void ApplyExcuseMePo()
    {
        LevelManager.Instance.hasExcuseMePo = true;
        Debug.Log("Excuse Me Po Activated");
    }

    private void ApplyRushHourRegulars()
    {

        Debug.Log("Rush Hour Regulars Activated");
    }
    #endregion

    #region
    public CardType GetCardType()
    {
        return _cardType;
    }
    #endregion
}
