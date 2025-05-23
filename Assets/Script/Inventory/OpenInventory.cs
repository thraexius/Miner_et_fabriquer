using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryObject; // Référence à l'empty object contenant l'inventaire
    private bool isInventoryOpen = false;

    private void Start()
    {
        // Assurez-vous que l'inventaire est bien caché au départ
        inventoryObject.SetActive(false);
    }

    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if (context.performed) // Vérifie si l'action a été exécutée
        {
            isInventoryOpen = !isInventoryOpen; // Bascule l'état
            inventoryObject.SetActive(isInventoryOpen); // Affiche ou cache l'empty object
        }
    }
}
