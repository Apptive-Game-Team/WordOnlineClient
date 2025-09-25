using System;
using System.Collections.Generic;
using Script.Data;

public static class CombinedMagicResolver
{
    public static bool TryResolve(IList<CardType> recipe, out CombinedMagicData data)
    {
        foreach (var d in LocalCombinedMagicData.dataList)
        {
            if (AreSameMultiset(d.recipe, recipe))
            {
                data = d;
                return true;
            }
        }
        data = null;
        return false;
    }
    
    private static bool AreSameMultiset(IList<CardType> a, IList<CardType> b)
    {
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;

        var counts = new Dictionary<CardType, int>();
        foreach (var x in a)
        {
            counts.TryGetValue(x, out var c);
            counts[x] = c + 1;
        }

        foreach (var y in b)
        {
            if (!counts.TryGetValue(y, out var c) || c == 0)
                return false;
            counts[y] = c - 1;
        }

        return true;
    }
}