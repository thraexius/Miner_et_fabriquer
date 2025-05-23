using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input Actions")]
    [Tooltip("Référence à l'action d'input pour le mouvement (doit être de type Vector2).")]
    [SerializeField] private InputActionReference moveActionReference;
    [Tooltip("Référence à l'action d'input pour le saut (doit être de type Button).")]
    [SerializeField] private InputActionReference jumpActionReference; // NOUVEAU

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float jumpHeight = 1.8f; // NOUVEAU : Hauteur souhaitée du saut

    private CharacterController characterController;
    private Vector3 horizontalVelocity;
    private Vector3 verticalVelocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("Le composant CharacterController est introuvable sur cet objet.", this);
            enabled = false;
            return;
        }

        if (moveActionReference == null || moveActionReference.action == null)
        {
            Debug.LogError("La référence à l'action 'Move' (moveActionReference) n'est pas configurée dans l'Inspecteur !", this);
            enabled = false;
            return;
        }

        // NOUVEAU : Vérification pour l'action de saut
        if (jumpActionReference == null || jumpActionReference.action == null)
        {
            Debug.LogError("La référence à l'action 'Jump' (jumpActionReference) n'est pas configurée dans l'Inspecteur !", this);
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        if (moveActionReference != null && moveActionReference.action != null)
        {
            moveActionReference.action.Enable();
        }
        // NOUVEAU : Activer l'action de saut
        if (jumpActionReference != null && jumpActionReference.action != null)
        {
            jumpActionReference.action.Enable();
        }
    }

    void OnDisable()
    {
        if (moveActionReference != null && moveActionReference.action != null)
        {
            moveActionReference.action.Disable();
        }
        // NOUVEAU : Désactiver l'action de saut
        if (jumpActionReference != null && jumpActionReference.action != null)
        {
            jumpActionReference.action.Disable();
        }
    }

    void Update()
    {
        bool isGrounded = characterController.isGrounded;

        HandleHorizontalMovement();
        HandleVerticalMovement(isGrounded); // Passe isGrounded à la méthode

        Vector3 finalMovement = (horizontalVelocity + verticalVelocity) * Time.deltaTime;
        characterController.Move(finalMovement);
    }

    private void HandleHorizontalMovement()
    {
        if (moveActionReference == null || !moveActionReference.action.enabled)
        {
            horizontalVelocity = Vector3.zero;
            return;
        }

        Vector2 inputVector = moveActionReference.action.ReadValue<Vector2>();
        Vector3 moveDirection = transform.forward * inputVector.y + transform.right * inputVector.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }
        horizontalVelocity = moveDirection * moveSpeed;
    }

    // MODIFIÉ pour inclure la logique de saut
    private void HandleVerticalMovement(bool isGrounded)
    {
        // Si le joueur est au sol
        if (isGrounded)
        {
            // Si la vélocité verticale est négative (tombe ou stable), la réinitialiser.
            // Cela évite l'accumulation de gravité quand on est au sol.
            if (verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f; // Petite force vers le bas pour coller au sol
            }

            // NOUVEAU : Gérer le saut
            // .triggered est vrai pour le frame où le bouton est pressé
            if (jumpActionReference != null && jumpActionReference.action.triggered)
            {
                // Formule pour atteindre une hauteur de saut spécifique : v = sqrt(h * -2 * g)
                // On utilise -2f car gravityValue est négatif.
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            }
        }

        // Appliquer la gravité constamment (sauf si on vient de sauter et qu'on est au sol,
        // la vélocité du saut prendra le dessus pour ce frame)
        verticalVelocity.y += gravityValue * Time.deltaTime;
    }
}
