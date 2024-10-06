using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private int originalIndex;
    private RectTransform parentRectTransform;
    private HorizontalLayoutGroup layoutGroup;

    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float maxRotation = 360f;
    [SerializeField] private float throwHeight = 100f;
    [SerializeField] private float landingBounceScale = 1.2f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Check if the canvas is available in the parent
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return; // Return early if no canvas is found to avoid null reference errors later.
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        parentRectTransform = transform.parent?.GetComponent<RectTransform>();
        if (parentRectTransform == null)
        {
            Debug.LogError("Parent RectTransform not found.");
        }

        layoutGroup = transform.parent?.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
        {
            Debug.LogError("HorizontalLayoutGroup not found in parent.");
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalIndex = transform.GetSiblingIndex();
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
        
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        GameObject overlappingCard = FindOverlappingCard();
        
        if (overlappingCard != null)
        {
            int newIndex = overlappingCard.transform.GetSiblingIndex();
            StartCoroutine(SwapCards(newIndex));
        }
        else
        {
            StartCoroutine(ReturnToOriginalPosition());
        }

        if (layoutGroup != null)
        {
            layoutGroup.enabled = true;
        }
    }

    private GameObject FindOverlappingCard()
    {
        foreach (Transform child in parentRectTransform)
        {
            if (child == transform) continue;

            RectTransform childRect = child.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(childRect, rectTransform.position, canvas.worldCamera))
            {
                return child.gameObject;
            }
        }
        return null;
    }

    private IEnumerator SwapCards(int newIndex)
    {
        GameObject otherCard = parentRectTransform.GetChild(newIndex).gameObject;
        Vector2 otherCardPosition = otherCard.GetComponent<RectTransform>().anchoredPosition;

        transform.SetSiblingIndex(newIndex);
        otherCard.transform.SetSiblingIndex(originalIndex);

        yield return StartCoroutine(AnimateCardMovement(rectTransform, otherCardPosition));
        yield return StartCoroutine(AnimateCardMovement(otherCard.GetComponent<RectTransform>(), originalPosition));
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        yield return StartCoroutine(AnimateCardMovement(rectTransform, originalPosition));
    }

    private IEnumerator AnimateCardMovement(RectTransform cardRect, Vector2 targetPosition)
    {
        Vector2 startPosition = cardRect.anchoredPosition;
        float startRotation = cardRect.rotation.eulerAngles.z;
        float targetRotation = startRotation + maxRotation;
        Vector3 startScale = cardRect.localScale;

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;

            // Easing function for smooth start and end
            float easeT = EaseOutBack(t);

            // Calculate position with a throw and land effect
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, easeT);
            float yOffset = Mathf.Sin(t * Mathf.PI) * throwHeight;
            newPosition.y += yOffset;

            // Rotate the card
            float newRotation = Mathf.Lerp(startRotation, targetRotation, t);

            // Scale animation for landing effect
            float scaleMultiplier = 1f;
            if (t > 0.8f)
            {
                float landT = (t - 0.8f) / 0.2f;
                scaleMultiplier = 1f + Mathf.Sin(landT * Mathf.PI) * (landingBounceScale - 1f);
            }

            cardRect.anchoredPosition = newPosition;
            cardRect.rotation = Quaternion.Euler(0, 0, newRotation);
            cardRect.localScale = startScale * scaleMultiplier;

            yield return null;
        }

        // Ensure final position, rotation and scale are exact
        cardRect.anchoredPosition = targetPosition;
        cardRect.rotation = Quaternion.identity;
        cardRect.localScale = startScale;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
}