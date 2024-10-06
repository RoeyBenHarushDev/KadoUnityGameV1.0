using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedCard = eventData.pointerDrag; // קלף שנשבר
        if (droppedCard != null && droppedCard.CompareTag("PlayerCard"))
        {
            // העבר את הקלף לאזור הנכון
            droppedCard.transform.SetParent(transform);
            // עדכון המיקום של הקלף
            droppedCard.transform.position = transform.position; // אפשר לשנות בהתאם
        }
    }
}
