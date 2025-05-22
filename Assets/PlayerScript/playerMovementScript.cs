using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private InputActionReference crouchActionReference;
    [SerializeField] private InputActionReference sprintActionReference; // NOUVEAU

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float jumpHeight = 1.8f;

    [Header("Sprint Settings")] // NOUVELLE SECTION
    [SerializeField] private float sprintSpeedMultiplier = 1.75f; // Ex: vitesse * 1.75 en sprintant
    [Tooltip("Le joueur doit-il avancer pour pouvoir sprinter ?")]
    [SerializeField] private bool mustBeMovingForwardToSprint = true;
    [Tooltip("Le joueur peut-il sprinter en l'air ?")]
    [SerializeField] private bool canSprintInAir = false;


    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeightTarget = 1.9f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float crouchJumpHeight = 1.1f;
    [Tooltip("Le Transform de la caméra principale du joueur (doit être un enfant de cet objet Player).")]
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private float crouchTransitionSpeed = 15f;

    private CharacterController characterController;
    private Vector3 horizontalVelocity;
    private Vector3 verticalVelocity;

    private float standingHeight;
    private Vector3 standingControllerCenter;
    private Vector3 standingCameraLocalPosition;

    private float currentTargetControllerHeight;
    private Vector3 currentTargetControllerCenter;
    private Vector3 currentTargetCameraLocalPosition;

    private bool isCrouching = false;
    private bool isSprinting = false; // NOUVEAU: État actuel du sprint
    private Vector2 currentMoveInput; // Pour vérifier la direction du mouvement

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null) { Debug.LogError("CharacterController manquant !", this); enabled = false; return; }

        standingHeight = characterController.height;
        standingControllerCenter = characterController.center;

        if (playerCameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.transform.IsChildOf(transform)) { playerCameraTransform = mainCam.transform; }
            else { Debug.LogWarning("PlayerCameraTransform non assigné. L'ajustement de la caméra pourrait ne pas être optimal.", this); }
        }
        if (playerCameraTransform != null) { standingCameraLocalPosition = playerCameraTransform.localPosition; }
        else { standingCameraLocalPosition = Vector3.up * (standingHeight * 0.45f); }

        currentTargetControllerHeight = standingHeight;
        currentTargetControllerCenter = standingControllerCenter;
        currentTargetCameraLocalPosition = standingCameraLocalPosition;

        // Vérifications des actions d'input
        if (moveActionReference == null || moveActionReference.action == null) { Debug.LogError("MoveActionReference non configurée !", this); enabled = false; return; }
        if (jumpActionReference == null || jumpActionReference.action == null) { Debug.LogError("JumpActionReference non configurée !", this); enabled = false; return; }
        if (crouchActionReference == null || crouchActionReference.action == null) { Debug.LogError("CrouchActionReference non configurée !", this); enabled = false; return; }
        if (sprintActionReference == null || sprintActionReference.action == null) { Debug.LogError("SprintActionReference non configurée !", this); enabled = false; return; } // NOUVEAU
    }

    void OnEnable()
    {
        moveActionReference.action.Enable();
        jumpActionReference.action.Enable();
        crouchActionReference.action.Enable();
        sprintActionReference.action.Enable(); // NOUVEAU

        crouchActionReference.action.performed += OnCrouchPerformed;
        crouchActionReference.action.canceled += OnCrouchCanceled;

        // S'abonner aux événements de l'action de sprint
        sprintActionReference.action.performed += OnSprintStarted; // Quand la touche est pressée
        sprintActionReference.action.canceled += OnSprintCanceled; // Quand la touche est relâchée
    }

    void OnDisable()
    {
        moveActionReference.action.Disable();
        jumpActionReference.action.Disable();
        crouchActionReference.action.Disable();
        sprintActionReference.action.Disable(); // NOUVEAU

        crouchActionReference.action.performed -= OnCrouchPerformed;
        crouchActionReference.action.canceled -= OnCrouchCanceled;

        sprintActionReference.action.performed -= OnSprintStarted;
        sprintActionReference.action.canceled -= OnSprintCanceled;

        if (isCrouching)
        {
            isCrouching = false;
            SetTargetCrouchState(false);
            characterController.height = currentTargetControllerHeight;
            characterController.center = currentTargetControllerCenter;
            if (playerCameraTransform != null) playerCameraTransform.localPosition = currentTargetCameraLocalPosition;
        }
        isSprinting = false; // S'assurer que le sprint est désactivé
    }

    void Update()
    {
        // Lire l'input de mouvement ici pour y avoir accès partout dans Update
        currentMoveInput = moveActionReference.action.ReadValue<Vector2>();
        bool isGrounded = characterController.isGrounded;

        // Mettre à jour l'état de sprint basé sur les conditions
        UpdateSprintState(isGrounded);

        HandleCrouchTransitions();
        HandleHorizontalMovement();
        HandleVerticalMovement(isGrounded);

        Vector3 finalMovement = (horizontalVelocity + verticalVelocity) * Time.deltaTime;
        characterController.Move(finalMovement);
    }

    // --- Gestion du Sprint ---
    private void OnSprintStarted(InputAction.CallbackContext context)
    {
        // On ne fait que noter l'intention de sprinter ici.
        // L'état réel de `isSprinting` sera géré dans UpdateSprintState.
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false; // Arrêter de sprinter immédiatement si la touche est relâchée
    }

    private void UpdateSprintState(bool isGrounded)
    {
        // Le joueur essaie-t-il activement de sprinter (bouton maintenu) ?
        bool wantsToSprint = sprintActionReference.action.IsPressed();

        if (wantsToSprint)
        {
            // Conditions pour pouvoir effectivement sprinter
            bool canSprint = !isCrouching; // Ne pas sprinter si accroupi

            if (!canSprintInAir && !isGrounded) // Si on ne peut pas sprinter en l'air et qu'on n'est pas au sol
            {
                canSprint = false;
            }

            if (mustBeMovingForwardToSprint && currentMoveInput.y <= 0.1f) // Si on doit avancer et qu'on n'avance pas (ou recule/va sur le côté sans avancer)
            {
                canSprint = false;
            }

            // Si on a un input de mouvement significatif (pour éviter de sprinter sur place si on ne bouge pas du tout)
            if (currentMoveInput.sqrMagnitude < 0.01f)
            {
                canSprint = false;
            }

            isSprinting = canSprint;
        }
        else
        {
            isSprinting = false; // Si le bouton n'est pas pressé, on ne sprinte pas
        }
    }


    // --- Gestion de l'Accroupissement ---
    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        if (!isCrouching) { isCrouching = true; SetTargetCrouchState(true); }
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        if (isCrouching) { isCrouching = false; SetTargetCrouchState(false); }
    }

    private void SetTargetCrouchState(bool crouch)
    {
        if (crouch)
        {
            currentTargetControllerHeight = crouchHeightTarget;
            float centerOffsetY = (standingHeight - crouchHeightTarget) / 2.0f;
            currentTargetControllerCenter = new Vector3(standingControllerCenter.x, standingControllerCenter.y - centerOffsetY, standingControllerCenter.z);
            if (playerCameraTransform != null) { currentTargetCameraLocalPosition = new Vector3(standingCameraLocalPosition.x, standingCameraLocalPosition.y - centerOffsetY, standingCameraLocalPosition.z); }
        }
        else
        {
            currentTargetControllerHeight = standingHeight;
            currentTargetControllerCenter = standingControllerCenter;
            if (playerCameraTransform != null) { currentTargetCameraLocalPosition = standingCameraLocalPosition; }
        }
    }

    private void HandleCrouchTransitions()
    {
        characterController.height = Mathf.Lerp(characterController.height, currentTargetControllerHeight, Time.deltaTime * crouchTransitionSpeed);
        characterController.center = Vector3.Lerp(characterController.center, currentTargetControllerCenter, Time.deltaTime * crouchTransitionSpeed);
        if (playerCameraTransform != null) { playerCameraTransform.localPosition = Vector3.Lerp(playerCameraTransform.localPosition, currentTargetCameraLocalPosition, Time.deltaTime * crouchTransitionSpeed); }
    }

    // --- Gestion du Mouvement ---
    private void HandleHorizontalMovement()
    {
        if (moveActionReference == null || !moveActionReference.action.enabled) { horizontalVelocity = Vector3.zero; return; }

        // currentMoveInput est déjà lu dans Update()
        Vector3 moveDirection = transform.forward * currentMoveInput.y + transform.right * currentMoveInput.x;

        if (moveDirection.sqrMagnitude > 1f) moveDirection.Normalize();

        float currentActualSpeed = moveSpeed;

        if (isSprinting) // Priorité au sprint s'il est actif
        {
            currentActualSpeed *= sprintSpeedMultiplier;
        }
        else if (isCrouching) // Sinon, vérifier l'accroupissement
        {
            currentActualSpeed *= crouchSpeedMultiplier;
        }
        // Si ni sprint ni accroupi, currentActualSpeed reste moveSpeed

        horizontalVelocity = moveDirection * currentActualSpeed;
    }

    private void HandleVerticalMovement(bool isGrounded)
    {
        if (isGrounded)
        {
            if (verticalVelocity.y < 0) verticalVelocity.y = -2f;

            if (jumpActionReference != null && jumpActionReference.action.triggered)
            {
                float currentActualJumpHeight = isCrouching ? crouchJumpHeight : jumpHeight;
                verticalVelocity.y = Mathf.Sqrt(currentActualJumpHeight * -2f * gravityValue);
            }
        }
        verticalVelocity.y += gravityValue * Time.deltaTime;
    }
}
