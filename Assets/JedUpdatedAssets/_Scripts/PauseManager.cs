using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    public void UnPause()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }
    
    public void MainMenu() 
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("StartScreen");
    }
}
