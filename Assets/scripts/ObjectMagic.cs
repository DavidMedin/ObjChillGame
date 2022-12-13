using Unity.Netcode;
using UnityEngine;

namespace DefaultNamespace
{
    public static class ObjectMagic
    {
        public static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static player GetPlayerClass()
        {
            return NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<player>();
        }
    }
}