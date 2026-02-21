using Global;
using UnityEngine;

namespace Data
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
