# 2026-08-31 — Sea Serpent 5카드 물 소환수 추가

- Date: 2026-08-31
- GitHub Issue: #555
- Status: In Progress

## Goal

`Spawn + Shoot + Water + Water + Water` 조합으로 소환되는 Sea Serpent를 DB, 서버,
Unity 클라이언트에 연결한다. 수룡은 평소 잠수 이동하며 WaterField를 남기고, 지상과
공중 대상을 향해 일직선 하이드로펌프를 발사해 선형 영역의 모든 적에게 피해를 준다.

## Non-goals

- 기존 TideCall, WaterField, WaterSlime 동작의 밸런스 변경
- 새 물 원소 상성 또는 Wet 규칙 추가
- 기존 마법 레시피 변경
- 클라이언트 버전 상승

## Context / Constraints

- Database issue: Apptive-Game-Team/WordOnlineDatabase#88, branch `feature/88`
- Server issue: Apptive-Game-Team/WordOnlineServer#471, branch `feature/471`
- Client issue: Apptive-Game-Team/WordOnlineClient#555, branch `feature/555`
- DB 변경을 서버보다 먼저 배포한다.
- 클라이언트 이미지는 `.agents/skills/make-game-art`의 master-v2 앵커와 물 슬라임
  팔레트를 사용한다.
- 기본/공격 프레임은 PPU, Bottom Center 피벗, 몸 배율과 기준점을 공유한다.

## Approach (Checklist)

- [x] **Step 0: Recon** — 세 저장소 지침, 열린 PR, 라벨, 기존 물 소환수 패턴 확인
- [x] **Step 1: Database** — 다음 Flyway 마이그레이션으로 오브젝트, 파라미터, 레시피, 태그 등록
- [x] **Step 2: Server** — SeaSerpent 마법/프리팹/잠수 이동/지대지·지대공 선형 광역 공격 구현
- [x] **Step 3: Client Art** — 잠수 기본 프레임, 기립 공격 프레임, 하이드로펌프 효과 제작 및 검증
- [x] **Step 4: Client Wiring** — 로컬라이징, 리소스, 프리팹, 상태 프레임 전환 연결
- [ ] **Step 5: Tests** — DB 정적 검사, 서버 테스트·컴파일, 클라이언트 빌드/리소스 검증
- [ ] **Step 6: Rollout** — DB → Server → Client 순서와 되돌리기 절차 문서화

## Validation

- **Commands to run:**
  - Database: `scripts/ci/validate-migrations.sh`
  - Server: `./gradlew test`
  - Client: `dotnet build Assembly-CSharp.csproj -v minimal`
  - Art: 알파, 최대 256px, 64px 판독성, 기본/공격 피벗 비교
- **Expected output:** 모든 검사 통과, SeaSerpent 리소스 이름과 서버 PrefabType 일치

## Risks & Rollback

- **Risks:** 선형 판정의 팀/대상 필터 오류, 공중 대상 누락, WaterField 과다 생성,
  공격 프레임 기준점 불일치, DB 파라미터 누락
- **Rollback steps:** 각 저장소 커밋을 역순으로 revert한다. 이미 적용된 DB 마이그레이션은
  기존 파일을 수정하지 않고 후속 forward-fix 마이그레이션으로 비활성화한다.

## Open Questions

- 수치 밸런스는 기존 5카드 소환수와 물 계열 파라미터를 기준으로 보수적으로 정한다.
