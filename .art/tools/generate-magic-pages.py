#!/usr/bin/env python3
"""Generate art concept pages from authoritative game-server magic code."""

from __future__ import annotations

import re
import json
from collections import defaultdict
from pathlib import Path


CLIENT_ROOT = Path(__file__).resolve().parents[2]
GAME_ROOT = CLIENT_ROOT.parent / "game"
MAGIC_ROOT = GAME_ROOT / "src/main/java/com/wordonline/server/game/domain/magic/implement"
PREFAB_ROOT = GAME_ROOT / "src/main/java/com/wordonline/server/game/domain/object/prefab/implement"
OUTPUT_ROOT = CLIENT_ROOT / ".art/magic"
PAGE_ROOT = OUTPUT_ROOT / "pages"

KOREAN_NAMES = {
    "aqua_archer": "물의 궁수",
    "bubble_generator": "거품 생성기",
    "bubble_spirit": "거품 정령",
    "cannon": "대포",
    "chain_lightning": "연쇄 번개",
    "chicken_commando": "치킨 특공대",
    "cloud_dragon": "운룡",
    "crater": "분화구",
    "dimension_toad": "차원 두꺼비",
    "electric_tower": "전기 타워",
    "ember_spirit_swarm": "불씨 정령 무리",
    "fire_drop": "화염 투하",
    "fire_lord_spirit": "불 대왕 정령",
    "fire_shot": "화염탄",
    "fire_slime_nest": "화염 슬라임 둥지",
    "fire_spirit": "불의 정령",
    "frenzy_totem": "광란의 토템",
    "healing_totem": "회복 토템",
    "leafair": "술이파리",
    "life_tree": "생명의 나무",
    "lightning_drop": "번개 투하",
    "lightning_explosion": "번개 폭발",
    "lightning_shot": "전격탄",
    "lightning_slime_nest": "번개 슬라임 둥지",
    "magma_explosion": "마그마 폭발",
    "magma_spirit": "용암 정령",
    "mana_well": "마나 샘",
    "meteor_shower": "유성우",
    "mini_rock_swarm": "꼬마 돌 무리",
    "nature_drop": "자연 투하",
    "nature_slime_nest": "자연 슬라임 둥지",
    "overgrowth": "과생장",
    "pve_water_slime_nest": "PVE 물 슬라임 둥지",
    "rallying_torch": "집결의 횃불",
    "razor_gale": "칼바람 회오리",
    "rock_drop": "바위 투하",
    "rock_golem": "바위 골렘",
    "rock_mage": "돌 마법사",
    "rock_rolling": "돌 굴리기",
    "rock_slime_nest": "바위 슬라임 둥지",
    "rock_turret": "돌 포탑",
    "sand_storm": "모래 폭풍",
    "seed_spirit_swarm": "씨앗 정령 무리",
    "shock_overload": "전격 과부하",
    "storm_rider": "폭풍 기수",
    "thunder_bird_swarm": "천둥 새 무리",
    "thunder_spirit": "번개 정령",
    "tide_call": "파도 소환",
    "tornado_strike": "토네이도",
    "tower": "타워",
    "towerback": "포탑등이",
    "tree_golem": "나무 골렘",
    "vine_colony": "덩굴 군집",
    "vine_fan": "덩굴 부채",
    "vine_spirit": "덩굴 정령",
    "vine_toss": "덩굴 투척",
    "water_explosion": "간헐천 폭발",
    "water_shot": "물 탄환",
    "water_slime_swarm": "물 슬라임 무리",
    "will_o_wisp": "도깨비불",
    "wind_blade": "바람 칼날",
    "wind_drop": "바람 투하",
    "wind_explosion": "돌풍",
    "wind_slime_nest": "바람 슬라임 둥지",
    "wind_spirit": "바람 정령",
    "wind_totem": "바람 토템",
    "zap_mouse": "찌릿쥐",
}

