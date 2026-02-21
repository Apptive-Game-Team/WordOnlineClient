using Scripts.Global;
using UnityEngine;

namespace Scripts.Data
{
    public class UserInfo : MonoBehaviour
    {
        public string userID;

        private void Awake()
        {
            userID = IDMaker.GetUserID();
        }
    }
}
