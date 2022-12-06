using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    //starts game (fun things for aiden)
    public void StartGame(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
        Debug.Log("Loading Game...");
    }    

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game...");
    }
}
