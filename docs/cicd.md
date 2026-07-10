# CI/CD 설정 가이드

이 문서는 GitHub Actions를 통한 자동 빌드 및 itch.io 배포에 필요한 설정을 안내합니다.

---

## 워크플로우 개요

`main` 브랜치에 푸시되면 다음 과정이 자동으로 실행됩니다.

1. Unity WebGL 빌드
2. 빌드 결과물을 [WordOnline_Play](https://github.com/Apptive-Game-Team/WordOnline_Play) 저장소에 푸시
3. 빌드 결과물을 itch.io에 배포

---

## 필요한 GitHub Secrets

GitHub 저장소의 **Settings > Secrets and variables > Actions** 에서 아래 시크릿을 등록해야 합니다.

### Unity 빌드 관련

| 시크릿 이름 | 설명 | 획득 방법 |
|---|---|---|
| `UNITY_LICENSE` | Unity 라이선스 파일 내용 (`.ulf` 파일) | [게임씨아이 유니티 액션 가이드](https://game.ci/docs/github/activation) 참고하여 `Unity_v20XX.X.XXXX.ulf` 파일 내용을 복사 |
| `UNITY_EMAIL` | Unity 계정 이메일 | Unity 로그인에 사용하는 이메일 주소 |
| `UNITY_PASSWORD` | Unity 계정 비밀번호 | Unity 로그인에 사용하는 비밀번호 |

### WordOnline_Play 저장소 배포 관련

| 시크릿 이름 | 설명 | 획득 방법 |
|---|---|---|
| `WORD_ONLINE_PLAY_TOKEN` | WordOnline_Play 저장소에 푸시하기 위한 GitHub Personal Access Token | GitHub **Settings > Developer settings > Personal access tokens** 에서 `repo` 권한을 포함하여 생성 |

### itch.io 배포 관련

| 시크릿 이름 | 설명 | 획득 방법 |
|---|---|---|
| `BUTLER_CREDENTIALS` | itch.io API 키 | 아래 [itch.io API 키 발급 방법](#itchio-api-키-발급-방법) 참고 |
| `ITCH_USER` | itch.io 계정 사용자명 | itch.io 프로필 URL에서 확인 (예: `https://itch.io/profile` → 사용자명 부분) |
| `ITCH_GAME` | itch.io 게임 슬러그(slug) | itch.io 게임 대시보드 URL에서 확인 (예: `https://사용자명.itch.io/게임슬러그` → 게임슬러그 부분) |

---

## itch.io API 키 발급 방법

1. [itch.io](https://itch.io) 에 로그인합니다.
2. 우측 상단 프로필 → **Settings** 클릭
3. 왼쪽 메뉴에서 **API keys** 클릭
4. **Generate new API key** 버튼을 눌러 새 키를 생성합니다.
5. 생성된 키를 복사하여 GitHub Secrets의 `BUTLER_CREDENTIALS` 에 등록합니다.

---

## itch.io 게임 페이지 생성 방법

butler로 배포하기 전에 itch.io에 게임 페이지가 미리 생성되어 있어야 합니다.

1. [itch.io](https://itch.io) 에 로그인합니다.
2. 우측 상단 프로필 → **Dashboard** 클릭
3. **Create new project** 클릭
4. 게임 정보를 입력합니다.
   - **Kind of project**: HTML (WebGL 빌드의 경우)
5. 저장 후 게임 페이지 URL을 확인합니다. (예: `https://사용자명.itch.io/게임슬러그`)
6. URL에서 사용자명과 게임슬러그를 각각 `ITCH_USER`, `ITCH_GAME` 시크릿에 등록합니다.

---

## Unity 라이선스 활성화 방법

Unity 라이선스가 없거나 갱신이 필요한 경우 아래 절차를 따릅니다.

1. [game-ci/unity-request-activation-file](https://github.com/game-ci/unity-request-activation-file) 워크플로우를 실행하여 `.alf` 파일을 다운로드합니다.
2. [Unity 라이선스 활성화 페이지](https://license.unity3d.com/manual) 에서 `.alf` 파일을 업로드하고 `.ulf` 파일을 발급받습니다.
3. `.ulf` 파일의 내용 전체를 복사하여 `UNITY_LICENSE` 시크릿에 등록합니다.

자세한 내용은 [게임씨아이(GameCI) 공식 문서](https://game.ci/docs/github/activation)를 참고하세요.
