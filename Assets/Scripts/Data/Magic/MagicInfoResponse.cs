using System;
using System.Collections.Generic;

namespace Data.Magic
{
    [Serializable]
    public class MagicInfoResponse
    {
        public long version;
        public List<MagicInfoDto> magics;
    }
}
