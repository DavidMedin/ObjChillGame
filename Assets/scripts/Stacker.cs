using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Stacker : MonoBehaviour
{
    private GameObject[] stack;

    [SerializeField] private GameObject hex_cell;

    [SerializeField] private RenderTexture target_texture;

    [SerializeField] private int _render_size;

    [SerializeField] private GameObject ui_target;
    // Start is called before the first frame update
    void Start()
    {
        int layer = LayerMask.NameToLayer("stack");
        
        target_texture.width = (int)Math.Ceiling(_render_size * 0.3);
        target_texture.height = _render_size;
        
        // Do the ui math for me. hahaha
        ui_target.GetComponent<RawImage>().SetNativeSize();
        
        stack = new GameObject[5];
        // Create new objects to stack
        for (int i = 0; i < 5; i++)
        {
            stack[i] = Instantiate(hex_cell,transform.position + new Vector3(0,i*1.25f, 3), Quaternion.Euler(-90+150,0,0));
            stack[i].layer = layer;
            stack[i].transform.parent = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
