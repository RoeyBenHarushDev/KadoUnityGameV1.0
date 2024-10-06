using UnityEngine;
using UnityEngine.EventSystems;

public class SwapZone : MonoBehaviour, IDropHandler
{
    public CardManager cardManager;

    private void Start()
    {
        cardManager = FindObjectOfType<CardManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject != null)
        {
            CardData cardData = droppedObject.GetComponent<CardData>();
            if (cardData != null && cardData.card.IsSpecial && cardData.card.SpecialType == SpecialCardType.DoubleSwap)
            {
                cardManager.ActivateDoubleSwap(droppedObject);
            }
        }
    }
}