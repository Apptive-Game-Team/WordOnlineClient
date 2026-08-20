using Coach;
using UnityEngine;

namespace Global.Coach
{
    /// <summary>
    /// 훈수 상황 하나. 규칙은 "지금 문제가 있는가"만 답한다. 타이밍과 cooldown,
    /// 숙달 집계는 전부 <see cref="CoachDirector"/>와 <see cref="CoachScheduler"/>가 갖는다.
    /// </summary>
    public interface ICoachRule
    {
        CoachRuleId Id { get; }

        /// <summary>Onboarding 테이블의 로컬라이제이션 키.</summary>
        string MessageKey { get; }

        /// <summary>같은 프레임에 여럿이 준비되면 값이 작은 쪽이 이긴다.</summary>
        int Priority { get; }

        /// <summary>힌트가 뜨기까지 문제가 이어져야 하는 시간.</summary>
        float DwellSeconds { get; }

        /// <summary>씬 한 번 방문에 이 힌트가 뜰 수 있는 최대 횟수.</summary>
        int MaxShowsPerSession { get; }

        /// <summary>
        /// director의 기본 배치 대신 대체 배치를 쓴다. 강조 대상이 기본 자리 아래에
        /// 있는 힌트는 패널이 대상을 가리므로 다른 자리로 비켜야 한다.
        /// </summary>
        bool UseAlternatePlacement { get; }

        /// <summary>
        /// 문제가 있는 동안 참이다. 거짓이 되면 dwell 타이머가 초기화되고,
        /// 힌트가 떠 있는 동안 거짓이 되면 유저가 힌트를 따른 것으로 센다.
        /// </summary>
        bool IsActive();

        /// <summary>테두리를 두를 대상. 메시지만 띄우는 힌트는 null을 준다.</summary>
        Transform[] ResolveTargets();

        /// <summary>힌트가 뜰 때 한 번 돈다. 추천 목록 갱신 같은 부수 동작에 쓴다.</summary>
        void OnShown();

        /// <summary>힌트가 사라질 때 한 번 돈다. 어떤 이유로 사라졌든 마찬가지다.</summary>
        void OnHidden();
    }
}
