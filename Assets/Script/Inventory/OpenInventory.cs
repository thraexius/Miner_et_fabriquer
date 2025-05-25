using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryObject;
    public FirstPersonCameraRotation cameraController; // Référence au script de la caméra
    private bool isInventoryOpen = false;

    private void Start()
    {
        inventoryObject.SetActive(false);
    }

    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryObject.SetActive(isInventoryOpen);

            Time.timeScale = isInventoryOpen ? 0f : 1f;

            if (cameraController != null)
            {
                cameraController.EnableLook(!isInventoryOpen); // Désactive la rotation de la caméra quand l'inventaire est ouvert
            }
        }
    }
}
