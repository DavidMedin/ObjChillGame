using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_controller : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 10f;

    [SerializeField] private GameObject gridCube;
    
    private Camera _camera;

    private Grid _grid;
    // Start is called before the first frame update
    void Start()
    {
       
        _camera = GetComponent<Camera>();

        _grid = GameObject.Find("grid").GetComponent<Grid>();
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
            var int_snap = _grid.WorldToCell(point);
            var grid_pos = _grid.CellToWorld(int_snap);
            gridCube.transform.position = grid_pos;
        }
    }
}
