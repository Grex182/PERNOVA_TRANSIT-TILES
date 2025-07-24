using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CardsMovement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private RectTransform designatedSlot;
    [SerializeField] private RectTransform rectTransform;

    private Vector3 originalLocalScale;
    private Vector2 originalAnchoredPosition; // Stores slot's original position
    private int originalSiblingIndex;

    [Header("Movement Settings")]
    [SerializeField] private readonly float moveSpeed = 1f;
    [SerializeField] private readonly float hoverYOffset = 100f;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetSlot(GameObject slot)
    {
        designatedSlot = slot.GetComponent<RectTransform>();

        originalAnchoredPosition = designatedSlot.anchoredPosition;
        originalLocalScale = rectTransform.localScale;
        originalSiblingIndex = rectTransform.GetSiblingIndex();

        // Snap to slot position initially
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }

    #region POINTER
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        Vector2 hoverPosition = new Vector2(
            originalAnchoredPosition.x,
            originalAnchoredPosition.y + hoverYOffset
        );

        activeCoroutine = StartCoroutine(DoHover(hoverPosition));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(DoHover(originalAnchoredPosition));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
    #endregion

    #region DRAG
    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {

    }
    #endregion

    private IEnumerator DoHover(Vector2 targetPosition)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        float duration = 0.15f; // Adjust for speed
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;

        //rectTransform.anchoredPosition = targetPosition;
        //rectTransform.localScale = originalLocalScale;
        //rectTransform.SetSiblingIndex(originalSiblingIndex);
    }

    IEnumerator DoHover()
    {
        //Vector2 startPos = rectTransform.anchoredPosition;
        //Vector2 targetPos = startPos + moveOffset;

        //float t = 0;
        //while (t < 1)
        //{
        //    t += Time.deltaTime * moveSpeed;
        //    rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
        //    yield return null;
        //}

        yield return null;
    }

    private bool IsAtDesignatedSlot()
    {
        return true;
    }

    public void RemoveCard()
    {
        HandManager handManager = GetComponentInParent<HandManager>();
        handManager.OnCardRemoved(designatedSlot.gameObject);
        Destroy(gameObject);
    }
}
