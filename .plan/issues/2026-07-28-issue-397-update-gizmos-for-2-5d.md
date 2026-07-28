# 2026-07-28 - 2.5D 기즈모 좌표계 수정

- Date: 2026-07-28
- GitHub Issue: #397
- Status: Implemented

## Goal

에디터 디버그 기즈모의 원과 박스를 기존 XY 평면에서 X축 기준 45° 기울여 2.5D 시점에 맞게 렌더링한다.

## Non-goals

- 서버 기즈모 DTO 형식이나 카테고리별 색상은 변경하지 않는다.
- 런타임 빌드에 에디터 전용 기즈모를 포함하지 않는다.
- 현재 브랜치의 오브젝트 UI 및 프리팹 변경은 수정하지 않는다.

## Context / Constraints

- 완전한 XZ 평면은 기즈모가 바닥에 90° 누워 보이므로 XY와 XZ의 중간 각도가 필요하다.
- 원과 박스, 겹침 오프셋이 모두 동일한 45° 평면을 사용해야 한다.

## Approach (Checklist)

- [x] **Step 0: Recon** (`ServedObjectGizmoRenderer`와 기존 XZ 바닥 도형 구현 확인)
- [x] **Step 1: Implementation** (원/박스 좌표와 겹침 오프셋을 45° 평면 기준으로 변경)
- [x] **Step 2: Tests** (C# 빌드, Unity 컴파일 및 좌표 투영 확인)
- [x] **Step 3: Rollout / Rollback** (에디터 전용 변경 확인 및 단일 파일 롤백 경로 기록)

## Validation

- **Commands to run:** `dotnet build Assembly-CSharp.csproj -v minimal`, `git diff --check`
- **Expected output:** 컴파일 오류 0개, 공백 오류 0개, 기즈모 스크립트 진단 0개
- **Result:** 빌드 오류 0개, `git diff --check` 통과, Unity 스크립트 진단 0개
- **Runtime geometry check:** 깊이 1 투영 결과 `(Y: 0.7071, Z: 0.7071)`, 원/박스 모두 45° 평면 사용 확인

## Risks & Rollback

- **Risks:** 서버가 박스 깊이를 `boxSize.z`에 전달해야 45° 박스 크기가 정확히 표현된다.
- **Rollback steps:** `ServedObjectGizmoRenderer.cs`의 45° 투영 변경만 되돌린다.

## Open Questions

- 없음