DISPLAY_NAMES = {
    "aqua_archer": "물결 궁수",
    "bubble_generator": "물방울 배양기",
    "bubble_spirit": "거품 슬라임",
    "cannon": "마도 대포",
    "chain_lightning": "뇌광 사슬",
    "chicken_commando": "비전 강하대",
    "cloud_dragon": "구름비늘 운룡",
    "crater": "잿불 분화구",
    "dimension_toad": "균열두꺼비",
    "electric_tower": "뇌전 마도탑",
    "ember_spirit_swarm": "잿불 악마 무리",
    "fire_drop": "지옥불 강하",
    "fire_lord_spirit": "지옥불 군단장",
    "fire_shot": "악마의 화염탄",
    "fire_slime_nest": "지옥불 산란장",
    "fire_spirit": "지옥불 하급 악마",
    "frenzy_totem": "군단 광란토템",
    "healing_totem": "세계수 치유토템",
    "leafair": "잎바람 요정",
    "life_tree": "생명의 묘목",
    "lightning_drop": "낙뢰 강하",
    "lightning_explosion": "뇌광 폭발",
    "lightning_shot": "번개 파편",
    "lightning_slime_nest": "뇌광 정령제단",
    "magma_explosion": "용암 균열",
    "magma_spirit": "용암 갑각 악마",
    "mana_well": "비전 마나샘",
    "meteor_shower": "지옥불 유성우",
    "mini_rock_swarm": "꼬마돌 돌격대",
    "nature_drop": "뿌리 강하",
    "nature_slime_nest": "세계수 씨앗터",
    "overgrowth": "세계수의 과생장",
    "pve_water_slime_nest": "물방울 보금자리",
    "rallying_torch": "집결 신호화",
    "razor_gale": "면도날 회오리",
    "rock_drop": "거석 낙하",
    "rock_golem": "이끼바위 골렘",
    "rock_mage": "룬바위 주술사",
    "rock_rolling": "구르는 거석",
    "rock_slime_nest": "꼬마돌 집결지",
    "rock_turret": "암석 포탑",
    "sand_storm": "황진 폭풍",
    "seed_spirit_swarm": "씨앗 정령 군락",
    "shock_overload": "뇌광 과부하",
    "storm_rider": "폭풍갈기 기수",
    "thunder_bird_swarm": "천둥새 편대",
    "thunder_spirit": "뇌광 정령",
    "tide_call": "밀려드는 해일",
    "tornado_strike": "회오리 정령",
    "tower": "대공 마도탑",
    "towerback": "포탑등이",
    "tree_golem": "고목 수호자",
    "vine_colony": "덩굴 군락",
    "vine_fan": "세 갈래 덩굴",
    "vine_spirit": "덩굴 추적자",
    "vine_toss": "솟구치는 덩굴",
    "water_explosion": "솟구치는 간헐천",
    "water_shot": "응축 물방울",
    "water_slime_swarm": "물방울 생존자 무리",
    "will_o_wisp": "길잡이 도깨비불",
    "wind_blade": "초승달 바람칼",
    "wind_drop": "하강 돌풍",
    "wind_explosion": "터지는 돌풍",
    "wind_slime_nest": "바람 정령제단",
    "wind_spirit": "질풍 정령",
    "wind_totem": "순풍 토템",
    "zap_mouse": "찌릿꼬리쥐",
}

FAMILY_LABELS = {
    "build": "설치",
    "drop": "투하",
    "explode": "범위 폭발",
    "shoot": "투사체",
    "spawn": "소환",
}

