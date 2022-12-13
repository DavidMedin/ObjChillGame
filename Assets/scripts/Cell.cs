using System;
using System.Collections.Generic;
using UnityEngine;
using DefaultNamespace;
using EasyButtons;
using TMPro;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum Biome
{
    forest=0,
    town=1,
    plains,
    castle,
    
}


public class Cell : MonoBehaviour
{
    private static List<Cell> _all_cells = new List<Cell>();
    
    // How many troops are generated for every castle at the beginning of every turn?
    private const int troops_per_turn = 4;
    public static List<Cell> All_Cells
    {
        get => _all_cells;
    }

    [SerializeField]
    private Biome _biome;

    [SerializeField] private GameObject _prefab_troop_indic;

    // Troop state
    public bool has_moved=false;
    private uint _troop_count = 0;
    private GameObject _troop_indic;
    public GameObject Troop_Indic
    {
        get => _troop_indic;
    }
    public uint Troops
    {
        set
        {
            if (_troop_count == 0 && value > 0)
            {
                Assert.AreEqual(_troop_indic,null);
                // If there is no troop indicator, create one.
                _troop_indic = Instantiate(_prefab_troop_indic, transform.position, Quaternion.identity);
                is_owned = true;
            }else if (value == 0 && _troop_count > 0)
            {
                // If there was troops, and now there isn't, KILL.
                Destroy(_troop_indic);
                is_owned = false;
            }
            _troop_count = value;

            // Modify the text.
            if (_troop_count > 0)
            {
                var trans = _troop_indic.GetComponent<Transform>();
                trans.GetChild(1).GetComponent<TMP_Text>().text = _troop_count.ToString();
            }
        }
        get => _troop_count;
    }

    public bool is_owned = false;
    public ulong owner_id; // if is_owned is false, ignore this value.

    public Biome Biome
    {
        set
        {
            _biome = value;
            Rebuild_Mesh();
        }
        get => _biome;
    }

    [SerializeField] private GameObject[] prefabs;
    public Hex hex;

    private bool _is_highlighted = false;
    [SerializeField] private Material[] materials;
    public Material Target_Material
    {
        get
        {
            if (is_owned)
            {
                return materials[owner_id];
            }
            else
            {
                return materials[2];
            }
        }
    }

    public void ToggleHighlight()
    {
        var renderer = GetComponent<Renderer>();
        if (_is_highlighted)
        {
            // turn off highlight
            renderer.material = Target_Material;
        }else{
            // turn on highlight    
            renderer.material = materials[3];
        }

        _is_highlighted = !_is_highlighted;
    }
    
    // [Button]
    // void AddTroop(uint count=1)
    // {
    //     Troops = count;
    // }

    // This should be called at the beginning of every frame.
    //TurnManager.cs should use Cell's All_Cells list to call NewTurn.
    public void NewTurn()
    {
        has_moved = false;
        if (_biome == Biome.castle && _troop_count > 0) Troops += troops_per_turn;
    }
    
    public void Start()
    {
        _all_cells.Add(this);
    }

    // Is temperary. Should not be used in the final game!
    // hex_pos is the position of the hexagon game object. (should be the center of it).
    // private Vector3 GetBiomePos(Vector3 hex_pos, int side)
    // {
    //     return (hex_pos + new Vector3 { y = 0.5f + 0.25f/2 }) +
    //            new Vector3
    //            {
    //                x = (float)Math.Sin(Mathf.Deg2Rad * 30 + Mathf.Deg2Rad*60*side), // Flipped Sine and Cosine so that the
    //                z = (float)Math.Cos(Mathf.Deg2Rad * 30  + Mathf.Deg2Rad*60*side) // 0 degrees is at the Z axis.
    //            } * 0.5f; // 0.5 is the distance from the center.
    //     
    // }

    /// <summary>
    /// This function will inspect this cell and determine the meshes required to build it
    /// and their location.
    /// </summary>
    public void Rebuild_Mesh()
    {
        // requires meshes to make this work.
        var biome_pos = transform.position + new Vector3 { y = 0.1f};
        int index = Convert.ToInt32(_biome);
        var forest = Instantiate(prefabs[index], biome_pos, Quaternion.identity);
        forest.layer = gameObject.layer;
        forest.transform.parent = transform;
    }
    
    // Use this function to get a random biome for generation.
    // It uses a whitelist of generatable biomes.
    public static Biome GetRandomBiome()
    {
        // Biomes that are used in generation.
        var biomes = new Biome[] {Biome.forest, Biome.town, Biome.plains };

        var rand = Random.Range(0, biomes.Length);
        var obj = biomes.GetValue(rand);
        return (Biome)obj;
    }
}