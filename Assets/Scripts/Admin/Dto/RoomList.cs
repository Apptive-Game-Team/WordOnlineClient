using System;
using System.Collections.Generic;

namespace Scripts.Admin.Dto
{
    [Serializable]
    public class RoomList
    {
        public List<RoomInfo> rooms = new List<RoomInfo>();
    }
}