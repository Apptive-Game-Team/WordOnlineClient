# 2026-08-31 — Storm Stag 도감 이름과 설명 추가

- Date: 2026-08-31
- GitHub Issue: #554
- Status: Complete

## Goal

Storm Stag가 마법 도감에서 한국어와 영어 이름 및 설명으로 정상 표시되도록 로컬라이제이션 항목을 추가한다.

## Non-goals

- Storm Stag의 전투 동작, 수치, 레시피 또는 시각 연출을 변경하지 않는다.
- 클라이언트 버전을 변경하지 않는다.

## Context / Constraints

- 서버 이름 `storm_stag`는 이름 키 `stormStag`, 설명 키 `storm_stag`로 변환된다.
- Unity Localization 공유 테이블과 한국어/영어 테이블에서 동일한 엔트리 ID를 사용해야 한다.

## Approach (Checklist)
- [x] **Step 0: Recon** (기존 Magic 및 MagicBook 키 규칙과 표시 경로 확인)
- [x] **Step 1: Implementation** (`Magic` 이름과 `MagicBook` 설명의 공유/한글/영문 테이블 추가)
- [x] **Step 2: Tests** (키와 ID 대응 관계 및 중복 여부 검증)
- [x] **Step 3: Rollout / Rollback** (별도 마이그레이션 없이 에셋 변경으로 배포, 문제 시 커밋 되돌리기)

## Validation
- **Commands to run:** 관련 키 검색 및 Localization 테이블 ID 대응 검사
- **Expected output:** `stormStag`와 `storm_stag`가 각각 공유/한글/영문 테이블에 정확히 한 번 존재

## Risks & Rollback
- **Risks:** 공유 테이블과 언어 테이블의 ID가 다르면 런타임에 번역이 표시되지 않을 수 있다.
- **Rollback steps:** 해당 Localization 엔트리와 계획 파일 변경을 되돌린다.

## Open Questions
- 없음
