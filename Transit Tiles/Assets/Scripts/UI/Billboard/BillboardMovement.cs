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


    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("On Pointer Enter");
        StartCoroutine(DoHover(designatedSlot.transform.position.y + hoverHeight));
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
        SetDay(lightingManager.Day);
    }

    public void SetDay(int day)
    {
        //Setting Day Number
        string dayNumber = day.ToString();

        int zeroLength = 3 - dayNumber.Length;

        string addZero = "";
        for (int i = 0; i < zeroLength; i++)
        {
            addZero += "0";
        }

        addZero += dayNumber;

        dayCount.text = addZero;

        //Setting Day Text
        int dayText = day % 7;

        dayWord.text = days[dayText];
    }

    private void SetTime(float time)
    {
        if (time < 12f)
        {
            AmPm.text = "woop";
        }
    }
}
