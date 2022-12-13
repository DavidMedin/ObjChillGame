using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject wait_menu;
    public void WaitMenu()
    {
        wait_menu.SetActive(true);
        NetworkManager.Singleton.StartHost();
        print("Starting host");
    }
    //starts game (fun things for aiden)
    public void StartGame(int sceneID)
    {
        // SceneManager.LoadScene(sceneID);
        Debug.Log("Loading Game...");
    }    

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game...");
    }
}
