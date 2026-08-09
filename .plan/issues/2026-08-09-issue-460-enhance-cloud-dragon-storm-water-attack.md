# 2026-08-09 — Cloud Dragon 번개·물 공격 연출 강화

- Date: 2026-08-09
- GitHub Issue: #460
- Status: Complete

## Goal

기존 `CloudDragon.png`를 프로젝트의 2.5D 컷페이퍼 화풍으로 다시 제작하고 기본/공격 2프레임으로 분리해, 평상시에는 입에 물이 없고 공격 이벤트 중에만 물을 분사하도록 연결한다.

## Non-goals

- 서버 동작, 공격 판정, 밸런스는 변경하지 않는다.
- 전용 구형 물 오라 `cloud.png`와 공용 원소 오라는 교체하지 않는다.
- 마스터 화풍이나 정령 진영의 형태 언어를 변경하지 않는다.

## Context / Constraints

- 마스터 기준은 `.art/anchors/master-v2/MasterStyleKey.png`이며, 정령은 둥글고 부드러운 실루엣과 친근한 눈 문법을 사용한다.
- 본체와 오라는 별도 자산이다. 본체에 큰 구형 오라를 굽지 않는다.
- 기존 `CloudDragon.png.meta`를 보존해 프리팹 GUID 연결을 유지한다.
- 공격 프레임은 `CloudDragonAttacking.png`로 추가하고 `AttackSpriteSwapController`에 연결한다.
- 우향 3/4 시점, 투명 배경, 최대 256px, 64px에서 읽히는 실루엣을 유지한다.

## Approach (Checklist)

- [x] **Step 0: Recon** (기존 본체, 물 오라, 프리팹, 마스터 스타일 및 임포트 설정 확인)
- [x] **Step 1: Implementation** (입에 물이 없는 기본 프레임과 물을 분사하는 공격 프레임 제작, 동일 캔버스 정렬, 프리팹 공격 이벤트 연결)
- [x] **Step 2: Tests** (양쪽 프레임 치수·알파·PPU·피벗·본체 배치·GUID·프리팹 참조·64px 실루엣 검증)
- [x] **Step 3: Rollout / Rollback** (변경 파일 범위를 확인하고 기존 파일은 Git 이력으로 복구 가능하게 유지)

## Validation

- **Commands to run:** Pillow 기반 이미지 식별/알파 검사와 64px 축소 검토, 집중 비교 시트 검토, `git diff --check`, 프리팹 GUID 정적 참조 검사
- **Expected output:** 기본/공격 모두 `220x156` RGBA PNG와 동일한 PPU 100/Bottom Center 설정을 사용한다. `CloudDragon.prefab`은 공격 이벤트 때 `CloudDragonAttacking.png`를 0.1초 표시하고 기본 프레임으로 복원하며, 기존 본체와 물 오라 GUID는 유지한다. 로컬 `magick` 실행 파일이 없어 프로젝트 전체 시트 스크립트 대신 동일 목적의 집중 비교 시트를 Pillow로 생성해 검토했다.

## Risks & Rollback

- **Risks:** 번개가 과하면 lightning 전용 유닛으로 오인될 수 있고, 물 표현을 본체에 과도하게 넣으면 전용 물 오라와 시각적으로 겹칠 수 있다.
- **Rollback steps:** `CloudDragon.png`와 `CloudDragon.prefab`을 이전 Git 버전으로 되돌리고 새 `CloudDragonAttacking.png/.meta`를 제거한다. `cloud.png`는 유지한다.

## Open Questions

- 없음. 요청의 “번개 느낌은 조금”을 보조 포인트로 해석하고, 물 공격을 주 역할로 유지한다.