COMPONENT_BEHAVIOR = {
    "AttackMob": "공격 상태를 수행한다.",
    "BehaviorMob": "대상을 탐색하고 상태 머신으로 이동·공격을 결정한다.",
    "BubbleGeneratorMob": "주기적으로 거품 관련 개체를 생성한다.",
    "BubbleSpiritMob": "거품 정령 전용 공격 행동을 수행한다.",
    "Cannon": "고정 위치에서 탄환을 발사한다.",
    "ChickenCommandoMob": "치킨 특공대 전용 이동·공격 행동을 수행한다.",
    "CraterEmber": "분화구에서 나온 불씨가 이동 후 화염 지대를 만든다.",
    "CraterSpawner": "분화구가 불씨 개체를 반복 생성한다.",
    "Drop": "상공에서 목표 지점으로 낙하한다.",
    "Explode": "범위 안 대상에게 폭발 효과를 적용하고 종료한다.",
    "FrenzyTotem": "범위 안 아군에게 광란 상태 효과를 제공한다.",
    "Leafair": "낙하 후 술이파리 고유 동작을 수행한다.",
    "LightningStrike": "번개 타격을 적용한다.",
    "LightningTowerMob": "복수 대상을 탐지해 전기 공격을 수행한다.",
    "LimitedSequenceSpawner": "등록된 하위 개체를 정해진 순서와 횟수만큼 소환한다.",
    "ManaWellMob": "마나 샘 고유 지원 행동을 수행한다.",
    "MeleeAttackMob": "근접 거리까지 접근해 공격한다.",
    "NonAttackingCowardMob": "직접 공격하지 않고 위협에서 거리를 벌린다.",
    "OvergrowthExplosion": "씨앗 정령을 나무 골렘으로 성장시키고 추가 씨앗을 만든다.",
    "PathSpawner": "이동 경로를 따라 원소 지대를 남긴다.",
    "ProjectileRangeAttackMob": "사거리 안 대상을 향해 투사체를 발사한다.",
    "PushShot": "명중 대상을 밀어내는 투사체다.",
    "RallyingTorch": "아군을 집결시키는 이동 지시를 제공한다.",
    "RangeAttackMob": "거리를 유지하며 원거리 공격한다.",
    "RazorGale": "회전하는 칼바람 범위 공격을 수행한다.",
    "RollingRock": "지면을 굴러가며 충돌 대상을 공격한다.",
    "Rune": "밟거나 접촉한 대상에 원소 효과를 적용한다.",
    "SelfDestructMob": "대상에게 접근한 뒤 자폭한다.",
    "SelfHealer": "주기적으로 자신 또는 아군을 회복한다.",
    "SequentialLineSpawner": "직선 경로에 개체를 순차 생성한다.",
    "Shot": "시전자에서 목표 지점 방향으로 비행한다.",
    "ShockOverloadExplosion": "전격 과부하 범위 효과를 적용한다.",
    "Spawner": "정해진 종류의 개체를 반복 생성한다.",
    "SprayingAttacker": "부채꼴 또는 연속 분사 공격을 수행한다.",
    "SummonMob": "주기적으로 지정된 하위 개체를 소환한다.",
    "SummonerMob": "전투 상태 머신 안에서 하위 개체를 소환한다.",
    "ThunderBirdMob": "천둥 새 전용 돌진·공격 행동을 수행한다.",
    "TimedBehaviorMob": "정해진 간격으로 행동을 반복한다.",
    "Tornado": "회오리 이동과 충돌 효과를 수행한다.",
    "Totem": "고정 범위 안 대상에게 지속 지원 효과를 준다.",
    "Tower": "움직이지 않고 탐지 범위 안 적을 공격한다.",
    "Turret": "고정 위치에서 표적을 추적해 공격한다.",
    "VineToss": "덩굴을 연속 생성해 경로상의 대상을 타격한다.",
    "VineHitTracker": "한 번의 덩굴 연쇄에서 같은 대상을 중복 타격하지 않도록 추적한다.",
    "WaterExplode": "물 폭발 고유 범위 효과를 적용한다.",
    "WindBladeShot": "바람 칼날 투사체의 관통·타격 동작을 수행한다.",
}

SPECIAL_BEHAVIOR = {
    "chicken_commando": [
        "지정 지점 상공에서 낙하산을 펼친 인간 공수대원으로 생성된다.",
        "높이 0.5 초과에서는 낙하산 프레임, 지상에서는 전투 프레임을 표시한다.",
        "착지 후 지상 대상을 추적해 근접 공격한다.",
    ],
    "dimension_toad": [
        "직접 공격하지 않고 지상 위협에서 거리를 벌린다.",
        "`FireTadpole`과 `LightningTadpole`을 10초마다 번갈아가며 제한 없이 소환한다.",
    ],
    "fire_lord_spirit": [
        "`FireChildSpirit`를 5초마다 1마리씩, 최대 5마리 소환한다.",
        "소환된 비행 악마는 지상·공중 대상을 향해 `FireShot`으로 원거리 공격한다.",
    ],
    "vine_fan": [
        "시전자에서 목표 방향을 구한다.",
        "중앙 방향 기준 60도 부채꼴에 3개의 `VineToss`를 발사한다.",
        "각 덩굴 줄기는 전방에 6개 덩굴을 0.12초 간격, 1칸 간격으로 연속 생성한다.",
    ],
    "vine_toss": [
        "시전자에서 목표 방향을 구한다.",
        "첫 덩굴을 전방 1칸에 생성한다.",
        "같은 방향으로 총 6개 덩굴을 0.12초 간격으로 연속 생성한다.",
        "한 연쇄 안에서 같은 대상을 중복 타격하지 않도록 명중 대상을 공유 추적한다.",
    ],
}

TACTICAL_OVERRIDES = {
    "chicken_commando": {
        "mobility": "상공 낙하 후 지상 이동",
        "mobility_evidence": "지정 높이에서 중력 낙하한 뒤 지상 이동 AI로 전환한다.",
        "combat_role": "공수 투입·근접 공격",
        "targeting": "지상",
        "attack_shape": "단일 표적",
        "special_movement": "높이에 따른 낙하산·지상 2프레임 전환",
    },
    "dimension_toad": {
        "combat_role": "원소 올챙이 소환",
        "targeting": "지상 위협 감지",
        "attack_shape": "10초 간격 교대 소환",
        "special_movement": "위협에서 도주·두 종류 하위 개체 무제한 생성",
    },
    "fire_lord_spirit": {
        "combat_role": "하위 비행 악마 소환",
        "targeting": "소환 지점",
        "attack_shape": "5초 간격 순차 소환",
        "special_movement": "안전거리 유지·동일 하위 개체 최대 5마리 생성",
    },
}

