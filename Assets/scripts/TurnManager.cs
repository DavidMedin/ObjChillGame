using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EasyButtons;
using Unity.Netcode;
//using EasyButtons;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class TurnManager : NetworkBehaviour
    {
    
        private NetworkVariable<ulong> whos_turn = new NetworkVariable<ulong>(0);


        [SerializeField] private GameObject _camera;
        [SerializeField] private GameObject _stacker;
        [SerializeField] private GameObject _endTurnButton;

        [SerializeField] private GameObject win;
        [SerializeField] private GameObject lose;
        //void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        //{
        //    // _endTurnButton = GameObject.Find("End Turn Button");  
        //}
        
        #region Turn Request

        [ServerRpc(RequireOwnership = false)]
        public void RequestEndOfTurnServerRpc(ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            print($"Client {clientId} requested end of turn!");
            if (whos_turn.Value != clientId)
            {
                print("Rejected : it is not their turn.");
                return;
            }

            print($"Current turn is: {whos_turn.Value}");
            whos_turn.Value = (clientId + 1) % 2;
            print($"Turn changed to: {whos_turn.Value}");
            //print($"turn changed, was {whos_turn.Value}, is -> {OwnerClientId} -> {(OwnerClientId + 1) % 2}");
        }
        
        public void OnTurnChange(ulong prev, ulong now)
        {
            // Add troops to enemy castles
            ResetTurn(now);
            print("got turn change");
            if (NetworkManager.LocalClient.ClientId == now)
            {
                print("It is my turn");
                // Show the end turn button
                _endTurnButton.SetActive(true);
                // reset the stack of tiles
                _stacker.GetComponent<Stacker>().Repopulate();
                // enable tile placement and troop movement
                _camera.GetComponent<camera_controller>().is_enabled = true;
            }

            if (NetworkManager.LocalClient.ClientId == prev)
            {
                print("it is not my turn anymore.");
                // disable end turn button
                _endTurnButton.SetActive(false);
                // disable tile and troop movement
                _camera.GetComponent<camera_controller>().is_enabled = false;
            }
        }

        
        #endregion
                

        public void TryNewTurn()
        {
            print("Sending end turn request");
            RequestEndOfTurnServerRpc();
        }
        
        //Yes, I hate this. This should be the Cell class, but in order to be called from
        // a UnityEvent, the function must attached to a GameObject. And I'm not about
        // to make a dummy cell object to sit around.
        public void ResetTurn(ulong dest)
        {
            print($"Adding troops to client {dest}");
            // Reset the 'has_moved' member of all cells.
            foreach (Cell cell in Cell.All_Cells)
            {
                if (cell.is_owned && cell.owner_id == dest)
                {
                    cell.NewTurn();
                }
            }
        }

        [Button]
        public void GiveLoss(int p1,int p2)
        {
            // if (p1 == 0 || p2 == 0)
            // {
            //     if (p2 == 0 && 1 == NetworkManager.LocalClientId)
            //     {
            //         // I lost
            //         lose.SetActive(true);
            //     }
            //     else
            //     {
            //         // I won
            //         win.SetActive(true);
            //     }
            // }
            if (0 == NetworkManager.LocalClientId)
            {
                // I am player 1
                if(p1 == 0) lose.SetActive(true);
                else if(p2 == 0) win.SetActive(true);
            }else if (1 == NetworkManager.LocalClientId)
            {
                if(p1 == 0) win.SetActive(true);
                if(p2 == 0) lose.SetActive(true);
            }
        }
        
        // //https://github.com/madsbangh/EasyButtons Very nice.
        // [Button]
        // public void NewTurn()
        // {
        //     new_turn.Invoke();
        // }
        
        //public void Start()

        // void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        // {
        //     
        //     var cam_obj = GameObject.Find("Main Camera");
        //     _cam_ctrl = cam_obj.GetComponent<camera_controller>();
        //     _stacker = GameObject.Find("stack").GetComponent<Stacker>();
        //
        // }
        
        public override void OnNetworkSpawn()
        {
            whos_turn.OnValueChanged += OnTurnChange;
            // SceneManager.sceneLoaded += OnSceneLoaded; // load it up.
            //TurnManager.OnTurnChange(1,0);

            print("Done with turn mngr start()");
        }
    }
}