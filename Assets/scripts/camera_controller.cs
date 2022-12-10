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
    
    
    // Mouse Input State
    private float _x_mouse_delta = 0;
    private float _x_mouse = 0;
    private bool is_mouse_down = false;

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
            print("Moused a new hexagon!");
            // Restore last selected cell (not this one) their old material.
            if (_selected_obj != null)
            {
                print("Restoring old material.");
                // If there was a last object,
                var old_renderer = _selected_obj.GetComponent<Renderer>();
                old_renderer.material = _selected_old_material;
            }                    
                    
            // Attempt to get the hexagon there.
            GameObject mouse_over_object = _grid.Get(hex);
            if (mouse_over_object != null)
            {
                print("Writing new state.");
                // Save state
                _selected_obj = mouse_over_object;
                var renderer = mouse_over_object.GetComponent<Renderer>();
                _selected_old_material = renderer.material;

                // Set material of cell we are pointing at
                renderer.material = _selected_material;
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



            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            var is_left_down = false;
            //                     v--v---  I didn't know you could do this.
            if (_plane.Raycast(ray, out var distance))
            {
                var point = ray.GetPoint(distance);

                // Snap to grid
                var hex_pos = _grid.Global2Hex(new Vector2(point.x, point.z));

                
                HighlightCell(hex_pos);

                var grid_pos = _grid.Hex2Global(hex_pos);
                // _grid_indicator.transform.position = grid_pos + new Vector3(0, 0.5f, 0);

                if (Input.GetMouseButton(0))
                {
                    is_left_down = true;
                    if (_grid.Get(hex_pos) == null) // If there is not a cell already here.
                    {
                        var neighbors = _grid.Neighbors(hex_pos);
                        foreach (var neigh in neighbors)
                        {
                            var obj = _grid.Get(neigh);
                            if (obj != null)
                            {
                                if (_stacker.Pop(out var biome))
                                {
                                    _grid.CreateHex(hex_pos, biome);
                                    HighlightCell(hex_pos,ignore_same_cell:true);
                                    break;
                                }
                            }
                        }
                    }
                }

            }

            if (!is_left_down && Input.GetMouseButtonDown(1))
            {
                is_mouse_down = true;
            }



            if (is_mouse_down)
            {
                var camera_ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                if (_plane.Raycast(camera_ray, out var ray_distance))
                {
                    var point = camera_ray.GetPoint(ray_distance);
                    transform.RotateAround(point, Vector3.up, Mathf.Deg2Rad * _x_mouse_delta * 5);
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                is_mouse_down = false;
            }
        }
        else // If the game is 'paused'
        {
            // Fixes state, probably. Don't @ me.
            is_mouse_down = false;
        }
    }
}
