using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectMenu : MonoBehaviour
{
    static bool ConnMenuIsOpen = false;

    public GameObject connectMenu;
    public GameObject try_connect_menu;
    public GameObject wait_menu;
    public GameObject ip_text_box;
    
    
    private string IPinput;

    // Update is called once per frame
    void Update ()
    {
        //allows for the connection menu to be closed via esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ConnMenuIsOpen)
            {
                CloseConnMenu();
            }
           
        }
        if (Input.GetKeyDown(KeyCode.Return) && ConnMenuIsOpen)
        {
            TryConnectToServer();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            OpenConnMenu();
            TryConnectToServer();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartServer();
        }
    }
    //opens connect menu
    public void OpenConnMenu()
    {
        connectMenu.SetActive(true);
        Time.timeScale = 0f;
        ConnMenuIsOpen = true;
    }
    
    public void StartServer()
    {
        wait_menu.SetActive(true);
        NetworkManager.Singleton.StartHost();
        print("Starting host");
    }

    public void TryConnectToServer()
    {
        try_connect_menu.SetActive(true);
        
        // Get the user's string input.
        var address = ip_text_box.GetComponent<TMP_InputField>().text;
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = address.Trim();
        NetworkManager.Singleton.StartClient();
    }
    //closes connect menu
    public void CloseConnMenu()
    {
        connectMenu?.SetActive(false);
        Time.timeScale = 1f;
        ConnMenuIsOpen = false;
    }
     //reads ip
    public void ReadStringInput(string IP)
    {
        IPinput = IP;
        Debug.Log(IPinput);
    }   
}


