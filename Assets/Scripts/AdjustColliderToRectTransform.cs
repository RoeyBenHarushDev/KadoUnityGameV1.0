using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AdjustColliderToRectTransform : MonoBehaviour
{
    private RectTransform rectTransform;
    private BoxCollider2D boxCollider;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        boxCollider = GetComponent<BoxCollider2D>();

        AdjustCollider();
    }

    void AdjustCollider()
    {
        // ממירים את הגודל של RectTransform למידות של BoxCollider2D
        boxCollider.size = rectTransform.rect.size;
        boxCollider.offset = rectTransform.rect.center;
    }
}