CONCEPT_DESCRIPTION_OVERRIDES = {
    "chicken_commando": (
        "비전 강하대는 인간 마법 문명의 공수 전투원이다. 지정 지점 상공에서 "
        "낙하산을 펼친 상태로 투입되며, 높이 0.5 이하로 내려오면 낙하산 없는 "
        "지상 전투 프레임으로 전환한다. 착지 후에는 지상 대상을 추적해 근접 공격한다."
    ),
    "dimension_toad": (
        "균열두꺼비는 차원 틈새를 떠도는 야생 생물이다. 다른 차원의 마력을 먹고 "
        "알에 저장하며, `FireTadpole`과 `LightningTadpole`을 10초마다 번갈아가며 "
        "제한 없이 소환한다. 잿불 올챙이와 뇌광 올챙이는 악마나 정령이 아니라 "
        "흡수한 마력에 적응한 균열두꺼비의 새끼다."
    ),
    "fire_lord_spirit": (
        "지옥불 군단장은 전장 상공에 머무는 거대한 악마 생체 모함이다. "
        "직접 공격하지 않고 안전거리를 유지하며 `FireChildSpirit`를 5초마다 1마리씩, "
        "최대 5마리 방출한다. 소환된 비행 악마는 지상·공중 대상을 향해 "
        "`FireShot`으로 원거리 공격한다."
    ),
    "towerback": (
        "포탑등이는 인간 마법 문명이 납치한 꼬마돌의 등에 지대공 타워를 "
        "강제로 결속한 고정형 병기다. 생물 본체는 돌 골렘 부족이지만 전장에서는 "
        "인간 진영의 원거리 방어 구조물로 운용되며 지상·공중 대상을 공격한다."
    ),
}

FIXED_COMPONENTS = {
    "BubbleGeneratorMob", "Cannon", "CraterSpawner", "DummyMob",
    "LightningTowerMob", "ManaWellMob", "SummonMob", "Totem", "Tower", "Turret",
}
MOVING_COMPONENTS = {
    "AttackMob", "BehaviorMob", "BubbleSpiritMob", "ChickenCommandoMob",
    "KeepDistanceMob", "MeleeAttackMob", "NonAttackingCowardMob",
    "ProjectileRangeAttackMob", "RangeAttackMob", "SelfDestructMob",
    "SprayingAttacker", "SummonerMob", "ThunderBirdMob",
}
RANGED_COMPONENTS = {
    "BubbleSpiritMob", "Cannon", "KeepDistanceMob", "LightningTowerMob",
    "ProjectileRangeAttackMob", "RangeAttackMob", "SprayingAttacker",
    "ThunderBirdMob", "Tower", "Turret",
}
SUPPORT_COMPONENTS = {
    "FrenzyTotem", "ManaWellMob", "RallyingTorch", "SelfHealer", "Totem",
}
SUMMON_COMPONENTS = {
    "BubbleGeneratorMob", "CraterSpawner", "LimitedSequenceSpawner",
    "Spawner", "SummonMob", "SummonerMob",
}
AREA_COMPONENTS = {
    "BubbleSpiritMob", "CraterEmber", "Explode", "OvergrowthExplosion",
    "RazorGale", "ShockOverloadExplosion", "SprayingAttacker", "Tower",
    "WaterExplode",
}


def rel(path: Path) -> str:
    return path.relative_to(CLIENT_ROOT.parent).as_posix()


def first(pattern: str, text: str) -> str | None:
    match = re.search(pattern, text, re.MULTILINE | re.DOTALL)
    return match.group(1) if match else None


def unique(values: list[str]) -> list[str]:
    return list(dict.fromkeys(values))


