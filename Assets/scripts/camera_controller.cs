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
    [SerializeField] private GameObject _stack_obj; // The gameobject with the Stacker class.
    private Stacker _stacker;
    private Plane _plane;
    private Camera _camera;

    private ChillGrid _grid;

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

                var grid_pos = _grid.Hex2Global(hex_pos);
                _grid_indicator.transform.position = grid_pos + new Vector3(0, 0.5f, 0);

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
