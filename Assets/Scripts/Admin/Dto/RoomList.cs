using System;
using System.Collections.Generic;

namespace Admin.Dto
{
    [Serializable]
    public class RoomList
    {
        public List<RoomInfo> rooms = new List<RoomInfo>();
    }
}