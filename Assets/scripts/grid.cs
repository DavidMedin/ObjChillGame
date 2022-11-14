using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class grid : MonoBehaviour
    {
        public GameObject grid_prefab;
        private Grid _grid;
        private void Start()
        {
            _grid = GetComponent<Grid>();
            // Create a lot of items in this grid.
            for(int x=-50; x < 50; x++)
            {
                for (int z = -50; z < 50; z++)
                {
                    var world_point = _grid.GetCellCenterLocal(new Vector3Int(x, z, 0));
                    GameObject new_obj =Instantiate(grid_prefab, world_point, Quaternion.Euler(-90,0,0));
                    // new_obj.transform.parent = this.transform;
                }
            }
        }
    }
}