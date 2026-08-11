using System.Collections.Generic;

namespace Data.Magic
{
    public interface IMagicRecipeSource
    {
        long Id { get; }
        string Name { get; }
        string Text { get; }
        string CastType { get; }
        IReadOnlyList<string> Cards { get; }
    }
}
