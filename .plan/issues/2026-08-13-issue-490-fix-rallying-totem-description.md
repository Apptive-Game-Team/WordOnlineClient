# 2026-08-13 — Rallying Totem 설명 누락 수정

- Date: 2026-08-13
- GitHub Issue: #490
- Status: Complete

## Goal

Rallying Totem의 서버 식별자와 클라이언트 MagicBook 로컬라이제이션 키 불일치를 확인하고, 한국어와 영어 설명이 정상적으로 표시되도록 수정한다.

## Non-goals

- 마법의 게임플레이 동작, 밸런스, 프리팹 또는 아트 변경
- 전체 마법 명명 체계의 일괄 마이그레이션

## Context / Constraints

- 런타임 이름과 일반 Magic 로컬라이제이션은 `RallyingTotem`/`rallyingTotem`을 사용한다.
- 현재 MagicBook 설명 테이블은 `rallying_torch` 키에만 한국어와 영어 설명을 보유한다.
- 사용자의 기존 작업 트리 변경은 보존하고 이 이슈 소유 파일만 수정한다.

## Approach (Checklist)
- [x] **Step 0: Recon** (`RallyingTotem`/`RallyingTorch` 사용처와 MagicBook 조회 후보 확인)
- [x] **Step 1: Implementation** (`rallying_torch` 키를 현재 서버 계약인 `rallying_totem`으로 변경하고 설명을 리워크된 토템 동작에 맞게 수정)
- [x] **Step 2: Tests** (키-테이블 정합성 검사와 `git diff --check` 통과, C# 빌드 오류 0 확인)
- [x] **Step 3: Rollout / Rollback** (런타임 로컬라이제이션 3개 파일로 변경 범위를 제한하고 사용자 작업과 분리)

## Validation
- **Commands to run:** 관련 키 검색 및 중복/누락 검사, `dotnet build Assembly-CSharp.csproj -v minimal`, `git diff --check`
- **Expected output:** Rallying Totem 설명 키가 두 로케일에서 해석되고 C# 컴파일 오류가 없다.
- **Result:** 로컬라이제이션 검사 및 diff 검사가 통과했고, 빌드는 기존 경고 6개와 오류 0개로 성공했다.

## Risks & Rollback
- **Risks:** 서버가 과거 `rallying_torch`와 현재 `rallying_totem`을 혼용하면 한쪽 키만 바꾸는 수정이 구버전 페이로드를 깨뜨릴 수 있다.
- **Rollback steps:** 이 이슈에서 추가하거나 수정한 로컬라이제이션 엔트리와 조회 호환 코드를 되돌린다.

## Open Questions
- 없음. 서버 리워크 커밋과 현재 소스에서 `RallyingTotem`/`rallying_totem` 계약을 확인했다.
