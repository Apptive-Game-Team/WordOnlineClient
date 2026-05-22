using System;
using System.Collections.Generic;
using Data.Versioning;

namespace Data.Magic
{
    [Serializable]
    public class MagicInfoResponse : IVersionedResponse
    {
        public bool changed;
        public string version;
        public string source_url;
        public List<MagicInfoDto> magics;

        public bool Changed
        {
            get => changed;
            set => changed = value;
        }

        public string Version
        {
            get => version;
            set => version = value;
        }

        public string SourceUrl
        {
            get => source_url;
            set => source_url = value;
        }
    }
}
