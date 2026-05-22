using System;
using System.Collections.Generic;

namespace Data.Magic
{
    [Serializable]
    public class MagicInfoResponse
    {
        public string version;
        public string source_url;
        public List<MagicInfoDto> magics;
    }
}
