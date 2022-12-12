using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

namespace DefaultNamespace
{

    public struct Hex : INetworkSerializable
    {
        public Vector2Int chunk;
        public int r;
        public int q;

        public void SnapToGrid()
        {
            var vec = new Vector2(q, r);
            var chunk_mount = vec / ChillGrid.ChunkSize;
            var chunk_int = new Vector2( (int)Math.Floor (chunk_mount.x), (int)Math.Floor(chunk_mount.y));
            vec -= chunk_int * ChillGrid.ChunkSize;
            chunk += new Vector2Int((int)chunk_int.x,(int)chunk_int.y);
            
            q = (int)vec.x;
            r = (int)vec.y;
        }

        public bool IsOrigin()
        {
            return this == new Hex { r = 0, q = 0, chunk = new Vector2Int(0, 0) };
        }

        public static bool operator ==(Hex a, Hex b)
        {
            return a.chunk.x == b.chunk.x && a.chunk.y == b.chunk.y && a.r == b.r && a.q == b.q;
        }
        public static bool operator !=(Hex a, Hex b)
        {
            return a.chunk.x != b.chunk.x || a.chunk.y != b.chunk.y || a.r != b.r || a.q != b.q;
        }

        // To send over network.
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref chunk);
            serializer.SerializeValue(ref r);
            serializer.SerializeValue(ref q);
        }
    }
    
    // An Axial Hexagon grid. (Look at this : https://www.redblobgames.com/grids/hexagons/)
    public class ChillGrid : MonoBehaviour
    {
        [SerializeField] private float _cell_size;

        // These are rhombus shaped chunks in the world.
        public static int ChunkSize = 20;
        private Dictionary<Vector2Int, GameObject[,]> _chunks;

        // Magic, do not ask.
        private static Vector2 q_basis = new Vector2((float)Math.Sqrt(3), 0);
        private static Vector2 r_basis = new Vector2((float)Math.Sqrt(3)/2, (float)3/2);

        public GameObject cell_prefab;
        public Vector3 Hex2Global(Hex hex)
        {
            var new_hex = new Hex
            {
                q = hex.q + hex.chunk.x * ChunkSize,
                r = hex.r + hex.chunk.y * ChunkSize
            };
            return new Vector3
            {
                x = _cell_size  * ( q_basis.x * new_hex.q + r_basis.x * new_hex.r ),
                z = _cell_size * (r_basis.y * new_hex.r)
            };
        }

        [CanBeNull]
        public GameObject Get(Hex hex)
        {
            try
            {
                return _chunks[hex.chunk][hex.q, hex.r];
                
            }
            catch
            {
                return null;
            }
        }
        public Hex Global2Hex(Vector2 point)
        {
            var vec2 = new Vector2((float)Math.Sqrt(3)/3 * point.x - (float)1/3 * point.y, (float)2/3 * point.y) / _cell_size;

            var axial_hex = AxialRound(vec2);
            vec2.x = axial_hex.q;
            vec2.y = axial_hex.r;
            var chunk = (vec2 / ChunkSize);
            var chunk_i = new Vector2Int((int)Math.Floor(chunk.x), (int)Math.Floor(chunk.y));
            vec2 -= chunk_i * ChunkSize;

            return new Hex { chunk = chunk_i, q = (int)vec2.x, r = (int)vec2.y };
        }

        public List<Hex> Neighbors(Hex hex)
        {
            var list = new List<Hex>
            {
                new Hex{q=hex.q+1,r=hex.r,chunk=hex.chunk},
                new Hex{q=hex.q-1,r=hex.r,chunk=hex.chunk},
                new Hex{q=hex.q,r=hex.r+1,chunk=hex.chunk},
                new Hex{q=hex.q,r=hex.r-1,chunk=hex.chunk},
                new Hex{q=hex.q+1,r=hex.r-1,chunk=hex.chunk},
                new Hex{q=hex.q-1,r=hex.r+1,chunk=hex.chunk}
            };

            for (int i = 0; i < list.Count; i++)
            {
                var what = list[i];
                what.SnapToGrid();
                list[i] = what; // Kill me.
            }

            return list;
        }

        // Input fractional axial space vector and return a Hex.
        private Hex AxialRound(Vector2 point)
        {
            var q = Math.Round(point.x);
            var r = Math.Round(point.y);
            var s = Math.Round(-q - r);

            var q_diff = Math.Abs(q - point.x);
            var r_diff = Math.Abs(r - point.y);
            var s_diff = s;
            if (q_diff > r_diff && q_diff > s_diff)
            {
                q = -r - s;
            }else if (r_diff > s_diff)
            {
                r = -q - s;
            }

            return new Hex { q = (int)q, r = (int)r };
        }
        public GameObject CreateHex(Hex hex,Biome biome,ulong client_id)
        {
            // Add a hexagon to the map.
            var thing = Hex2Global(hex);
            var game_obj = Instantiate(cell_prefab, thing, Quaternion.Euler(-90,0,0));

            GameObject[,] chunk;
            if (!_chunks.ContainsKey(hex.chunk))
            {
                // Create this boi.
                chunk = new GameObject[ChunkSize,ChunkSize];
                _chunks[hex.chunk] = chunk;
            }
            else
            {
                chunk = _chunks[hex.chunk];
            }
            
            // Add the GameObject into this chunk.
            // This will overwrite!
            var cell = game_obj.GetComponent<Cell>();
            cell.Biome = biome;
            cell.hex = hex;
            cell.owner_id = client_id;
            chunk[hex.q, hex.r] = game_obj;
            return game_obj;
        }

        private void Awake()
        {
            _chunks = new Dictionary<Vector2Int, GameObject[,]>();
            _chunks[new Vector2Int(0,0)] = new GameObject[ChunkSize,ChunkSize];

        }

        private void Start()
        {
            // Create the first chunk.


            // var start = CreateHex(new Hex { r = 0, q = 0, chunk = new Vector2Int(0, 0) }, Biome.castle);
            // start.GetComponent<Cell>().Troop_Count = 20;
            //
            // var enemy_hex = new Hex { q = 0, r = (int)castle_distance, chunk = new Vector2Int(0, 0) };
            // enemy_hex.SnapToGrid(); // I love you, SnapToGrid.
            // var enemy_obj = CreateHex(enemy_hex, Biome.castle);
            // enemy_obj.GetComponent<Cell>().Troop_Count = 20;
        }
    }
}