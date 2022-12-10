using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Stacker : MonoBehaviour
{
    private GameObject[] stack;
    private int last = 5;
    [SerializeField] private GameObject hex_cell;

    [SerializeField] private RenderTexture target_texture;

    [SerializeField] private int _render_size;

    [SerializeField] private GameObject ui_target;
    // Start is called before the first frame update
    void Start()
    {
        
        target_texture.width = (int)Math.Ceiling(_render_size * 0.3);
        target_texture.height = _render_size;
        
        // Do the ui math for me. hahaha
        ui_target.GetComponent<RawImage>().SetNativeSize();
        
        stack = new GameObject[5];

        Repopulate();
    }
    
    public void Repopulate()
    {
        // Destroy all that are not destroyed
        for (int i = 0; i < last; i++)
        {
            Destroy(stack[i]);
        }
        
        last = 5;
        int layer = LayerMask.NameToLayer("stack");

        // Create new objects to stack
        for (int i = 0; i < 5; i++)
        {
            stack[i] = Instantiate(hex_cell,transform.position + new Vector3(0,i*1.25f, 3), Quaternion.Euler(-90,0,0));
            stack[i].layer = layer;
            stack[i].transform.parent = transform;
            
            
            var biomes = Enum.GetValues(typeof(Biome));
            stack[i].GetComponent<Cell>().Biome = Cell.GetRandomBiome();

            stack[i].transform.rotation = Quaternion.Euler(-90 + 150 + 180, 0, 0);
        }
        }


    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// You should use this like this
    /// `
    /// if(Pop(out var biome) ) { }
    /// `
    /// </summary>
    /// <param name="biome"></param>
    /// <returns></returns>
    public bool Pop(out Biome biome)
    {
        if (last == 0)
        {
            biome = 0; // Garbage
            return false;
        }
        var obj = stack[last-1];
        biome = obj.GetComponent<Cell>().Biome;
        last -= 1;
        Destroy(obj);
        return true;
    }
}
