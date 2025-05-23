using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Ajout du namespace Input System

public class DragableItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Image image;
    [HideInInspector] public Transform parentAfterDrag;
    private InputAction mousePositionAction; // Déclaration de l'action d'entrée

    private void Awake()
    {
        // Configuration de l'action Input System
        var inputActionAsset = new InputActionMap("DragActions");
        mousePositionAction = inputActionAsset.AddAction("MousePosition", binding: "<Mouse>/position");
        mousePositionAction.Enable();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Drag started");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false; // Désactiver le raycast sur l'image pour éviter les interférences
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>(); // Récupérer la position de la souris via Input System
        transform.position = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Drag ended");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true; // Réactiver le raycast sur l'image
    }
}
