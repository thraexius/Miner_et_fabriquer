using UnityEngine;

public class DemoScript : MonoBehaviour
{
    public InventoryManage inventoryManage;
    public Item[] itemsToPickup;

    public void PickupItem(int id){
        bool result = inventoryManage.AddItem(itemsToPickup[id]);
        if (result == true) {
            Debug.Log("Item Added");
        } else {
            Debug.Log("Item not added");
        }
    }   

    public void GetSelectedItem() {
    DragableItem receivedItem;
    inventoryManage.GetSelectedItem(out receivedItem, true);

    if (receivedItem != null) {
        Debug.Log("Selected Item: " + receivedItem.item.name);
    } else {
        Debug.Log("No item selected");
    }
}


}