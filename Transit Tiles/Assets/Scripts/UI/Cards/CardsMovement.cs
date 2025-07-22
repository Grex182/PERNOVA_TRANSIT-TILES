using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CardsMovement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Movement Settings")]
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float hoverHeight = 1.1f;

    private GameObject designatedSlot;
    private Vector3 originalScale;
    private int originalSiblingIndex;

    [Header("Flags")]
    private bool isDragging = false;
    public bool isSelected = false;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform targetArea;
    [SerializeField] private RectTransform rectTransform;

    private Coroutine activeCoroutine;

    private void Start()
    {
        originalScale = transform.localScale;
        originalSiblingIndex = transform.GetSiblingIndex();
        isDragging = false;
        isSelected = false;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetSlot(GameObject slot)
    {
        designatedSlot = slot;
    }

    #region POINTER
    public void OnPointerDown(PointerEventData eventData)
    {
        if (LevelManager.Instance.currState != MovementState.Card) { return; }
        
        isSelected = true;
        transform.localScale *= 1.1f;

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClips[2], false); // Play selection sound
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isSelected = false;
        transform.localScale = originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected || isDragging || !IsAtDesignatedSlot()) { return; }

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(DoHover(designatedSlot.transform.position.y * hoverHeight));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected || isDragging) { return; }

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(ReturnToOriginalPosition());
    }
    #endregion

    #region DRAG
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (LevelManager.Instance.currState != MovementState.Card)
        {
            if (isDragging)
            {
                isDragging = false;
                StartCoroutine(ReturnToOriginalPosition());
            }
            return; 
        }

        GetComponent<CanvasGroup>().blocksRaycasts = false;
        isDragging = true;
        transform.SetParent(transform.parent.parent);
        transform.SetAsLastSibling(); // Bring to front while dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        isDragging = false;
        StartCoroutine(ReturnToOriginalPosition());

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (RectTransformUtility.RectangleContainsScreenPoint(targetArea, Input.mousePosition, eventData.pressEventCamera))
        {
            rectTransform.anchoredPosition = targetArea.anchoredPosition;
        }
    }
    #endregion

    private IEnumerator ReturnToOriginalPosition()
    {
        transform.SetParent(designatedSlot.transform);
        transform.SetSiblingIndex(originalSiblingIndex);

        while (Vector3.Distance(transform.position, designatedSlot.transform.position) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, designatedSlot.transform.position, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = designatedSlot.transform.position;
        transform.localScale = originalScale;
    }

    IEnumerator DoHover(float targetY)
    {
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = new Vector3(currentPosition.x, targetY, currentPosition.z);

        while (Vector2.Distance(currentPosition, targetPosition) > 0.1f)
        {
            float newY = Mathf.Lerp(currentPosition.y, targetY, hoverSpeed * Time.deltaTime);

            transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
            currentPosition = transform.position;
            yield return null;
        }
    }

    private bool IsAtDesignatedSlot()
    {
        if (designatedSlot == null) return false;
        return Vector3.Distance(transform.position, designatedSlot.transform.position) < 0.1f;
    }

    public void RemoveCard()
    {
        HandManager handManager = GetComponentInParent<HandManager>();
        handManager.OnCardRemoved(designatedSlot);
        Destroy(gameObject);
    }
}
