using System;
using UnityEngine;
using DefaultNamespace;


public enum Biome
{
    forest=0,
    town=1,
    plains
}
public class Cell : MonoBehaviour
{
    
    [SerializeField]
    private Biome _biome;

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
    private Hex _hex;
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
}