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

        /// <summary>조준 표시를 적은 document. 서버가 주지 않았으면 null 이다.</summary>
        MagicIndicatorDocument Indicator { get; }
    }
}
