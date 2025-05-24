using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManage : MonoBehaviour
{
    public int maxStackItems = 4;
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;

    int selectedSlot = -1;

    private void Start() {
        ChangeSelectedSlot(0);
    }

    private void Update() {
    var keyboard = Keyboard.current;
    
    if (keyboard.digit1Key.wasPressedThisFrame) ChangeSelectedSlot(0);
    else if (keyboard.digit2Key.wasPressedThisFrame) ChangeSelectedSlot(1);
    else if (keyboard.digit3Key.wasPressedThisFrame) ChangeSelectedSlot(2);
    else if (keyboard.digit4Key.wasPressedThisFrame) ChangeSelectedSlot(3);
    else if (keyboard.digit5Key.wasPressedThisFrame) ChangeSelectedSlot(4);
    else if (keyboard.digit6Key.wasPressedThisFrame) ChangeSelectedSlot(5);
    else if (keyboard.digit7Key.wasPressedThisFrame) ChangeSelectedSlot(6);
    }

    void ChangeSelectedSlot(int newValue) {
        if(selectedSlot >= 0) {
            inventorySlots[selectedSlot].Deselected();
        }
        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }

    public bool AddItem(Item item) {
        for (int i = 0; i < inventorySlots.Length; i++){
            InventorySlot slot = inventorySlots[i];
            DragableItem itemInSlot = slot.GetComponentInChildren<DragableItem>();
            if (itemInSlot != null &&
                itemInSlot.item == item &&
                itemInSlot.count < maxStackItems &&
                itemInSlot.item.stackable == true) {

                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        
        }
        
        for (int i = 0; i < inventorySlots.Length; i++){
            InventorySlot slot = inventorySlots[i];
            DragableItem itemInSlot = slot.GetComponentInChildren<DragableItem>();
            if (itemInSlot == null) {
                SpawnNewItem(item, slot);
                return true;
            }
        
        }
        return false;
    }

    void SpawnNewItem(Item item, InventorySlot slot) {
        GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        DragableItem dragableItem = newItem.GetComponent<DragableItem>();
        dragableItem.InitialiseItem(item);
    }

    public void GetSelectedItem(out DragableItem item, bool use) {
    if (selectedSlot < 0 || selectedSlot >= inventorySlots.Length) {
        item = null;
        return;
    }

    InventorySlot slot = inventorySlots[selectedSlot];
    item = slot.GetComponentInChildren<DragableItem>();

    if (item != null && use) {
        UseItem(item);
    }
}

private void UseItem(DragableItem item) {
    if (item.count > 0) {
        item.count--;
        item.RefreshCount(); // Met à jour l'affichage du nombre d'items
        Debug.Log(item.item.name + " utilisé. Quantité restante : " + item.count);

        if (item.count == 0) {
            Destroy(item.gameObject);
            Debug.Log(item.item.name + " a été supprimé !");
        }
    } else {
        Debug.Log(item.item.name + " n'est plus disponible.");
    }
}




}
