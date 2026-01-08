using System;
using System.Collections.Generic;

namespace Script.Admin.Dto
{
    [Serializable]
    public class RoomList
    {
        public List<RoomInfo> rooms = new List<RoomInfo>();
    }
}