def faction_for(bean: str) -> tuple[str, str]:
    if bean == "chicken_commando":
        return "인간 마법 문명", "같은 인간 공수대원을 낙하산 전개 상태와 지상 전투 상태로 나눈다. 두 프레임의 얼굴·복장·장비·오른쪽 방향과 45도 카메라를 동일하게 유지한다."
    if bean == "dimension_toad":
        return "차원 유랑종", "차원 틈새의 이질적인 생물 조직과 흡수한 원소 마력이 함께 보이도록 표현한다. 특정 원소 진영의 재질을 그대로 사용하지 않는다."
    if bean in {"healing_totem", "life_tree", "mana_well"}:
        return "세계수 풀 정령", "세계수의 생명력과 마나가 뿌리·잎·빛의 겹친 종이층으로 순환하게 표현한다."
    if bean == "towerback":
        return "인간 마법 문명", "따뜻한 꼬마돌 몸체와 차가운 인간제 지대공 타워를 명확히 분리한다. 가죽끈·금속 고정구로 강제 결속된 흔적을 보여준다."
    if bean == "rallying_torch":
        return "인간 마법 문명", "인간 부대가 사용하는 신호 장치. 청동 프레임과 절제된 마법 불빛으로 표현한다."
    if any(x in bean for x in ("fire", "magma", "ember", "meteor", "frenzy", "crater", "rallying")):
        return "지옥불 군단", "검붉은 종이 갑각과 내부 용암광. 불꽃 정령이 아니라 악마 군단의 마법으로 표현한다."
    if any(x in bean for x in ("water", "aqua", "bubble", "tide")):
        return "물 슬라임", "물질적인 반투명 종이층과 둥근 체적. 세계수 정령과 구분한다."
    if any(x in bean for x in ("rock", "mini")):
        return "돌 골렘 부족", "따뜻한 석재 조각과 이끼. 인간제 회색 기계와 구분한다."
    if any(x in bean for x in ("lightning", "thunder", "electric", "shock", "zap")):
        return "세계수 전기 정령", "세계수에서 갈라진 전기 생명과 각진 전격 파편을 사용한다."
    if any(x in bean for x in ("nature", "vine", "seed", "tree", "life", "overgrowth", "leaf", "will_o")):
        return "세계수 풀 정령", "잎·씨앗·덩굴의 겹친 컷페이퍼 형태로 생장 방향을 보여준다."
    if any(x in bean for x in ("wind", "storm", "cloud", "tornado", "gale", "sand")):
        return "세계수 바람 정령", "넓고 읽기 쉬운 곡선 종이 띠로 흐름과 회전을 표현한다."
    if any(x in bean for x in ("tower", "cannon", "mana")):
        return "인간 마법 문명", "차가운 석재, 스틸블루 금속, 청동 결합부. 얼굴 없는 조립 구조물로 표현한다."
    return "귀속 미정", "기존 세계관만으로 귀속을 확정할 수 없다. 시각 제작 전 설정 결정이 필요하다."


def concept_for(bean: str, family: str, faction: str) -> str:
    subject = DISPLAY_NAMES[bean]
    verbs = {
        "build": "전장에 남아 지속적으로 영향력을 만드는",
        "drop": "상공에서 목표 지점에 개입하는",
        "explode": "한순간에 범위를 장악하는",
        "shoot": "방향성과 궤적이 핵심인",
        "spawn": "독립 개체로 전장에 합류하는",
    }
    return f"{faction}의 {verbs[family]} {subject}."


def concept_description(
    display_name: str,
    faction: str,
    family: str,
    tactics: dict[str, str],
    art_rule: str,
) -> str:
    special_sentence = (
        "추가 특수 이동은 없다."
        if tactics["special_movement"] == "추가 특수 이동 없음"
        else f"특수 행동으로 {tactics['special_movement']}을 수행한다."
    )
    return (
        f"{display_name}은 {faction}의 {FAMILY_LABELS[family]} 마법이다. "
        f"{tactics['mobility_evidence']} 전투에서는 {tactics['combat_role']} 역할을 맡으며, "
        f"{tactics['targeting']} 대상을 {tactics['attack_shape']} 방식으로 다룬다. "
        f"{special_sentence} "
        f"시각적으로는 {art_rule}"
    )


def initializer_index() -> dict[str, tuple[Path, str]]:
    result: dict[str, tuple[Path, str]] = {}
    for path in PREFAB_ROOT.rglob("*PrefabInitializer.java"):
        text = path.read_text(encoding="utf-8")
        prefab = first(r"super\(\s*PrefabType\.([A-Za-z0-9_]+)", text)
        if prefab:
            result[prefab] = (path, text)
    return result


