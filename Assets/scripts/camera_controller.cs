using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Assertions;

public class camera_controller : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 10f;
    [SerializeField] private GameObject _stack_obj; // The gameobject with the Stacker class.
    private Stacker _stacker;
    private Plane _plane;
    private Camera _camera;
    private ChillGrid _grid;

    // Selection State
    private Hex _last_selected_hex; // The last hex that was moused-over or selected.
    private GameObject _selected_obj; // Mouse over of selection.
    private Material _selected_old_material; // The old material of _selected_obj.
    [SerializeField] private Material _selected_material;
    
    // Troop Movement State
    private GameObject _troop_source;

    // Mouse Input State
    private float _x_mouse_delta = 0;
    private float _x_mouse = 0;

    // Start is called before the first frame update
    void Start()
    {
        _stacker = _stack_obj.GetComponent<Stacker>();
        _camera = GetComponent<Camera>();

        _grid = GameObject.Find("grid").GetComponent<ChillGrid>();

        _x_mouse = Input.mousePosition.x;
        
        // Mouse support
        _plane = new Plane(Vector3.up, new Vector3(0, 0, 0));
    }
    
    void HighlightCell(Hex hex,bool ignore_same_cell=false)
    {
        // In hopes of not running GetComponent every frame.
        if (ignore_same_cell || hex != _last_selected_hex)
        {
            _last_selected_hex = hex;
            // Restore last selected cell (not this one) their old material.
            if (_selected_obj != null)
            {
                // If there was a last object,
                var old_renderer = _selected_obj.GetComponent<Renderer>();
                old_renderer.material = _selected_old_material;
            }                    
                    
            // Attempt to get the hexagon there.
            GameObject mouse_over_object = _grid.Get(hex);
            if (mouse_over_object != null)
            {
                // Save state
                _selected_obj = mouse_over_object;
                var renderer = mouse_over_object.GetComponent<Renderer>();
                _selected_old_material = renderer.material;

                // Set material of cell we are pointing at
                renderer.material = _selected_material;
            }
        }
    }

    void MoveTroops(GameObject source)
    {
        if (_troop_source != null)
        {
            // Move the troops from _troop_source.                            
            // Treat source like dest.
            var dest_cell = source.GetComponent<Cell>();
            var src_cell = _troop_source.GetComponent<Cell>();
            if (dest_cell.hex == src_cell.hex) return; // Don't move from to same place.
            dest_cell.Troop_Count += src_cell.Troop_Count;
            src_cell.Troop_Count = 0;
            _troop_source = null;
        }
        else
        {
            // There is already a cell, try to move troops!
            var pointed_cell = source.GetComponent<Cell>();
            if (pointed_cell.Troop_Count > 0)
            {
                _troop_source = source;
            }
        }
    }

    void DoMousePointing(bool is_left_down)
    {
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        //                     v--v---  I didn't know you could do this.
        if (_plane.Raycast(ray, out var distance) == false)
        {
            return;
        }
        // Convert the cursor's screen position to a position of the plan XZ.
        var point = ray.GetPoint(distance);

        // Snap to hexagon grid.
        var hex_pos = _grid.Global2Hex(new Vector2(point.x, point.z));

        // highlight the pointed at cell
        HighlightCell(hex_pos);

        if (is_left_down == false) return; // VERY IMPORTANT!
        // Everything past this is if the left moues button has just been clicked.
        
        // Potentially get the GameObject the cursor is pointing at.
        var pointed_obj = _grid.Get(hex_pos);
        
        // If it pointed at something, (and left click)
        if (pointed_obj != null)
        {
            MoveTroops(pointed_obj);
            return;
        }

        // If there is not a cell already here.
        // Create a new cell!
        
        // Check the neighbors, (there should be one to place.)
        var neighbors = _grid.Neighbors(hex_pos);
        foreach (var neigh in neighbors)
        {
            var obj = _grid.Get(neigh);
            if (obj == null) continue; // Keep looking.

            if (_stacker.Pop(out var biome))
            {
                _grid.CreateHex(hex_pos, biome);
                HighlightCell(hex_pos,ignore_same_cell:true);
                break;
            }
        }

    }
    
    // Update is called once per frame
    void Update()
    {
        _x_mouse_delta = Input.mousePosition.x - _x_mouse;
        _x_mouse = Input.mousePosition.x;

        if (!PauseMenu.GameIsPaused) // If the game is not paused. Read input and do stuff.
        {
            var vert = Input.GetAxis("Vertical");
            var hor = Input.GetAxis("Horizontal");

            var rot = Mathf.Deg2Rad * transform.rotation.eulerAngles.y;

            var diffPos = new Vector3
            {
                // This is cool.
                x = (float)Math.Sin(rot),
                z = (float)Math.Cos(rot)
            } * vert;
            var strafe = new Vector3
            {
                x = (float)Math.Sin(rot + Mathf.PI / 2),
                z = (float)Math.Cos(rot + Mathf.PI / 2)
            } * hor;
            diffPos = (diffPos + strafe).normalized * (cameraSpeed * Time.deltaTime);

            transform.position += diffPos;

            var is_left_down = Input.GetMouseButtonDown(0);

            DoMousePointing(is_left_down);

            if (!is_left_down && Input.GetMouseButtonDown(1))
            {
                // Only runs on the first frame the right mouse button is down!
                _troop_source = null;
            }

            if (!is_left_down && Input.GetMouseButton(1))
            {
                var camera_ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                if (_plane.Raycast(camera_ray, out var ray_distance))
                {
                    var point = camera_ray.GetPoint(ray_distance);
                    transform.RotateAround(point, Vector3.up, Mathf.Deg2Rad * _x_mouse_delta * 5);
                }
            }

        }
        
    }
}
