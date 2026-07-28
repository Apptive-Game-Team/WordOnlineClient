# 2026-07-28 - 2.5D 오브젝트 월드 UI 통합

- Date: 2026-07-28
- GitHub Issue: #397
- Status: Implemented

## Goal

HP 바, 플레이어 이름, 건물 지속 시간(TTL) 게이지를 프리팹에 구성된 하나의 World Space Canvas 아래로 통합하고, 고정된 2.5D 카메라를 기준으로 생성 시 한 번만 정렬한다.

## Non-goals

- 런타임에 흩어진 UI 오브젝트를 새 부모 아래로 재구성하지 않는다.
- 동적 카메라 추적 기능은 추가하지 않는다.
- HP/TTL 값 갱신 규칙은 변경하지 않는다.

## Approach (Checklist)

- [x] 기존 HP/TTL/이름 프리팹과 상속 구조 조사
- [x] 공통 `ServedObjectWorldUI` 프리팹 및 Mob/Building/Player 변형 생성
- [x] 대상 오브젝트 프리팹을 단일 World Space Canvas 구조로 전환
- [x] 매 프레임 빌보드 갱신 제거 및 고정 카메라 기준 1회 정렬
- [x] 기존 플레이어 탐색 코드를 통합 UI 기준으로 변경
- [x] C# 빌드, Unity 컴파일/콘솔, 프리팹 구조 검증

## Validation

- `dotnet build Assembly-CSharp.csproj -v minimal`: 성공, 오류 0개
- `git diff --check`: 통과
- Unity 강제 리프레시/컴파일: 성공
- Unity Console 오류: 0개
- 대상 프리팹: `ServedObjectWorldUI` 1개, World Space Canvas 1개, 레거시 UI 컴포넌트 0개

## Risks & Rollback

- 실제 게임 해상도와 가장 큰/작은 스프라이트에서 세로 간격은 Play Mode 육안 확인이 필요하다.
- 롤백은 이슈 #397 범위의 새 UI 프리팹과 소유 프리팹 변경을 되돌리면 된다.
