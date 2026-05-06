using UnityEngine;

public class RANDOMBTNS : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    public void ReloadScene()
    {
        gameManager.ResetGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void CreateProfile()
    {
        FindFirstObjectByType<SceneAndUserMgt>().NewUser();
    }

    public void PauseOrResumeGame()
    {
        if (Time.timeScale == 1f)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
