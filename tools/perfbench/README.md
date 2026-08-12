# perfbench — PR #489 성능 검증 하네스

PR #489(필드 선택 중 프레임 저하 해소)의 구/신 구현을 **동작 동일성 검증 + 벤치마크**하는 독립 콘솔 앱이다. Unity 프로젝트(`Assets/`) 밖에 있으므로 Unity 임포트/빌드에 영향을 주지 않는다.

## 실행

```bash
cd tools/perfbench
dotnet run -c Release
```

.NET 8 SDK 필요.

## 구성

구현 코드는 요약본이 아니라 **git에서 그대로 복사**한 것이다:

| 파일 | 출처 |
|---|---|
| `OldResolver.cs` | 수정 전 `GameParameterResolver` (HEAD~1) — `ParametersDataSource` 접근만 치환 |
| `NewResolver.cs` | 수정 후 동일 파일 (HEAD) — 동일 치환 |
| `Multiset.cs` | `CombinedMagicResolver.AreSameMultiset` 구/신 |
| `GeomBench.cs` | `SkillIndicatorShapeRenderer` 도형 생성 경로 구/신 — Unity 수학 타입을 동일 시그니처 스텁으로 치환, Mesh GPU 업로드 제외 |
| `Shared.cs` | 데이터 타입 + 테스트 데이터 생성 (파라미터 400행, 조합마법 60종) |
| `Program.cs` | 동일성 검증(3,911건) + 측정 하네스 |

## 측정하는 것 / 못 하는 것

- 측정: 순수 C# 로직의 시간·힙 할당 (파라미터 조회, 다중집합 비교, 폴리곤 클리핑/삼각분할, dirty check)
- 측정 불가(Unity 전용, 수정으로 호출 자체가 제거/지연됨): `FindObjectsByType` 씬 스캔, `EventSystem.RaycastAll`, Mesh GPU 재업로드, WebGL Boehm GC 정지 시간

수치는 서버급 x64 .NET 8 JIT 기준이다. WebGL(wasm)에서는 절대값이 수 배 느려지고 GC 정지가 프레임에 직접 얹히므로, **상대 비율은 참고가 되지만 절대값은 그대로 옮겨지지 않는다.** 최종 확인은 Unity Profiler + WebGL 빌드에서 해야 한다.
