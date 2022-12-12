using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class network_scene_manager : NetworkBehaviour
    {
        // Define the singleton
        public static network_scene_manager Singleton;
        
        // Parameters

        
    #if UNITY_EDITOR
            public UnityEditor.SceneAsset SceneAsset;
            private void OnValidate()
            {
                if (SceneAsset != null)
                {
                    _game_scene = SceneAsset.name;
                }
            }
    #endif
        [SerializeField]private string _game_scene = "GameScene";
        // This will not run for clients.
        private void OnClientEntered(ulong clientId)
        {
            if (clientId != NetworkManager.LocalClient.ClientId)
            {
                print("Client is cool, and here.");
                var status = NetworkManager.SceneManager.LoadScene(_game_scene, LoadSceneMode.Single);
                if (status != SceneEventProgressStatus.Started)
                {
                    Debug.LogWarning($"Failed to load {_game_scene} " +
                                     $"with a {nameof(SceneEventProgressStatus)}: {status}");
                }

                // StartGame();
            }
            else
            {
                print("I am here.");
            }
        }

        [ServerRpc]
        public void InitializeClientServerRpc(ServerRpcParams serverRpcParams= default)
        {
            
        }

        // This function is the initialization of the world.
        // private void StartGame()
        // {
        //     PlaceCellClientRpc(new Hex{r=0,q=0,chunk=new Vector2Int(0,0)} , Biome.castle);
        //     
        //     var player_two_hex = new Hex { r = 0, q = (int)castle_distance, chunk = new Vector2Int(0, 0) };
        //     player_two_hex.SnapToGrid();
        //     PlaceCellClientRpc( player_two_hex, Biome.castle);
        // }

        

        public override void OnNetworkSpawn()
        {
            if (Singleton == null) Singleton = this;
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback += OnClientEntered;
                print("registered event.");
            }

            print("Done with scene mngr start()");
        }
        private void Start()
        {
            
        }
    }
}