def tactical_profile(
    family: str,
    initializer_text: str,
    combined: str,
    components: list[str],
) -> dict[str, str]:
    component_set = set(components)

    if re.search(r"new\s+ZPhysics\s*\(\s*gameObject\s*,", initializer_text):
        mobility = "공중 부유형"
        mobility_evidence = "`ZPhysics(gameObject, hoverY)`로 고도를 유지한다."
    elif component_set & FIXED_COMPONENTS or family == "build":
        mobility = "고정형"
        mobility_evidence = "이동 AI 없이 설치 위치에서 행동한다."
    elif component_set & MOVING_COMPONENTS:
        mobility = "지상 이동형"
        mobility_evidence = "이동 AI가 지면 경로와 `RigidBody` 속도를 사용한다."
    elif family == "drop":
        mobility = "상공 투하형"
        mobility_evidence = "목표 상공에서 생성되어 낙하한다."
    elif family == "shoot":
        mobility = "투사체·전파형"
        mobility_evidence = "시전자에서 목표 방향으로 투사체 또는 연속 개체를 보낸다."
    elif family == "explode":
        mobility = "목표 지점 고정형"
        mobility_evidence = "목표 지점을 중심으로 범위 효과를 생성한다."
    else:
        mobility = "이동 없음"
        mobility_evidence = "소스에서 지속 이동 AI를 확인하지 못했다."

    roles = []
    if component_set & SUMMON_COMPONENTS:
        roles.append("소환")
    if component_set & SUPPORT_COMPONENTS:
        roles.append("지원")
    if "SelfDestructMob" in component_set:
        roles.append("자폭 공격")
    if component_set & RANGED_COMPONENTS:
        roles.append("원거리 공격")
    if "MeleeAttackMob" in component_set:
        roles.append("근접 공격")
    if family == "explode":
        roles.append("범위 공격")
    if family == "shoot" and not roles:
        roles.append("직접 공격")
    if not roles and family == "drop":
        roles.append("투하 공격")
    if not roles and family == "build":
        roles.append("설치물")
    if not roles and family == "spawn":
        roles.append("전투 개체")

    if "TargetMask.ANY.bit" in initializer_text:
        targets = "지상·공중"
    else:
        masks = []
        if "TargetMask.GROUND.bit" in initializer_text:
            masks.append("지상")
        if "TargetMask.AIR.bit" in initializer_text:
            masks.append("공중")
        targets = "·".join(masks) if masks else "대상 제한을 이 파일에서 직접 확인하지 못함"

    if component_set & AREA_COMPONENTS or family == "explode":
        attack_shape = "범위"
    elif "LightningTowerMob" in component_set or "ChainShot" in component_set:
        attack_shape = "다중 표적"
    elif "SequentialLineSpawner" in component_set or "VineToss" in component_set:
        attack_shape = "직선 연속"
    elif "SelfDestructMob" in component_set:
        attack_shape = "접근 후 자폭"
    elif component_set & (RANGED_COMPONENTS | {"MeleeAttackMob"}):
        attack_shape = "단일 표적"
    elif family == "shoot":
        attack_shape = "방향성"
    else:
        attack_shape = "직접 공격 없음 또는 별도 효과"

    lifecycle_bits = []
    if "TimedSelfDestroyer" in component_set:
        lifecycle_bits.append("제한시간 후 소멸")
    if component_set & MOVING_COMPONENTS:
        lifecycle_bits.append("HP 소진 시 파괴")
    if family in {"shoot", "explode", "drop"} and not lifecycle_bits:
        lifecycle_bits.append("효과 완료 후 소멸")
    if not lifecycle_bits:
        lifecycle_bits.append("소스에서 시간 제한을 직접 확인하지 못함")

    special = []
    if "PathSpawner" in component_set:
        special.append("이동 경로에 원소 장판 생성")
    if component_set & SUMMON_COMPONENTS:
        special.append("하위 개체 생성")
    if "KeepDistanceMob" in component_set:
        special.append("선호 거리까지 접근")
    if "NonAttackingCowardMob" in component_set:
        special.append("공격하지 않고 위협에서 도주")
    if "SelfHealer" in component_set:
        special.append("자가 회복")
    if "RallyingTorch" in component_set:
        special.append("아군 이동 집결")
    if not special:
        special.append("추가 특수 이동 없음")

    return {
        "mobility": mobility,
        "mobility_evidence": mobility_evidence,
        "combat_role": "·".join(unique(roles)),
        "targeting": targets,
        "attack_shape": attack_shape,
        "lifecycle": "·".join(lifecycle_bits),
        "special_movement": "·".join(special),
    }


