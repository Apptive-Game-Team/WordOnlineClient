# 2026-07-28 — Runtime Art Replacement

- Date: 2026-07-28
- GitHub Issue: [#396](https://github.com/Apptive-Game-Team/WordOnlineClient/issues/396)
- Owning repository: `Apptive-Game-Team/WordOnlineClient`
- Status: In Progress

## Goal

선정된 스타일 A와 `master-v2` 앵커를 기준으로 구 Unity 런타임
스프라이트를 하나씩 교체하고, 같은 작업에서 `theevilent` 마법 도감의 컨셉·인게임
이미지도 갱신한다.

## Acceptance Criteria

- 서버 구현, 프리팹, `resourceName`을 확인한 파일만 교체한다.
- 기존 PNG 경로와 Unity `.meta` GUID를 유지한다.
- 우향, 지면 기준 45도 상단 시점, 진영 형태 언어를 지킨다.
- small 128, middle 192, big 256 최대 크기와 비율·알파·트림을 검증한다.
- 상태 프레임은 PPU, Bottom Center 피벗, 몸 크기와 발 위치를 맞춘다.
- 숫자 접미사 프레임과 별도 오라를 `.art/ANIMATION-ASSETS.md` 의미 이름으로 관리한다.
- 승인된 컨셉과 게임 에셋만 저장하고 탈락안·중간 크로마 이미지는 제거한다.
- `theevilent` 상세 페이지에 컨셉·인게임 이미지와 실제 동작을 함께 표시한다.

## Non-goals

- 서버 전투 동작 변경
- 서버 키나 Unity 리소스 파일명 변경
- 스타일 A 또는 `master-v2` 자체 변경
- 한 번에 전체 스프라이트를 무검증 일괄 교체

## Context / Constraints

- `feature/396`은 `feature/394` 위에 쌓인 브랜치다.
- 선행 Client PR은 #395다.
- 홈페이지 변경은 `theevilent`의 `feature/7`, PR #11에 계속 추가한다.
- `.claude/worktrees/`는 작업 범위 밖이다.

## Affected Repositories and Contracts

- `WordOnlineClient`: Unity 런타임 PNG, `.art` 승인 원본, 비교 시트
- `theevilent`: 컨셉·인게임 WebP, 상세 페이지, 브라우저 테스트
- `WordOnlineServer`: 읽기 전용. 소환 관계·프리팹·공격 방식 확인에만 사용
- 계약: 서버 키, 프리팹 이름, 리소스 파일명은 변경하지 않는다.

## Approach

- [x] Recon — `FireLordSpirit`가 `FireChildSpirit`를 5초마다 최대 5기 소환하고,
  자식이 공중 원거리 `FireShot` 공격을 수행함을 확인
- [x] Implementation — `FireChildSpirit` 생성, 알파 처리, 128px 런타임 반영,
  홈페이지 관련 아트 추가
- [x] Focused validation — 크기·알파·PPU·Bottom Center·웹 테스트 확인
- [ ] Compatibility and regression validation — Unity에서 실제 소환 크기와 공격 방향 확인
- [ ] Release order and rollback check
- [x] `EmberSpiritSwarm` — 반복 소환되는 단일 지상 근접 악마로 교체,
  이동 경로 불 장판 역할을 홈페이지에 기록
- [x] `FireTadpole` / `LightningTadpole` — 동일 종 몸 구조와 128x60 기준으로 교체
- [ ] `MagmaSpirit` 기본 자세·공격 자세를 같은 PPU와 기준점으로 재작업
- [x] 불·바람 오라를 본체와 분리된 공용 효과로 재생성
- [x] 운룡의 `cloud.png`를 바람 오라와 분리된 전용 구형 물 아우라로 재생성
- [ ] 인간/골렘 재질 분리와 나머지 스타일 이탈군

## Validation

- Commands:
  - `./.art/make-sheets.sh`
  - `magick identify Assets/Resources/Game/sprites/<Name>.png`
  - `git diff --check`
  - `npm run build`
  - `npm run lint`
  - `npx playwright test`
- Manual checks:
  - Unity에서 소환 직후 크기·중심·방향·공격 투사체 시작점 확인
  - 홈페이지 데스크톱·모바일 상세 페이지 확인
- Expected results:
  - 알파가 있는 타이트한 PNG
  - 서버가 기존 키로 새 이미지를 로드
  - 웹 빌드·린트 성공, Playwright 2개 통과

## Risks & Rollback

- 프리팹이 기존 이미지 치수에 맞춘 위치·스케일을 오버라이드할 수 있다.
- 45도 시점이 맞아도 투사체 발사점과 입 위치가 어긋날 수 있다.
- 파일별 커밋을 revert하면 기존 Git 이미지와 시트를 복구할 수 있다.
- 홈페이지 커밋은 Client와 독립적으로 revert 가능하다.

## Release Order

1. 선행 Client PR #395 병합
2. Client `feature/396` 스택 PR 병합
3. `theevilent` PR #11 병합·Vercel 배포

## Open Questions

- Unity Editor에서 자동 캡처 가능한 대표 전투 장면이 있는가?
- `FireTadpole`은 균열두꺼비 새끼 설정이므로 지옥불 악마와 다른 형태 언어를
  별도로 정의해야 하는가?
