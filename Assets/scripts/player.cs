using System;
using Unity.Netcode;
using UnityEngine;

namespace DefaultNamespace
{
    public class player : NetworkBehaviour
    {
        // public override void OnNetworkSpawn()
        // {
        //     if (!IsOwnedByServer)
        //     {
        //         print("Client connected!");
        //         
        //         GameObject.Find("Connect Menu").SetActive(false);
        //     }
        //     base.OnNetworkSpawn();
        // }
        private void Start()
        {
            // NetworkManager.Singleton.OnClientConnectedCallback += OnClientEntered;
        }


    }
}