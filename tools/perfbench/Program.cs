// 구/신 구현의 동작 동일성 검증 + 성능 비교 하네스
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Bench;
using GeomV2 = Bench.Geom.Vector2;
using GeomV3 = Bench.Geom.Vector3;

class Program
{
    // ───────────────────────── 측정 유틸 ─────────────────────────
    static (double nsPerOp, double bytesPerOp) Measure(int iterations, Action op)
    {
        // 워밍업 (JIT + 캐시)
        for (int i = 0; i < Math.Min(iterations, 2000); i++) op();

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();

        long allocBefore = GC.GetAllocatedBytesForCurrentThread();
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++) op();
        sw.Stop();
        long allocAfter = GC.GetAllocatedBytesForCurrentThread();

        double nsPerOp = sw.Elapsed.TotalMilliseconds * 1_000_000.0 / iterations;
        double bytesPerOp = (double)(allocAfter - allocBefore) / iterations;
        return (nsPerOp, bytesPerOp);
    }

    static void Report(string name, (double ns, double bytes) old_, (double ns, double bytes) new_)
    {
        double speedup = new_.ns > 0.0001 ? old_.ns / new_.ns : double.PositiveInfinity;
        Console.WriteLine($"{name}");
        Console.WriteLine($"  old: {old_.ns,12:N1} ns/op   {old_.bytes,10:N1} B/op");
        Console.WriteLine($"  new: {new_.ns,12:N1} ns/op   {new_.bytes,10:N1} B/op");
        Console.WriteLine($"  → 시간 {speedup:N1}×, 할당 {(old_.bytes > 0 ? (new_.bytes < 0.5 ? "0 (제거)" : (old_.bytes / new_.bytes).ToString("N1") + "× 감소") : "-")}");
        Console.WriteLine();
    }

    // ───────────────────────── 동작 동일성 검증 ─────────────────────────
    static int failures = 0;
    static void AssertEq<T>(T a, T b, string what) where T : IEquatable<T>
    {
        if (!a.Equals(b)) { failures++; Console.WriteLine($"  FAIL {what}: old={a} new={b}"); }
    }

    static void VerifyResolver()
    {
        Console.WriteLine("[검증 1] GameParameterResolver — 전 매직 × {range, radius, hp, damage, 미존재} 결과 일치");
        Bench.NewImpl.GameParameterResolver.ResetCacheForBench();
        int checkedCount = 0;
        foreach (var magic in BenchData.Magics)
        {
            foreach (var p in new[] { "range", "radius", "hp", "damage", "no_such_param" })
            {
                bool ro = Bench.OldImpl.GameParameterResolver.TryGetMagicParameter(magic, p, out float vo);
                // 캐시 미스 경로와 캐시 히트 경로 둘 다 검증
                bool rn1 = Bench.NewImpl.GameParameterResolver.TryGetMagicParameter(magic, p, out float vn1);
                bool rn2 = Bench.NewImpl.GameParameterResolver.TryGetMagicParameter(magic, p, out float vn2);
                AssertEq(ro, rn1, $"{magic.serverName}.{p} found(miss)");
                AssertEq(ro, rn2, $"{magic.serverName}.{p} found(hit)");
                AssertEq(vo, vn1, $"{magic.serverName}.{p} value(miss)");
                AssertEq(vo, vn2, $"{magic.serverName}.{p} value(hit)");
                checkedCount++;
            }
        }
        Console.WriteLine($"  {checkedCount}건 비교 완료\n");
    }

    static void VerifyMultiset()
    {
        Console.WriteLine("[검증 2] AreSameMultiset — 일치/불일치/중복 원소 케이스");
        var cases = new (List<CardType> a, List<CardType> b)[]
        {
            (new() { CardType.Fire, CardType.Shoot }, new() { CardType.Shoot, CardType.Fire }),
            (new() { CardType.Fire, CardType.Fire, CardType.Water }, new() { CardType.Fire, CardType.Water, CardType.Water }),
            (new() { CardType.Fire, CardType.Fire }, new() { CardType.Fire, CardType.Fire }),
            (new() { CardType.Fire }, new() { CardType.Water }),
            (new() { }, new() { }),
            (new() { CardType.Drop, CardType.Rock, CardType.Rock }, new() { CardType.Rock, CardType.Drop, CardType.Rock }),
        };
        foreach (var (a, b) in cases)
        {
            AssertEq(MultisetOld.AreSameMultiset(a, b), MultisetNew.AreSameMultiset(a, b), $"[{string.Join(",", a)}] vs [{string.Join(",", b)}]");
        }
        // 전 매직 recipe 쌍 대조
        int n = 0;
        foreach (var m1 in BenchData.Magics)
            foreach (var m2 in BenchData.Magics)
            {
                AssertEq(MultisetOld.AreSameMultiset(m1.recipe, m2.recipe),
                         MultisetNew.AreSameMultiset(m1.recipe, m2.recipe),
                         $"{m1.serverName} vs {m2.serverName}");
                n++;
            }
        Console.WriteLine($"  {cases.Length + n}건 비교 완료\n");
    }

    static void VerifyGeometry()
    {
        Console.WriteLine("[검증 3] 도형 생성 — 정점 수 일치 (내부/경계 클리핑/코너 케이스)");
        var oldB = new Bench.Geom.OldShapeBuilder();
        var newB = new Bench.Geom.NewShapeBuilder();

        var positions = new (float x, float z, float r)[]
        {
            (9f, 5f, 3f),     // 필드 중앙, 완전 내부
            (0.5f, 5f, 3f),   // 좌측 경계 클리핑
            (17.9f, 9.8f, 4f),// 코너 클리핑
            (9f, 0.1f, 2f),   // 하단 경계
            (-2f, -2f, 1f),   // 필드 밖 (전부 클리핑 → 0)
        };
        foreach (var (x, z, r) in positions)
        {
            oldB.transform.position = new GeomV3(x, 0, z);
            oldB.BuildCircleMesh(r);
            newB.SetCircle(new GeomV3(x, 0, z), r);
            AssertEq(oldB.LastVertexCount, newB.LastVertexCount, $"circle@({x},{z}) r={r} 정점수");
        }
        Console.WriteLine($"  {positions.Length}건 비교 완료\n");
    }

    // ───────────────────────── 벤치마크 ─────────────────────────
    static void Main()
    {
        BenchData.Build();
        Console.WriteLine($"데이터: 파라미터 {BenchData.Table.Count}행, 조합마법 {BenchData.Magics.Count}종");
        Console.WriteLine($"환경: .NET {Environment.Version}, {Environment.ProcessorCount} cores\n");

        // ── 1부: 동작 동일성 ──
        VerifyResolver();
        VerifyMultiset();
        VerifyGeometry();
        Console.WriteLine(failures == 0 ? "★ 동작 동일성: 전체 통과\n" : $"★ 동작 동일성: {failures}건 실패!!\n");
        if (failures > 0) return;

        Console.WriteLine("──────────────── 성능 비교 ────────────────\n");

        var magic = BenchData.Magics[BenchData.Magics.Count / 2];

        // A. 파라미터 조회 (프레임당 2회 발생: range, radius)
        Bench.NewImpl.GameParameterResolver.ResetCacheForBench();
        var oldLookup = Measure(20_000, () =>
        {
            Bench.OldImpl.GameParameterResolver.TryGetMagicParameter(magic, "range", out _);
            Bench.OldImpl.GameParameterResolver.TryGetMagicParameter(magic, "radius", out _);
        });
        var newLookup = Measure(20_000, () =>
        {
            Bench.NewImpl.GameParameterResolver.TryGetMagicParameter(magic, "range", out _);
            Bench.NewImpl.GameParameterResolver.TryGetMagicParameter(magic, "radius", out _);
        });
        Report("A. 파라미터 조회 ×2 (range+radius, 신버전은 캐시 히트 정상상태)", oldLookup, newLookup);

        // A-2. 신버전 캐시 미스 1회 비용 (참고치)
        var missCost = Measure(2_000, () =>
        {
            Bench.NewImpl.GameParameterResolver.ResetCacheForBench();
            Bench.NewImpl.GameParameterResolver.TryGetMagicParameter(magic, "range", out _);
        });
        Console.WriteLine($"   (참고) 신버전 캐시 미스 1회: {missCost.nsPerOp:N0} ns — 카드 선택이 바뀐 프레임에 1회만 발생\n");

        // B. TryResolve 다중집합 루프 (프레임당 1회)
        var selectedRecipe = new List<CardType>(magic.recipe);
        var oldResolve = Measure(50_000, () => ResolveLoop.RunOld(selectedRecipe));
        var newResolve = Measure(50_000, () => ResolveLoop.RunNew(selectedRecipe));
        Report($"B. TryResolve 다중집합 비교 루프 (M={BenchData.MagicCount}, 중간에서 매칭)", oldResolve, newResolve);

        var noMatch = new List<CardType> { CardType.Dummy, CardType.Dummy, CardType.Dummy };
        var oldResolveMiss = Measure(50_000, () => ResolveLoop.RunOld(noMatch));
        var newResolveMiss = Measure(50_000, () => ResolveLoop.RunNew(noMatch));
        Report("B-2. TryResolve 매칭 실패 (전체 스캔 — 무효 조합 선택 중)", oldResolveMiss, newResolveMiss);

        // C. 도형 생성 — 마우스가 움직이는 프레임 (조준원+스킬원 재생성)
        var oldGeomMove = new Bench.Geom.OldShapeBuilder();
        var newGeomMove = new Bench.Geom.NewShapeBuilder();
        var path = new GeomV3[256];
        var rng = new Random(7);
        for (int i = 0; i < path.Length; i++)
            path[i] = new GeomV3(2f + (float)rng.NextDouble() * 14f, 0f, 1f + (float)rng.NextDouble() * 8f);
        int oi = 0, ni = 0;

        var oldMoving = Measure(20_000, () =>
        {
            oldGeomMove.transform.position = path[oi++ & 255];
            oldGeomMove.BuildCircleMesh(1.5f);
        });
        var newMoving = Measure(20_000, () =>
        {
            newGeomMove.SetCircle(path[ni++ & 255], 1.5f);
        });
        Report("C. 도형 재생성 — 마우스 이동 중 (필드 내부, 신버전 fast path)", oldMoving, newMoving);

        // C-2. 경계 클리핑이 실제로 도는 위치 (fast path 미적용)
        var oldGeomClip = new Bench.Geom.OldShapeBuilder();
        var newGeomClip = new Bench.Geom.NewShapeBuilder();
        var clipPath = new GeomV3[256];
        for (int i = 0; i < clipPath.Length; i++)
            clipPath[i] = new GeomV3(0.2f + (float)rng.NextDouble() * 0.5f, 0f, 1f + (float)rng.NextDouble() * 8f);
        oi = 0; ni = 0;
        var oldClipping = Measure(20_000, () =>
        {
            oldGeomClip.transform.position = clipPath[oi++ & 255];
            oldGeomClip.BuildCircleMesh(1.5f);
        });
        var newClipping = Measure(20_000, () =>
        {
            newGeomClip.SetCircle(clipPath[ni++ & 255], 1.5f);
        });
        Report("C-2. 도형 재생성 — 필드 경계에서 클리핑 발생 중", oldClipping, newClipping);

        // C-3. 정지 상태 (사거리 원의 실제 상황: 캐스터 정지 → 입력 불변)
        var oldGeomStatic = new Bench.Geom.OldShapeBuilder();
        var newGeomStatic = new Bench.Geom.NewShapeBuilder();
        var fixedPos = new GeomV3(9f, 0f, 5f);
        var oldStatic = Measure(20_000, () =>
        {
            oldGeomStatic.transform.position = fixedPos;
            oldGeomStatic.BuildCircleMesh(4f); // 구버전은 같은 입력에도 매번 재생성
        });
        var newStatic = Measure(20_000, () =>
        {
            newGeomStatic.SetCircle(fixedPos, 4f); // 신버전은 dirty check로 생략
        });
        Report("C-3. 사거리 원 — 캐스터 정지 상태 (신버전 dirty check 생략)", oldStatic, newStatic);

        // D. 종합: 조준 중 1프레임의 순수 C# 로직 비용 재현
        //    (FindObjectsByType / RaycastAll / 메시 GPU 업로드는 Unity 밖에서 측정 불가 — 제외)
        var frameOldRange = new Bench.Geom.OldShapeBuilder();
        var frameOldAim = new Bench.Geom.OldShapeBuilder();
        var frameOldSkill = new Bench.Geom.OldShapeBuilder();
        var frameNewRange = new Bench.Geom.NewShapeBuilder();
        var frameNewAim = new Bench.Geom.NewShapeBuilder();
        var frameNewSkill = new Bench.Geom.NewShapeBuilder();
        var casterPos = new GeomV3(4f, 0f, 5f);
        string lastLoggedKey = null;
        oi = 0; ni = 0;
        var recipeBuffer = new List<CardType>();

        Bench.NewImpl.GameParameterResolver.ResetCacheForBench();
        Bench.NewImpl.GameParameterResolver.TryGetMagicParameter(magic, "range", out _);
        Bench.NewImpl.GameParameterResolver.TryGetMagicParameter(magic, "radius", out _);

        var oldFrame = Measure(10_000, () =>
        {
            // TryGetCurrentMagicData: recipe 리스트 신규 할당 + 다중집합 루프
            var recipe = new List<CardType>(magic.recipe);
            ResolveLoop.RunOld(recipe);
            // 파라미터 조회 2회
            Bench.OldImpl.GameParameterResolver.TryGetMagicParameter(magic, "range", out float range);
            Bench.OldImpl.GameParameterResolver.TryGetMagicParameter(magic, "radius", out float radius);
            // 로그 중복 방지용 문자열 키 (구버전 방식)
            string key = $"{magic.serverName}:{range:F3}:{radius:F3}";
            if (lastLoggedKey != key) lastLoggedKey = key;
            // 사거리 원 (정지) + 조준 원 (이동) + 스킬 원 (이동)
            var mouse = path[oi++ & 255];
            frameOldRange.transform.position = casterPos;
            frameOldRange.BuildCircleMesh(range);
            frameOldAim.transform.position = mouse;
            frameOldAim.BuildCircleMesh(0.18f);
            frameOldSkill.transform.position = mouse;
            frameOldSkill.BuildCircleMesh(radius);
        });

        string lastName = null; float lastRange = -1, lastRadius = -1;
        var newFrame = Measure(10_000, () =>
        {
            // 조회 경로: recipe 버퍼 재사용 + 다중집합 루프 (무할당)
            recipeBuffer.Clear();
            recipeBuffer.AddRange(magic.recipe);
            ResolveLoop.RunNew(recipeBuffer);
            // 파라미터 조회 2회 (캐시 히트)
            Bench.NewImpl.GameParameterResolver.TryGetMagicParameter(magic, "range", out float range);
            Bench.NewImpl.GameParameterResolver.TryGetMagicParameter(magic, "radius", out float radius);
            // 로그 중복 방지: 값 비교 (신버전 방식)
            if (lastName != magic.serverName || lastRange != range || lastRadius != radius)
            { lastName = magic.serverName; lastRange = range; lastRadius = radius; }
            // 사거리 원 (dirty skip) + 조준 원 + 스킬 원
            var mouse = path[ni++ & 255];
            frameNewRange.SetCircle(casterPos, range);
            frameNewAim.SetCircle(mouse, 0.18f);
            frameNewSkill.SetCircle(mouse, radius);
        });
        Report("D. 종합 — 조준 중 1프레임의 순수 C# 로직 비용", oldFrame, newFrame);

        Console.WriteLine("주의: D는 순수 C# 부분만이다. FindObjectsByType(씬 스캔), EventSystem.RaycastAll,");
        Console.WriteLine("      Mesh GPU 재업로드, GC 수집 정지 자체는 Unity 밖에서 측정할 수 없으며,");
        Console.WriteLine("      수정으로 호출 자체가 제거/지연되었으므로 실제 개선 폭은 이보다 크다.");
        Console.WriteLine();
        Console.WriteLine($"프레임 예산 대비 (60fps = 16,667µs 기준):");
        Console.WriteLine($"  old 로직 비용: {oldFrame.nsPerOp / 1000.0:N1} µs/frame ({oldFrame.nsPerOp / 166670.0:N2}% of budget)");
        Console.WriteLine($"  new 로직 비용: {newFrame.nsPerOp / 1000.0:N1} µs/frame ({newFrame.nsPerOp / 166670.0:N2}% of budget)");
        Console.WriteLine($"  old 할당: {oldFrame.bytesPerOp:N0} B/frame → 60fps 기준 {oldFrame.bytesPerOp * 60 / 1024.0:N0} KB/s");
        Console.WriteLine($"  new 할당: {newFrame.bytesPerOp:N0} B/frame");
    }
}
