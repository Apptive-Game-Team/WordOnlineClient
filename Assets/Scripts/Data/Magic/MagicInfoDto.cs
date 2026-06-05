using System;
using System.Collections.Generic;

namespace Data.Magic
{
    [Serializable]
    public class MagicInfoDto : IMagicRecipeSource
    {
        public long id;
        public string name;
        public string text;
        public List<string> cards;

        public long Id => id;
        public string Name => name;
        public string Text => text;
        public IReadOnlyList<string> Cards => cards;
    }
}
