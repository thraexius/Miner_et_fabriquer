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
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;
    [SerializeField] private InputActionReference lookActionReference;
    [SerializeField] private Transform playerBody;

    private Vector2 rotationAccumulator = Vector2.zero;

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
    }

    void OnEnable()
    {
        lookActionReference?.action.Enable();
    }

    void OnDisable()
    {
        lookActionReference?.action.Disable();
    }

    void Update()
    {
        if (lookActionReference == null || !lookActionReference.action.enabled || playerBody == null)
        {
            return;
        }

        Vector2 mouseInput = lookActionReference.action.ReadValue<Vector2>();

        rotationAccumulator.x += mouseInput.x * sensitivity;
        rotationAccumulator.y += mouseInput.y * sensitivity;
        rotationAccumulator.y = Mathf.Clamp(rotationAccumulator.y, -yRotationLimit, yRotationLimit);

        playerBody.localRotation = Quaternion.AngleAxis(rotationAccumulator.x, Vector3.up);
        transform.localRotation = Quaternion.AngleAxis(rotationAccumulator.y, Vector3.left);
    }

    public void EnableLook(bool enable)
    {
        if (lookActionReference != null && lookActionReference.action != null)
        {
            if (enable)
                lookActionReference.action.Enable();
            else
                lookActionReference.action.Disable();
        }
    }
}
