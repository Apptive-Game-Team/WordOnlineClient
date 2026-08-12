// CombinedMagicResolver.AreSameMultiset 구/신 — 각각 git HEAD~1 / HEAD에서 그대로 복사.
using System;
using System.Collections.Generic;

namespace Bench
{
    public static class MultisetOld
    {
        public static bool AreSameMultiset(IList<CardType> a, IList<CardType> b)
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

    public static class MultisetNew
    {
        private static readonly int[] RecipeCounts = new int[Enum.GetValues(typeof(CardType)).Length];

        public static bool AreSameMultiset(IList<CardType> a, IList<CardType> b)
        {
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;

            Array.Clear(RecipeCounts, 0, RecipeCounts.Length);

            for (int i = 0; i < a.Count; i++)
            {
                int index = (int)a[i];
                if (index < 0 || index >= RecipeCounts.Length) return false;
                RecipeCounts[index]++;
            }

            for (int i = 0; i < b.Count; i++)
            {
                int index = (int)b[i];
                if (index < 0 || index >= RecipeCounts.Length || RecipeCounts[index] == 0) return false;
                RecipeCounts[index]--;
            }

            return true;
        }
    }

    // TryResolve 루프 재현: 선택 중인 recipe를 dataList 전체와 비교 (프레임당 1회 발생하는 실제 패턴)
    public static class ResolveLoop
    {
        public static int RunOld(List<CardType> recipe)
        {
            int found = -1;
            for (int i = 0; i < BenchData.Magics.Count; i++)
            {
                if (MultisetOld.AreSameMultiset(BenchData.Magics[i].recipe, recipe)) { found = i; break; }
            }
            return found;
        }

        public static int RunNew(List<CardType> recipe)
        {
            int found = -1;
            for (int i = 0; i < BenchData.Magics.Count; i++)
            {
                if (MultisetNew.AreSameMultiset(BenchData.Magics[i].recipe, recipe)) { found = i; break; }
            }
            return found;
        }
    }
}
