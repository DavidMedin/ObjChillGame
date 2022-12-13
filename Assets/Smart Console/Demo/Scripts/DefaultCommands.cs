using UnityEngine;
using UnityEngine.SceneManagement;

namespace SmartConsole.Demo
{
    /// <summary>
    /// Script that initialize some util commands
    /// Must derive from CommandBehaviour that derive from MonoBehaviour
    /// </summary>
    public class DefaultCommands : CommandBehaviour
    {
        /// <summary>
        /// Start function must be overrided
        /// </summary>
        protected override void Start()
        {
            base.Start();
        }
    
        /* write command functions using snake case convention */
    
        [Command]
        public void print_hello_world()
        {
            Debug.Log("Hello World!");
        }
    
        [Command]
        public void reload_current_scene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    
        [Command]
        public void load_scene(int index)
        {
            SceneManager.LoadScene(index);
        }
    
        [Command]
        public void help()
        {
            for (int i = 0; i < Command.List.Count; i++)
            {
                Debug.Log(Command.List[i].MethodInfo.Name);
            }
        }
    }
}
