using UnityEngine;

namespace GameScene.PopupBook
{
    public static class GameCoordinateConverter
    {
        public static Vector3 ToUnity(Vector3 serverPosition)
        {
            return new Vector3(serverPosition.x, serverPosition.z, serverPosition.y);
        }

        public static Vector3 ToServer(Vector3 unityPosition)
        {
            return new Vector3(unityPosition.x, unityPosition.z, unityPosition.y);
        }
    }
}
