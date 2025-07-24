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
    [SerializeField] private float hoverHeight = 1.1f;

    [SerializeField] private GameObject designatedSlot;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("On Pointer Enter");
        StartCoroutine(DoHover(designatedSlot.transform.position.y + hoverHeight));
    }

    private void Start()
    {
        ChangeMessage(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("On Pointer Exit");
        StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        while (Vector3.Distance(transform.position, designatedSlot.transform.position) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, designatedSlot.transform.position, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = designatedSlot.transform.position;
    }

    IEnumerator DoHover(float targetY)
    {
        Vector3 currentPosition = designatedSlot.transform.position;
        Vector3 targetPosition = new Vector3(currentPosition.x, targetY, currentPosition.z);

        while (Vector2.Distance(currentPosition, targetPosition) > 0.1f)
        {
            float newY = Mathf.Lerp(currentPosition.y, targetY, hoverSpeed * Time.deltaTime);

            transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
            currentPosition = transform.position;
            yield return null;
        }
    }

    private void Update()
    {
        SetTime(lightingManager.TimeOfDay);
        doMessageBoard();

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
            ChangeMessage(false);
        }
    }

    public void ChangeMessage(bool isRushHour)
    {
        string[] chosenString = isRushHour ? rushHourMessages : offPeakMessages;
        Color chosenColor = isRushHour ? colorRush : colorOff;

        int stringIndex = Random.Range(0, chosenString.Length);

        messageBoard.text = chosenString[stringIndex];
        messageBoard.color = chosenColor;

        timerMessage = Random.Range(5f, 20f);
    }
}
