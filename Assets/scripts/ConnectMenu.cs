using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectMenu : MonoBehaviour
{
    static bool ConnMenuIsOpen = false;

    public GameObject connectMenu;

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
            connect();
        }
    }
    //opens connect menu
    public void OpenConnMenu()
    {
        connectMenu.SetActive(true);
        Time.timeScale = 0f;
        ConnMenuIsOpen = true;
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
    //does funny ip connection thing (Aiden do this)
    public void connect()
    {
        Time.timeScale = 0f;
    }
}


