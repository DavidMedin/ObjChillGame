using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class player : NetworkBehaviour
    {
        
        private ChillGrid _grid;
        [SerializeField] private uint castle_distance; // How far away are the enemies apart?

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Now the game scene is open, so only now does _grid exist.
            _grid = GameObject.Find("grid").GetComponent<ChillGrid>();
            RequestInitServerRpc();
            print("Done loading!");
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestInitServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[]{serverRpcParams.Receive.SenderClientId}
                }
            };
            PlaceCellClientRpc(new Hex{r=0,q=0,chunk=new Vector2Int(0,0)} , Biome.castle,serverRpcParams.Receive.SenderClientId,clientRpcParams);
            
            var player_two_hex = new Hex { r = 0, q = (int)castle_distance, chunk = new Vector2Int(0, 0) };
            player_two_hex.SnapToGrid();
            PlaceCellClientRpc( player_two_hex, Biome.castle,serverRpcParams.Receive.SenderClientId,clientRpcParams);
        }
        
        [ServerRpc]
        public void RequestCellServerRpc(Hex hex,Biome biome,ServerRpcParams serverRpcParams = default)
        {
            // Will be executable by the server.
            print($"Client wants cell here ${hex}");
            if (_grid.Get(hex) != null)
            {
                print("Rejecting client request for cell.");
                return;
            }
            
            PlaceCellClientRpc(hex,biome,serverRpcParams.Receive.SenderClientId);
        }
        
        [ClientRpc]
        public void PlaceCellClientRpc(Hex hex, Biome biome, ulong client_id,ClientRpcParams clientRpcParams=default)
        {
            // Will be executable by the client.
            print($"Server wants cell here ${hex}");
            _grid.CreateHex(hex, biome,client_id);
        }

        public override void OnNetworkSpawn()
        {
            SceneManager.sceneLoaded += OnSceneLoaded; // load it up.
        }

    }
}