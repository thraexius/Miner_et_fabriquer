using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragableItem dragableItem = eventData.pointerDrag.GetComponent<DragableItem>();
            if (dragableItem != null)
            {
                dragableItem.parentAfterDrag = transform;
                Debug.Log("Parent set to: " + transform.name);
            }
            else
            {
                Debug.LogError("Dragged object does not have DragableItem script.");
            }
        }
        else
        {
            Debug.LogError("Dropped object is null.");
        }
    }
}