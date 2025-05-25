using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public FirstPersonCameraRotation cameraController; // Référence au script de la caméra
    private bool isPaused = false;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
    }

    public void OnPauseToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (cameraController != null)
        {
            cameraController.EnableLook(!isPaused); // Désactive la caméra en pause
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        if (cameraController != null)
        {
            cameraController.EnableLook(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
