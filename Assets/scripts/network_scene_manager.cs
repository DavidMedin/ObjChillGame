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
    }
}