def page_data(path: Path, initializers: dict[str, tuple[Path, str]]) -> dict:
    text = path.read_text(encoding="utf-8")
    bean = first(r'@Component\("([^"]+)"\)', text)
    if not bean:
        raise ValueError(f"Concrete magic missing @Component: {path}")
    family = path.relative_to(MAGIC_ROOT).parts[0]
    class_name = first(r"public\s+class\s+([A-Za-z0-9_]+)", text) or path.stem
    parent = first(r"extends\s+([A-Za-z0-9_]+)", text) or "Magic"
    prefabs = unique(re.findall(r"PrefabType\.([A-Za-z0-9_]+)", text))
    primary = prefabs[0] if prefabs else None

    initializer_path = None
    initializer_text = ""
    if primary and primary in initializers:
        initializer_path, initializer_text = initializers[primary]

    combined = text + "\n" + initializer_text
    initializer_components = re.findall(r"new\s+([A-Z][A-Za-z0-9_]+)\s*\(", initializer_text)
    magic_components = [
        name
        for name in re.findall(r"new\s+([A-Z][A-Za-z0-9_]+)\s*\(", text)
        if name in COMPONENT_BEHAVIOR
    ]
    components = unique(initializer_components + magic_components)
    secondary_prefabs = unique(re.findall(r"PrefabType\.([A-Za-z0-9_]+)", combined))
    if primary in secondary_prefabs:
        secondary_prefabs.remove(primary)
    parameter_keys = unique(re.findall(r"ParameterKey\.([A-Z0-9_]+)", combined))
    object_keys = unique(re.findall(r"GameObjectKey\.([A-Z0-9_]+)", combined))
    faction, art_rule = faction_for(bean)
    tactics = tactical_profile(family, initializer_text, combined, components)
    tactics.update(TACTICAL_OVERRIDES.get(bean, {}))
    display_name = DISPLAY_NAMES[bean]

    behavior = SPECIAL_BEHAVIOR.get(
        bean,
        [COMPONENT_BEHAVIOR[x] for x in components if x in COMPONENT_BEHAVIOR],
    )
    if not behavior:
        behavior = {
            "build": ["목표 지점에 지속 개체를 설치한다. 세부 행동은 프리팹 상속 구조와 런타임 파라미터에 의존한다."],
            "drop": ["상공에서 목표 지점으로 개체를 투하한다."],
            "explode": ["목표 지점 중심의 범위 효과를 생성한다."],
            "shoot": ["시전자 위치에서 목표 방향으로 투사체 또는 연속 개체를 보낸다."],
            "spawn": ["목표 지점에 아군 개체를 소환한다."],
        }[family]

    return {
        "bean": bean,
        "korean": KOREAN_NAMES.get(bean, bean.replace("_", " ")),
        "concept_name": display_name,
        "family": family,
        "class_name": class_name,
        "parent": parent,
        "magic_path": path,
        "primary": primary,
        "initializer_path": initializer_path,
        "components": components,
        "secondary_prefabs": secondary_prefabs,
        "parameter_keys": parameter_keys,
        "object_keys": object_keys,
        "behavior": unique(behavior),
        "faction": faction,
        "art_rule": art_rule,
        "concept": concept_for(bean, family, faction),
        "concept_description": CONCEPT_DESCRIPTION_OVERRIDES.get(
            bean,
            concept_description(display_name, faction, family, tactics, art_rule),
        ),
        **tactics,
    }


def bullet_code(values: list[str]) -> str:
    return ", ".join(f"`{value}`" for value in values) if values else "없음 또는 소스에서 직접 확인되지 않음"


def render_page(data: dict) -> str:
    initializer = (
        f"`{rel(data['initializer_path'])}`"
        if data["initializer_path"]
        else "직접 연결되는 initializer를 정적 분석으로 찾지 못함"
    )
    behavior = "\n".join(f"{index}. {line}" for index, line in enumerate(data["behavior"], 1))
    uncertainties = []
    if data["faction"] == "귀속 미정":
        uncertainties.append("세계관 진영 귀속 필요")
    if data["faction"] == "지옥불 군단" and "spirit" in data["bean"]:
        uncertainties.append("서버 계약은 유지하더라도 표시명과 외형에서 `정령` 표현 제거 필요")
    if "slime_nest" in data["bean"] and "water" not in data["bean"]:
        uncertainties.append("물 외 슬라임이 멸종했다는 세계관과 충돌. 둥지의 정체 재정의 필요")
    if not data["initializer_path"]:
        uncertainties.append("상속 또는 다단 생성 경로를 수동 검토해야 함")
    if not uncertainties:
        uncertainties.append("현재 확인된 세계관 충돌 없음")

    return f"""# {data['korean']}

- 서버 키: `{data['bean']}`
- 현재 이름: {data['korean']}
- 컨셉 이름: {data['concept_name']}
- 시전 계열: {FAMILY_LABELS[data['family']]}
- 진영: {data['faction']}
- 상태: game 서버 구현 확인

## 컨셉

{data['concept']}

## 컨셉 설명

{data['concept_description']}

## 설명

플레이어가 사용하는 **{FAMILY_LABELS[data['family']]} 계열 마법**이다.
게임플레이 사실은 아래 서버 코드에서 가져왔고, 세계관·아트 문장은
`.art/WORLD.md`와 `.art/STYLE.md`를 적용한 해석이다.

## 동작

{behavior}

## 전투 프로필

| 항목 | 분류 | 코드 근거 |
|---|---|---|
| 기동 방식 | {data['mobility']} | {data['mobility_evidence']} |
| 전투 역할 | {data['combat_role']} | 부착 AI·마법 컴포넌트 기준 |
| 공격 형태 | {data['attack_shape']} | 공격 컴포넌트와 시전 계열 기준 |
| 표적 | {data['targeting']} | `TargetMask` 기준 |
| 특수 이동·행동 | {data['special_movement']} | 부착 컴포넌트 기준 |
| 생명주기 | {data['lifecycle']} | 파괴·시간제한 컴포넌트 기준 |

## 서버 구조

| 항목 | 값 |
|---|---|
| 구현 클래스 | `{data['class_name']}` |
| 상위 클래스 | `{data['parent']}` |
| 주 프리팹 | {f"`{data['primary']}`" if data['primary'] else "코드에서 직접 확인되지 않음"} |
| 부가 프리팹 | {bullet_code(data['secondary_prefabs'])} |
| 부착 컴포넌트 | {bullet_code(data['components'])} |
| 파라미터 키 | {bullet_code(data['parameter_keys'])} |
| 오브젝트 파라미터 | {bullet_code(data['object_keys'])} |

수치 자체는 런타임 DB 데이터다. 이 문서는 키만 기록하며 값을 추정하지 않는다.

## 아트 방향

{data['art_rule']}

- 공통 스타일: A — 2.5D 컷페이퍼
- 생성 참조: `.art/anchors/master-v2/MasterStyleKey.png`와 가장 가까운 재질 앵커
- 실루엣은 실제 동작이 읽혀야 한다. 공격 방향, 이동 방식, 설치 여부를 장식보다 우선한다.

## 미결 사항

{chr(10).join(f"- {item}" for item in uncertainties)}

## 근거

- Magic: `{rel(data['magic_path'])}`
- Prefab initializer: {initializer}
"""


