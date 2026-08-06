# 2026-08-06 — 타격 이펙트를 공격자 방향에 표시

- Date: 2026-08-06
- GitHub Issue: #449
- Status: Draft

## Goal

서버가 프레임에 싣기 시작한 `hit` 이벤트(Apptive-Game-Team/WordOnlineServer#356)를 읽어,
별 모양 타격 이펙트를 피격자 몸 중앙이 아니라 공격자 쪽 면에 띄운다.

## Non-goals

- `hit` 외 이벤트 처리. 판별자 등록만 늘리면 되도록 구조만 열어 둔다.
- 피격 반동(뽈롱) 연출 변경.

## Context / Constraints

- #451 의 Newtonsoft 마이그레이션 위에 쌓는다. 이벤트 리스트는 그 PR 이 만든
  `JsonSubtypeConverter` 를 그대로 재사용한다.
- 지금 `HitEffectController` 는 `ServedObject.OnHpDecreased` 하나로 별 스폰과 반동을 같이 한다.
  `OnHpDecreased` 는 프레임당 HP 델타 기준이라 한 프레임에 두 대 맞아도 한 번만 뛴다.
- 스프라이트는 기울어진 2.5D 카메라를 향해 빌보드된다. 월드 좌표로 오프셋을 주면 스프라이트에서
  어긋나므로, 방향은 화면 공간에서 구하고 적용은 렌더러 축으로 해야 한다
  (`GetSpeechBubbleAnchorWorldPosition` 과 같은 이유).
- 공격자는 같은 프레임의 `objects.update` 에서 파괴될 수 있다. 이벤트를 오브젝트 업데이트보다
  먼저 처리하면 죽은 공격자 좌표를 따로 캐싱할 필요가 없다.

## Approach (Checklist)

- [x] `GameEvent` / `HitEvent` + `GameEventConverter` (판별자 `type`), `FrameInfoDto.events`,
      `SyncFrameInfo.events`.
- [x] `GameEventHandler` — 이벤트를 오브젝트 생성 뒤, 업데이트 전에 처리.
- [x] `HitEffectController` 분리: 반동은 `OnHpDecreased` 유지, 별은 `hit` 이벤트로 이동.
      공격자를 못 찾으면 예전처럼 중앙에 띄운다.
- [x] `ServedObject.GetEdgeWorldPositionTowards` — 화면 공간 방향 × 스프라이트 half size ×
      `attackerSideBias`. 오프셋이 스프라이트 크기에 따라 자동으로 맞는다.
- [x] Edit Mode 테스트: 프레임/싱크 이벤트 파싱, 모르는 이벤트 타입, 이벤트 없는 프레임.

## Validation

- **Commands to run:** `Unity -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -`
- **Manual:** 근접·원거리·마법 피해를 각각 받아 별이 공격자 쪽에 뜨는지, 도트 피해에서는
  반동만 나오는지 확인.

## Risks & Rollback

- **Risks:** actor 를 못 찾는 경우가 잦으면 중앙 스폰으로 되돌아가 기존과 같아 보인다.
  `attackerSideBias` 가 크면 큰 유닛에서 별이 스프라이트 밖으로 나갈 수 있다.
- **Rollback steps:** `git revert`.

## Open Questions

- 없음.
