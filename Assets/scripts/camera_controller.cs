using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using Unity.Netcode;
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
    
    // Game Input State
    public bool is_enabled = false;

    // Selection State
    private Hex _last_selected_hex; // The last hex that was moused-over or selected.
    private GameObject _selected_obj; // Mouse over of selection.
    // private Material _selected_old_material; // The old material of _selected_obj.
    // [SerializeField] private Material _selected_material;
    
    // Troop Movement State
    private GameObject _troop_source;
    private int _troop_split;
    private int _max_troops;

    // Mouse Input State
    [SerializeField] private float _drag_speed;
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

        Input.ResetInputAxes();
    }
    
    void HighlightCell(Hex hex,bool ignore_same_cell=false)
    {
        if (_last_selected_hex == null || _selected_obj == null) return;
        // In hopes of not running GetComponent every frame.
        if (ignore_same_cell || hex != _last_selected_hex)
        {
            _last_selected_hex = hex;
            // Restore last selected cell (not this one) their old material.
            if (_selected_obj != null)
            {
                // If there was a last object,
                // var old_renderer = _selected_obj.GetComponent<Renderer>();
                // old_renderer.material = _selected_obj.GetComponent<Cell>().Target_Material;
                _selected_obj.GetComponent<Cell>().ToggleHighlight();
                _selected_obj = null;
            }                    
                    
            // Attempt to get the hexagon there.
            GameObject mouse_over_object = _grid.Get(hex);
            if (mouse_over_object != null)
            {
                // Save state
                _selected_obj = mouse_over_object;
                _selected_obj.GetComponent<Cell>().ToggleHighlight();
                // var renderer = mouse_over_object.GetComponent<Renderer>();
                // _selected_old_material = renderer.material;

                // Set material of cell we are pointing at
                // renderer.material = _selected_material;
            }
        }
    }

    // Function that is callable by everyone. It will try to
    // reevaluate the last selected hex.
    // This is only useful if the there was a hex placed
    // over the network.
    public void TryHighlight()
    {
        HighlightCell(_last_selected_hex, ignore_same_cell: true);
    }

    // Call this to disable the troop movement. Probably will be called when
    // the server moves the troops.
    public void CancelTroopMove()
    {
        _troop_source = null;
        _troop_split = 0;
        
    }
    
    void MoveTroops(GameObject source, bool divide)
    {
        var pointed_cell = source.GetComponent<Cell>();
        
        if (_troop_source != null && !divide) // If a cell is selected, and left click is pressed.
        {
            // Move the troops from _troop_source.
            // Treat source like dest.
            var src_cell = _troop_source.GetComponent<Cell>();
            if (pointed_cell.hex == src_cell.hex) return; // Don't move from to same place.
            if (!_grid.Neighbors(src_cell.hex).Contains(pointed_cell.hex)) return; // Return if it is not a neighbor of src.

            
            ObjectMagic.GetPlayerClass().RequestTroopMoveServerRpc(src_cell.hex, pointed_cell.hex, _troop_split);
            // var move_count = _troop_split == 0 ? src_cell.Troop_Count : (uint)_troop_split;
            // Assert.IsTrue(move_count <= src_cell.Troop_Count && move_count > 0); // Just checking.
            // pointed_cell.Troop_Count += move_count;
            // src_cell.Troop_Count -= move_count;
            // _troop_source = null;
            // _troop_split = 0;
            //
            // // Prevent future moving this turn
            // src_cell.has_moved = true;
            // pointed_cell.has_moved = true;
            //

        }
        else
        {
            if (pointed_cell.has_moved) return; // Don't move these troops, they're tired!
            
            // There is already a cell, try to move troops!
            if (pointed_cell.Troops > 0)
            {
                _troop_source = source;
                if (divide)
                {
                    if (pointed_cell.hex.IsOrigin() && pointed_cell.Troops == 1) return;
                    _troop_split = (int)Math.Ceiling(pointed_cell.Troops / 2.0f);
                    _max_troops = (int)pointed_cell.Troops;
                    
                    // Enable the splitting textbox.
                    var indic = pointed_cell.Troop_Indic;
                    var text_obj = indic.transform.GetChild(2);
                    text_obj.gameObject.SetActive(true);
                    var textbox = text_obj.gameObject.GetComponent<TMP_Text>();
                    textbox.text = _troop_split.ToString();
                    
                }
                else
                {
                    // We are moving all of the troops.
                    // It is not okay to move all troops from the Castle!
                    if (pointed_cell.hex.IsOrigin())
                    {
                        // Because this is the main state of what action we are doing, make it null.
                        _troop_source = null;
                    }
                }
                
                
            }
        }
    }

    void DoMousePointing(bool is_left_down, bool is_right_down)
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

        // don't do much if i a big booty fart.
        if (!is_enabled) return;
        
        if (is_left_down == false && is_right_down == false) return; // VERY IMPORTANT!
        // Everything past this is if the left moues button has just been clicked.
        
        // Potentially get the GameObject the cursor is pointing at.
        var pointed_obj = _grid.Get(hex_pos);
        
        // If it pointed at something, (and left click)
        if (pointed_obj != null)
        {
            MoveTroops(pointed_obj, is_right_down);
            return;
        }

        // Now, we don't care about the right mouse button, return if it is down.
        if (is_left_down == false || is_right_down == true) return;
        
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
                ObjectMagic.GetPlayerClass().RequestCellServerRpc(hex_pos,biome);
                // _grid.CreateHex(hex_pos, biome);
                // HighlightCell(hex_pos,ignore_same_cell:true);
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
            // Why?
            var vert = Input.GetAxisRaw("Vertical");
            var hor = Input.GetAxisRaw("Horizontal");

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
            diffPos = (diffPos + strafe).normalized * (cameraSpeed * Time.unscaledDeltaTime);
            // print(Time.unscaledDeltaTime);
            transform.position += diffPos;
            
            var is_left_down = Input.GetMouseButtonDown(0);
            var is_right_down = Input.GetMouseButtonDown(1);
            
            
            // Get mouse scroll
            if (_troop_split != 0)
            {
                // If we are splitting the troop count. 
                // troop count is not zero only when we are splitting the troops.
                var scroll = (int)Math.Round(Input.mouseScrollDelta.y,MidpointRounding.AwayFromZero); // Rounds.
                // So we don't GetComponent twice every frame.
                if (scroll != 0)
                {
                    _troop_split += scroll;
                    // Constrain _troop_split to 0 < _troop_split <= _max_troops
                    
                    // Hush
                    try
                    {
                        _troop_split = Math.Clamp(_troop_split,1, _max_troops-1);
                    }catch{}

                    // Update the text box for the split
                    var cell = _troop_source.GetComponent<Cell>();
                    var indic = cell.Troop_Indic;
                    var textbox = indic.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
                    textbox.text = _troop_split.ToString();
                }
            }
            
            if (!is_left_down &&  is_right_down)
            {
                // This is used to cancel selections!
                // Only runs on the first frame the right mouse button is down!

                // Disable the splitting textbox.
                if (_troop_source != null)
                {
                    var indic = _troop_source.GetComponent<Cell>().Troop_Indic;
                    var text_obj = indic.transform.GetChild(2);
                    text_obj.gameObject.SetActive(false);
                }
                
                // _troop_source = null;
                // _troop_split = 0;
                CancelTroopMove();
            }

            if (!is_left_down && Input.GetMouseButton(1))
            {
                var camera_ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                if (_plane.Raycast(camera_ray, out var ray_distance))
                {
                    var point = camera_ray.GetPoint(ray_distance);
                    transform.RotateAround(point, Vector3.up, Mathf.Deg2Rad * _x_mouse_delta * _drag_speed);
                }
            }

            DoMousePointing(is_left_down,is_right_down);

        }
        
    }
}
