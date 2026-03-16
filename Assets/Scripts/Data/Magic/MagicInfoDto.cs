using System;
using System.Collections.Generic;

namespace Data.Magic
{
    [Serializable]
    public class MagicInfoDto
    {
        public long id;
        public string name;
        public List<string> cards;
    }
}
