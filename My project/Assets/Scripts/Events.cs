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

    public void Level()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadLevelByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void SetGameObjectActive(GameObject go)
    {
        if (go != null) go.SetActive(true);
    }

    public void SetGameObjectInactive(GameObject go)
    {
        if (go != null) go.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}