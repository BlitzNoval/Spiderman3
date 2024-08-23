using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject controlsPanel;

    // Starts the game by loading the next scene in the build index
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Toggle the visibility of the controls panel
    public void ToggleControls()
    {
        // Ensure controlsPanel is assigned
        if (controlsPanel != null)
        {
            // Toggle the active state of the controlsPanel
            controlsPanel.SetActive(!controlsPanel.activeSelf);
        }
        else
        {
            Debug.LogWarning("Controls panel is not assigned!");
        }
    }

    // Quits the game
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}