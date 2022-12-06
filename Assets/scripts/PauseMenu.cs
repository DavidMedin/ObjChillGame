using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenu;

    void Update ()
    {
        //allows for the pause menu to be closed via esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
            
    }
    //pauses game
    public void Pause()
    {
        pauseMenu.SetActive(true);
        GameIsPaused = true;
    }
    //resume take
    public void Resume()
    {
        pauseMenu.SetActive(false);
        GameIsPaused = false;
    }
    //takes to menu
    public void Menu(int sceneID)
    {
        Debug.Log("Loading menu...");
        SceneManager.LoadScene(sceneID);


    }
    //quits game
    public void QuitGame()
    {
        Debug.Log("Quiting game...");
        Application.Quit();
    }
}
       
