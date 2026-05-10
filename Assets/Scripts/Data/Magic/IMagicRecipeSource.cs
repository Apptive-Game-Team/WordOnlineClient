using System.Collections.Generic;

namespace Data.Magic
{
    public interface IMagicRecipeSource
    {
        long Id { get; }
        string Name { get; }
        IReadOnlyList<string> Cards { get; }
    }
}
