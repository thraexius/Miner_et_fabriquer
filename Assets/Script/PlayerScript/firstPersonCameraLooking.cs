// Dans votre script FirstPersonCameraRotation.cs

using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraRotation : MonoBehaviour
{
    public float Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }
    [Range(0.01f, 1f)][SerializeField] float sensitivity = 0.1f;
    [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;

    [Tooltip("Référence à l'action d'input pour le regard.")]
    [SerializeField] private InputActionReference lookActionReference;

    // AJOUT : Référence au Transform du corps du joueur (l'objet parent de la caméra)
    [Tooltip("Le Transform du corps du joueur à faire pivoter horizontalement.")]
    [SerializeField] private Transform playerBody;

    private Vector2 rotationAccumulator = Vector2.zero; // Renommé pour plus de clarté

    void Awake()
    {
        if (lookActionReference == null || lookActionReference.action == null)
        {
            Debug.LogError("La référence à l'action 'Look' n'est pas configurée !", this);
            enabled = false;
            return;
        }
        if (playerBody == null)
        {
            // Essayer de trouver le parent si non assigné et que ce script est sur la caméra
            if (transform.parent != null)
            {
                playerBody = transform.parent;
                Debug.LogWarning("PlayerBody non assigné, tentative d'utiliser le parent: " + playerBody.name, this);
            }
            else
            {
                Debug.LogError("PlayerBody n'est pas assigné dans l'Inspecteur pour FirstPersonCameraRotation et aucun parent trouvé!", this);
                enabled = false;
                return;
            }
        }

        // Optionnel: Cacher et verrouiller le curseur
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void OnEnable()
    {
        if (lookActionReference != null && lookActionReference.action != null)
        {
            lookActionReference.action.Enable();
        }
    }

    void OnDisable()
    {
        if (lookActionReference != null && lookActionReference.action != null)
        {
            lookActionReference.action.Disable();
        }
    }

    void Update()
    {
        if (lookActionReference == null || !lookActionReference.action.enabled || playerBody == null)
        {
            return;
        }

        Vector2 mouseInput = lookActionReference.action.ReadValue<Vector2>();

        // rotationAccumulator.x est pour la rotation horizontale (yaw) du corps du joueur
        rotationAccumulator.x += mouseInput.x * sensitivity;

        // rotationAccumulator.y est pour la rotation verticale (pitch) de la caméra
        rotationAccumulator.y += mouseInput.y * sensitivity;
        rotationAccumulator.y = Mathf.Clamp(rotationAccumulator.y, -yRotationLimit, yRotationLimit);

        // Appliquer la rotation horizontale (yaw) au corps du joueur
        playerBody.localRotation = Quaternion.AngleAxis(rotationAccumulator.x, Vector3.up);

        // Appliquer la rotation verticale (pitch) à la caméra (ce script)
        // Si vous avez corrigé l'inversion avant, c'était -rotation.y, donc ici -rotationAccumulator.y
        // Si "souris vers le haut = regarder vers le haut" est désiré, et mouseInput.y est positif pour haut:
        transform.localRotation = Quaternion.AngleAxis(rotationAccumulator.y, Vector3.left);
        // Si vous aviez changé pour `Quaternion.AngleAxis(rotation.y, Vector3.left);` pour la correction d'inversion,
        // alors utilisez `Quaternion.AngleAxis(rotationAccumulator.y, Vector3.left);` ici.
        // Testez pour obtenir le bon sens. La ligne ci-dessus suppose que mouseInput.y positif = souris vers le haut.
    }
}
