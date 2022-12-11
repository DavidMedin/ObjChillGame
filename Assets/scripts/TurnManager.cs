using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EasyButtons;

namespace DefaultNamespace
{
    public class TurnManager : MonoBehaviour
    {
        ///https://forum.unity.com/threads/unityevent-where-have-you-been-all-my-life.321103/
        public UnityEvent new_turn;
        private void Start()
        {
            
        }
        
        //Yes, I hate this. This should be the Cell class, but in order to be called from
        // a UnityEvent, the function must attached to a GameObject. And I'm not about
        // to make a dummy cell object to sit around.
        public void ResetTurn()
        {
            // Reset the 'has_moved' member of all cells.
            foreach (Cell cell in Cell.All_Cells)
            {
                cell.NewTurn();
            }
        }
        
        //https://github.com/madsbangh/EasyButtons Very nice.
        [Button]
        public void NewTurn()
        {
            new_turn.Invoke();
        }
    }
}