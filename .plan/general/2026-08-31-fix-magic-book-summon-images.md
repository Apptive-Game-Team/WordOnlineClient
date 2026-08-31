# 2026-08-31 — Storm Stag와 Sea Serpent 도감 이미지 로딩 수정

- Date: 2026-08-31
- GitHub Issue: None
- Status: In Progress

## Goal

서버 마법 이름이 snake_case, camelCase, PascalCase 중 어떤 형식으로 오더라도 Storm Stag와 Sea Serpent 도감 이미지 리소스 이름을 정확히 생성한다.

## Non-goals

- 도감 이름과 설명 문구를 변경하지 않는다.
- 기존 이미지 또는 프리팹의 아트와 연출을 변경하지 않는다.

## Context / Constraints

- 도감 이미지는 `Assets/Resources/Game/sprites/<PascalCaseName>.png`에서 로드한다.
- 기존 `ToPascalCase`는 이미 camel/PascalCase인 입력의 내부 대문자를 소문자로 바꿔 `Stormstag`, `Seaserpent`를 만들 수 있다.
- Storm Stag와 Sea Serpent 이미지 에셋은 각각의 기능 브랜치에 이미 존재한다.

## Approach (Checklist)
- [x] **Step 0: Recon** (도감 이미지 로딩과 서버 이름 변환 경로 확인)
- [x] **Step 1: Implementation** (`ToPascalCase`가 camel/PascalCase 단어 경계를 보존하도록 수정)
- [x] **Step 2: Tests** (snake/camel/Pascal 입력에서 동일한 리소스 이름이 생성되는지 검증)
- [ ] **Step 3: Rollout / Rollback** (두 기능 PR 브랜치에 동일한 수정 반영)

## Validation
- **Commands to run:** 관련 Edit Mode 테스트 또는 프로젝트 컴파일, 리소스 경로 정적 검사
- **Expected output:** 모든 입력 형식에서 `StormStag`, `SeaSerpent` 생성 및 실제 PNG 경로와 일치

## Risks & Rollback
- **Risks:** 공용 문자열 변환 변경이 다른 마법 리소스 이름에 영향을 줄 수 있다.
- **Rollback steps:** 문자열 변환 및 테스트 커밋을 되돌린다.

## Open Questions
- 없음
