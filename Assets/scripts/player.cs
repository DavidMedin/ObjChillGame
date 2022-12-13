using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    // Where all the network magic happens.
    public class player : NetworkBehaviour
    {
        private GameObject[] _players; 
        private ChillGrid _grid;
        private camera_controller _cam_ctrl;
        [SerializeField] private uint castle_distance; // How far away are the enemies apart?


        // private int whos_turn;
        
        #region Scene Initialization

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Now the game scene is open, so only now does _grid exist.
            _grid = GameObject.Find("grid").GetComponent<ChillGrid>();
            _cam_ctrl = GameObject.Find("Main Camera").GetComponent<camera_controller>();
            
            // Only ask if we own this.
            if (IsOwner) RequestInitServerRpc();
            else
            {
                var player_one_hex = new Hex{r = 0, q = 0, chunk = new Vector2Int(0, 0)};
                var player_two_hex = new Hex{r = 0, q = (int)castle_distance, chunk = new Vector2Int(0, 0)};
                player_two_hex.SnapToGrid();
                _players = GameObject.FindGameObjectsWithTag("Player");
                _players[0].transform.position = _grid.Hex2Global(player_one_hex);
                _players[1].transform.position = _grid.Hex2Global(player_two_hex);
            }
            
            print($"Done loading, I am {OwnerClientId}");
            GameObject.Find("net_turn_mngr").GetComponent<TurnManager>().OnTurnChange(1,0);
        }

        [ServerRpc]
        public void RequestInitServerRpc()
        {
            print($"Initializing client {OwnerClientId}");
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[]{OwnerClientId}
                }
            };
            var player_one_hex = new Hex{r = 0, q = 0, chunk = new Vector2Int(0, 0)};
            PlaceCellClientRpc(player_one_hex, Biome.castle, 1, 20, clientRpcParams);
            
            var player_two_hex = new Hex{r = 0, q = (int)castle_distance, chunk = new Vector2Int(0, 0)};
            player_two_hex.SnapToGrid();
            PlaceCellClientRpc(player_two_hex, Biome.castle, 0, 20, clientRpcParams);
            
            _players = GameObject.FindGameObjectsWithTag("Player");
            _players[0].transform.position = _grid.Hex2Global(player_one_hex);
            _players[1].transform.position = _grid.Hex2Global(player_two_hex);
            
            /*
            if (GameObject.ReferenceEquals(gameObject, _players[0]))
            {
                
            }
            if (GameObject.ReferenceEquals(gameObject, _players[1]))
            {
                player.Transform(_grid.HexToGlobal(player_two_hex.hex));
            }
            */
            
            DoneWithInitClientRpc();
        }

        [ClientRpc]
        public void DoneWithInitClientRpc()
        {
            print($"Done init, {OwnerClientId}");
            //GameObject.Find("net_turn_mngr").GetComponent<TurnManager>().OnTurnChange(1,0);
        }
        #endregion
        
        
        #region Place Cells
        [ServerRpc]
        public void RequestCellServerRpc(Hex hex,Biome biome,ServerRpcParams serverRpcParams = default)
        {
            // Will be executable by the server.
            print($"Client wants cell here {hex}");
            if (_grid.Get(hex) != null)
            {
                print("Rejecting client request for cell.");
                return;
            }
            
            PlaceCellClientRpc(hex,biome,serverRpcParams.Receive.SenderClientId);
        }
        
        [ClientRpc]
        public void PlaceCellClientRpc(Hex hex, Biome biome, ulong client_id, int troop_count=0,ClientRpcParams clientRpcParams=default)
        {
            print($"Placing tile from server, owned by {client_id}");
            // Will be executable by the client.
            _grid.CreateHex(hex, biome,client_id,troop_count);
            _cam_ctrl.TryHighlight();
        }
        #endregion

        #region Troop Movement

        [ServerRpc]
        public void RequestTroopMoveServerRpc(Hex src,Hex dest, int split,ServerRpcParams serverRpcParams = default)
        {
            print("Client wants to move troops!");
            // Check to see if the player can move the troops
            var src_obj = _grid.Get(src);
            if (src_obj == null)
            {
                print("Rejected : no src object.");
                return;
            }
            var src_cell = src_obj.GetComponent<Cell>();
            if (src_cell.owner_id != serverRpcParams.Receive.SenderClientId)
            {
                print($"Rejected : They ({serverRpcParams.Receive.SenderClientId}) tried to move troops they didn't own ({src_cell.owner_id}).");
                return;
            } // Invalid request.
            if (src_cell.Troops == 0)
            {
                print("Rejected : tried to move troops they didn't have.");
                return;
            }

            if (_grid.Get(dest) == null)
            {
                print("Rejected : no dest object.");
                return;
            }
            if (!_grid.Neighbors(src).Contains(dest))
            {
                print("Rejected : didn't move troops to a neighbor.");
                return;
            } // Return if it is not a neighbor of src.
            // Is valid!
            print("Moving troops!");
            MoveTroopsClientRpc(src,dest,split);
        }

        [ClientRpc]
        public void MoveTroopsClientRpc(Hex src, Hex dest, int split)
        {
            print("Received move troops command!");
            var src_obj = _grid.Get(src);
            var src_cell = src_obj.GetComponent<Cell>();

            var dest_obj = _grid.Get(dest);
            var dest_cell = dest_obj.GetComponent<Cell>();

            var move_count = split == 0 ? src_cell.Troops : (uint)split;
            Assert.IsTrue(move_count <= src_cell.Troops && move_count > 0); // Just checking.
            
            if (dest_cell.is_owned && dest_cell.owner_id != OwnerClientId)
            {
                // We are attacking this cell.
                //if (move_count < dest_cell.Troops)
                //{
                //    dest_cell.owner_id = dest_cell.owner_id;
                //}
                
                if (move_count > dest_cell.Troops)
                {
                    dest_cell.owner_id = src_cell.owner_id;
                    dest_cell.Troops = move_count - dest_cell.Troops;
                }
                else
                {
                    dest_cell.Troops -= move_count;
                }

            }
            else
            {
                dest_cell.owner_id = OwnerClientId;
                dest_cell.Troops += move_count;

            }

            src_cell.owner_id = OwnerClientId;
            src_cell.Troops -= move_count;
            _cam_ctrl.CancelTroopMove();
            
            // Prevent future moving this turn
            src_cell.has_moved = true;
            dest_cell.has_moved = true;
            
            //Check the number of troops on either side.
            var p1 = GetTroopCount(0);
            var p2 = GetTroopCount(1);
            GameObject.Find("net_turn_mngr").GetComponent<TurnManager>().GiveLoss(p1,p2);

            // Disable the splitting textbox.
            var indic = src_cell.Troop_Indic;
            var text_obj = indic.transform.GetChild(2);
            text_obj.gameObject.SetActive(false);
        }

        #endregion


        private int GetTroopCount(ulong clientID)
        {
            var count = 0;
            foreach (Cell cell in Cell.All_Cells)
            {
                if (cell.is_owned && cell.owner_id == clientID)
                {
                    count += (int)cell.Troops;
                }
            }

            return count;
        }
        
        public override void OnNetworkSpawn()
        {
            SceneManager.sceneLoaded += OnSceneLoaded; // load it up.
            //GameObject.Find("net_turn_mngr").GetComponent<TurnManager>().OnTurnChange(1,0);
        }

    }
}