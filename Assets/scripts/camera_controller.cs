using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class camera_controller : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 10f;
    [SerializeField] private GameObject _grid_indicator;
    [SerializeField] private GameObject hexagon_model;
    [SerializeField] private GameObject _stack_obj; // The gameobject with the Stacker class.
    private Stacker _stacker;
    private Camera _camera;

    private ChillGrid _grid;
    // Start is called before the first frame update
    void Start()
    {
        _stacker = _stack_obj.GetComponent<Stacker>();
        _camera = GetComponent<Camera>();

        _grid = GameObject.Find("grid").GetComponent<ChillGrid>();
    }

    // Update is called once per frame
    void Update()
    {
    
        var vert = Input.GetAxis("Vertical");
        var hor = Input.GetAxis("Horizontal");
            
        var diffPos = new Vector3
        { // This is cool.
            x = hor * cameraSpeed * Time.deltaTime,
            z = vert * cameraSpeed * Time.deltaTime
        };
        
        transform.position += diffPos;
        
        // Mouse support
        var plane = new Plane(Vector3.up,new Vector3(0, 0, 0));

        var ray = _camera.ScreenPointToRay(Input.mousePosition);

        //                     v--v---  I didn't know you could do this.
        if (plane.Raycast(ray,out var distance))
        {
            var point = ray.GetPoint(distance);
            
            // Snap to grid
            var hex_pos = _grid.Global2Hex(new Vector2(point.x,point.z));
            var grid_pos = _grid.Hex2Global(hex_pos) ;
            _grid_indicator.transform.position = grid_pos + new Vector3(0,0.5f,0);
            
            if (Input.GetMouseButtonDown(0))
            {
                if(_stacker.Pop(out var biome))
                {
                    var new_hex = Instantiate(hexagon_model, grid_pos, Quaternion.Euler(-90, 0, 0));
                    var cell = new_hex.GetComponent<Cell>();
                    cell.Biome = biome;
                }
            }
        }

       
    }
}
