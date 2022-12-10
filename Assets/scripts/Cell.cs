using System;
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
    
    [SerializeField]
    private Biome _biome;

    [SerializeField] private GameObject _prefab_troop_indic;
    private uint _troop_count = 0;
    private GameObject _troop_indic;
    public GameObject Troop_Indic
    {
        get => _troop_indic;
    }
    public uint Troop_Count
    {
        set
        {
            if (_troop_count == 0 && value > 0)
            {
                Assert.AreEqual(_troop_indic,null);
                // If there is no troop indicator, create one.
                _troop_indic = Instantiate(_prefab_troop_indic, transform.position, Quaternion.identity);
            }else if (value == 0 && _troop_count > 0)
            {
                // If there was troops, and now there isn't, KILL.
                Destroy(_troop_indic);
            }
            _troop_count = value;

            if (_troop_count > 0)
            {
                var trans = _troop_indic.GetComponent<Transform>();
                trans.GetChild(1).GetComponent<TMP_Text>().text = _troop_count.ToString();
                // foreach(var child in trans)
                // {
                //     
                // }
            }
        }
        get => _troop_count;
    }

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
 
    [Button]
    void AddTroop(uint count=1)
    {
        Troop_Count = count;
    }
    
    public void Start()
    {
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
        var biome_pos = transform.position + new Vector3 { y = 0.1f + 0.25f/2 };
        int index = Convert.ToInt32(_biome);
        var forest = Instantiate(prefabs[index], biome_pos, Quaternion.Euler(-90, 0, 0));
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