using System;
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

        //https://github.com/madsbangh/EasyButtons Very nice.
        [Button]
        public void NewTurn()
        {
            new_turn.Invoke();
        }
    }
}