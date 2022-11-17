using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using Color = UnityEngine.Color;

namespace DefaultNamespace
{

    public struct Hex
    {
        public int r;
        public int q;
    }
    
    // An Axial Hexagon grid. (Look at this : https://www.redblobgames.com/grids/hexagons/)
    public class ChillGrid : MonoBehaviour
    {
        [SerializeField] private float _cell_size;

        // These are rhombus shaped chunks in the world.
        private static int _chunk_size = 20;
        private Dictionary<Vector2Int, GameObject[,]> _chunks;

        // Magic, do not ask.
        private static Vector2 q_basis = new Vector2((float)Math.Sqrt(3), 0);
        private static Vector2 r_basis = new Vector2((float)Math.Sqrt(3)/2, (float)3/2);

        public GameObject grid_prefab;
        public Vector3 Hex2Global(Vector2Int chunk, Hex hex)
        {
            var new_hex = new Hex
            {
                q = hex.q + chunk.x * _chunk_size,
                r = hex.r + chunk.y * _chunk_size
            };
            return new Vector3
            {
                x = _cell_size  * ( q_basis.x * new_hex.q + r_basis.x * new_hex.r ),
                z = _cell_size * (r_basis.y * new_hex.r)
            };
        }

        public (Vector2Int, Hex) Global2Hex(Vector2 point)
        {
            var vec2 = new Vector2((float)Math.Sqrt(3)/3 * point.x - (float)1/3 * point.y, (float)2/3 * point.y) / _cell_size;

            var axial_hex = AxialRound(vec2);
            vec2.x = axial_hex.q;
            vec2.y = axial_hex.r;
            var chunk = (vec2 / _chunk_size);
            var chunk_i = new Vector2Int((int)Math.Floor(chunk.x), (int)Math.Floor(chunk.y));
            vec2 -= chunk_i * _chunk_size;

            return (chunk_i, new Hex{q=(int)vec2.x,r=(int)vec2.y});
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
        public GameObject CreateHex(Vector2Int chunk_i, Hex hex)
        {
            // Add a hexagon to the map.
            var thing = Hex2Global(chunk_i, hex);
            var game_obj = Instantiate(grid_prefab, thing, Quaternion.Euler(-90,0,0));

            GameObject[,] chunk;
            if (!_chunks.ContainsKey(chunk_i))
            {
                // Create this boi.
                print($"Loading new chunk! {chunk_i.x} {chunk_i.y}");
                chunk = new GameObject[_chunk_size,_chunk_size];
                _chunks[chunk_i] = chunk;
            }
            else
            {
                chunk = _chunks[chunk_i];
            }
            
            // Add the GameObject into this chunk.
            // This will overwrite!
            chunk[hex.r, hex.q] = game_obj;
            return game_obj;
        }
        
        private void Start()
        {
            // Create the first chunk.
            _chunks = new Dictionary<Vector2Int, GameObject[,]>();
            _chunks[new Vector2Int(0,0)] = new GameObject[_chunk_size,_chunk_size];
            
            // Create a lot of items in this grid.
            // const int gen = 20;
            // for(var x=0; x < gen; x++)
            // {
            //     for (var z = 0; z < gen; z++)
            //     {
            //         CreateHex(new Vector2Int(0,0), new Hex {r=x, q=z});
            //     }
            // }
            // for(var x=0; x < gen; x++)
            // {
            //     for (var z = 0; z < gen; z++)
            //     {
            //         var obj = CreateHex(new Vector2Int(1,0), new Hex {r=x, q=z});
            //         obj.GetComponent<Renderer>().material.SetColor("_Color",Color.blue);
            //     }
            // }
            // for(var x=0; x < gen; x++)
            // {
            //     for (var z = 0; z < gen; z++)
            //     {
            //         var obj = CreateHex(new Vector2Int(0,1), new Hex {r=x, q=z});
            //         obj.GetComponent<Renderer>().material.SetColor("_Color",Color.red);
            //     }
            // }
            // for(var x=0; x < gen; x++)
            // {
            //     for (var z = 0; z < gen; z++)
            //     {
            //         var obj = CreateHex(new Vector2Int(1,1), new Hex {r=x, q=z});
            //         obj.GetComponent<Renderer>().material.SetColor("_Color",Color.cyan);
            //     }
            // }
        }
    }
}