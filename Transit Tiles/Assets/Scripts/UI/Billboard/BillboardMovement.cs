using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BillboardMovement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private LightingManager lightingManager;

    [Header("Movement Settings")]
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float hoverHeight = 250f;
    [SerializeField] private RectTransform billboardRect;
    private bool isHovered = false;
    private bool hasStartedHover = false;
    [SerializeField] private float hoverTimer;
    private float hoverTime;

    [SerializeField] private GameObject designatedSlot;
    [SerializeField] private RectTransform dropDownArrow;

    [Header("Billboard Text")]
    [SerializeField] private TMP_Text dayCount;
    [SerializeField] private TMP_Text dayWord;
    [SerializeField] private string[] days = new string[7]
    {
        "SUNDAY",
        "MONDAY",
        "TUESDAY",
        "WEDNESDAY",
        "THURSDAY",
        "FRIDAY",
        "SATURDAY"
    };
    [SerializeField] private TMP_Text timeHour;
    [SerializeField] private TMP_Text timeMinute;
    [SerializeField] private TMP_Text AmPm;

    [Header("Message Board")]
    [SerializeField] private TMP_Text messageBoard;
    [SerializeField] private string[] offPeakMessages;
    [SerializeField] private string[] rushHourMessages;
    [SerializeField] float timerMessage = 10f;
    [SerializeField] Color colorRush;
    [SerializeField] Color colorOff;
    private bool isRushHourPhase = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("On Pointer Enter");
        isHovered = true;
        hasStartedHover = true;
        //StartCoroutine(DoHover(designatedSlot.transform.position.y + hoverHeight));
    }

    private void Start()
    {
        ChangeMessage(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("On Pointer Exit");
        isHovered = false;
        //StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        while (billboardRect.anchoredPosition.y <= 0)
        {
            billboardRect.anchoredPosition += new Vector2(0, Time.deltaTime * 200f);
            yield return null;
        }

        /*
        while (Vector3.Distance(transform.position, designatedSlot.transform.position) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, designatedSlot.transform.position, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = designatedSlot.transform.position;
        */
    }

    IEnumerator DoHover(float targetY)
    {
        while (billboardRect.anchoredPosition.y > -hoverHeight)
        {
            float dist = ((billboardRect.anchoredPosition.y + hoverHeight) * hoverSpeed) + 1f;
            billboardRect.anchoredPosition -= new Vector2(0,Time.deltaTime * dist);
            yield return null;
        }
        
    }

    public void DoRushHourWarning()
    {
        isHovered = true;
        hasStartedHover = true;
    }

    private void Update()
    {
        SetTime(lightingManager.TimeOfDay);
        doMessageBoard();

        if (isHovered || hasStartedHover)
        {
            if (billboardRect.anchoredPosition.y > -hoverHeight)
            {
                float dist = ((billboardRect.anchoredPosition.y + hoverHeight) * hoverSpeed) + 1f;
                billboardRect.anchoredPosition -= new Vector2(0, Time.deltaTime * dist);
                
            }
            else
            {
                billboardRect.anchoredPosition = new Vector2(0, -hoverHeight);
                hasStartedHover = false;
            }
            hoverTime = 0;
        }
        else
        {
            if (hoverTime < hoverTimer)
            {
                hoverTime += Time.deltaTime;
            }
            else if (billboardRect.anchoredPosition.y < 0)
            {
                float dist = ((billboardRect.anchoredPosition.y + hoverHeight) * returnSpeed) + 1f;
                billboardRect.anchoredPosition += new Vector2(0, Time.deltaTime * dist);
            }
            else
            {
                billboardRect.anchoredPosition = new Vector2(0, 0);
            }
        }

        float dropDist = (Mathf.Abs(billboardRect.anchoredPosition.y) / hoverHeight) * 70f;
        dropDownArrow.anchoredPosition = new Vector2(0, dropDist);

    }

    public void SetDay(int day)
    {
        //Setting Day Number
        string dayNumber = day.ToString("000");

        dayCount.text = dayNumber;

        //Setting Day Text
        int dayText = day % 7;

        dayWord.text = days[dayText];
    }

    private void SetTime(float time)
    {
        AmPm.text = time < 12f ? "AM" : "PM";

        int hour = Mathf.FloorToInt(time);
        int minute = Mathf.FloorToInt((time - hour) * 60);

        if (hour > 12)
        { hour -= 12; }
        if (hour == 0)
        { hour = 12; }

        timeHour.text = hour.ToString("00");   // Format with leading zero if needed
        timeMinute.text = minute.ToString("00");


    }

    private void doMessageBoard()
    {
        if (timerMessage > 0f)
        {
            timerMessage -= Time.deltaTime;
        }
        else
        {
            ChangeMessage(isRushHourPhase);
        }
    }

    public void ChangeMessage(bool isRushHour)
    {
        string[] chosenString = isRushHour ? rushHourMessages : offPeakMessages;
        Color chosenColor = isRushHour ? colorRush : colorOff;

        int stringIndex = Random.Range(0, chosenString.Length);

        messageBoard.text = chosenString[stringIndex];
        messageBoard.color = chosenColor;

        if (isRushHour)
        {
            DoRushHourWarning();
        }

        timerMessage = Random.Range(5f, 20f);
        isRushHourPhase = isRushHour;

    }
}
