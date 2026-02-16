using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("StartScreen");
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
