using System.Text;
using UnityEngine;

namespace Scripts.Global
{
    public class IDMaker
    {
        public static string GetUserID()
        {
            StringBuilder sb = new StringBuilder();
            string userID;
            userID = sb.Append("User").Append(Random.Range(1000, 10000)).ToString();
            return userID;
        }

        private static int curUseID = 0;
    
        public static int GetCardUseInputID()
        {
            int id = curUseID;
            curUseID++;
            return id;
        }
    }
}