def render_index(pages: list[dict]) -> str:
    by_family: dict[str, list[dict]] = defaultdict(list)
    by_faction: dict[str, list[dict]] = defaultdict(list)
    for page in pages:
        by_family[page["family"]].append(page)
        by_faction[page["faction"]].append(page)

    family_sections = []
    for family in FAMILY_LABELS:
        rows = [
            f"| [{p['korean']}](pages/{p['bean']}.md) | `{p['bean']}` | {p['faction']} | `{p['primary'] or '-'}` |"
            for p in sorted(by_family[family], key=lambda x: x["bean"])
        ]
        family_sections.append(
            f"## {FAMILY_LABELS[family]} ({len(rows)})\n\n"
            "| 이름 | 서버 키 | 진영 | 주 프리팹 |\n"
            "|---|---|---|---|\n" + "\n".join(rows)
        )

    faction_summary = "\n".join(
        f"- {faction}: {len(items)}개"
        for faction, items in sorted(by_faction.items())
    )
    return f"""# 마법 컨셉 페이지

game 서버의 concrete `*Magic.java`를 기준으로 생성한 아트·동작 문서다.

- 생성 페이지: {len(pages)}개
- 생성기: `../tools/generate-magic-pages.py`
- 서버 수치: 미포함. 런타임 DB 파라미터 키만 기록
- 재생성: `python3 .art/tools/generate-magic-pages.py`

## 진영별 수

{faction_summary}

{"\n\n".join(family_sections)}
"""


def main() -> None:
    if not MAGIC_ROOT.exists():
        raise SystemExit(f"Game server not found: {MAGIC_ROOT}")

    initializers = initializer_index()
    pages = []
    for path in sorted(MAGIC_ROOT.rglob("*Magic.java")):
        text = path.read_text(encoding="utf-8")
        if '@Component("' not in text:
            continue
        pages.append(page_data(path, initializers))

    PAGE_ROOT.mkdir(parents=True, exist_ok=True)
    expected = set()
    for page in pages:
        output = PAGE_ROOT / f"{page['bean']}.md"
        output.write_text(render_page(page), encoding="utf-8")
        expected.add(output.name)

    for stale in PAGE_ROOT.glob("*.md"):
        if stale.name not in expected:
            stale.unlink()

    (OUTPUT_ROOT / "README.md").write_text(render_index(pages), encoding="utf-8")
    export = [
        {
            key: value
            for key, value in page.items()
            if key not in {"magic_path", "initializer_path"}
        }
        | {
            "magic_source": rel(page["magic_path"]),
            "initializer_source": rel(page["initializer_path"]) if page["initializer_path"] else None,
        }
        for page in pages
    ]
    (OUTPUT_ROOT / "data.json").write_text(
        json.dumps(export, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print(f"Generated {len(pages)} magic concept pages.")


if __name__ == "__main__":
    main()
