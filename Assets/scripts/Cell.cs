using System;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;
using DefaultNamespace;


public enum Biome
{
    forset=0,
    town=1,
    plains
}
public class Cell : MonoBehaviour
{
    
    [SerializeField]
    public Biome biome;

    [SerializeField] private GameObject[] prefabs;
    private Hex _hex;
    public void Start()
    {
        #region set _sides to random biomes.
        var biomes = Enum.GetValues(typeof(Biome));
        biome = Enumerable.Repeat(0,1).Select(i =>
        {
            var rand = Random.Range(0, biomes.Length);
            var obj = biomes.GetValue(rand);
            return (Biome)obj; // gross
        }).ToArray()[0];
        #endregion
        
        Rebuild_Mesh();
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
        // for(var i=0;i < 6;i++)
        // {
            // var biome_pos = GetBiomePos(transform.position, i);
        var biome_pos = transform.position + new Vector3 { y = 0.1f + 0.25f/2 };
        int index = Convert.ToInt32(biome);
        var forest = Instantiate(prefabs[index], biome_pos, Quaternion.Euler(-90, 0, 0));
        forest.layer = gameObject.layer;
        forest.transform.parent = transform;
        // }
    }
}