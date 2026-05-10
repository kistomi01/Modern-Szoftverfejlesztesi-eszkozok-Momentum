using UnityEngine;
using UnityEngine.SceneManagement;

public class Events : MonoBehaviour
{
    public void Menu()
    {
        SceneManager.LoadScene(0);
    }

    public void Continue()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadNextLevel();
        }
    }

    public void Retry()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.RetryLevel();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}