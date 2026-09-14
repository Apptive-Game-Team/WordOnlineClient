using System;
using System.Collections.Generic;
using Data;
using Data.GameConfig;
using Data.Magic;
using Global;
using Global.Serialization;
using UnityEngine;

namespace GameScene
{
    /// <summary>indicator layer 하나를 그릴 수 있는 도형으로 푼 결과.</summary>
    public readonly struct ResolvedIndicatorShape
    {
        public enum Kind
        {
            Circle,
            Lane
        }

        public readonly Kind kind;

        /// <summary>circle 이면 중심, lane 이면 시작점.</summary>
        public readonly Vector3 origin;

        /// <summary>lane 이 향하는 점. <see cref="SkillIndicatorShapeRenderer.SetLine"/> 은 방향만 쓴다.</summary>
        public readonly Vector3 target;

        /// <summary>circle 반지름.</summary>
        public readonly float radius;

        /// <summary>lane 길이.</summary>
        public readonly float length;

        /// <summary>lane 의 중심선에서 가장자리까지 거리.</summary>
        public readonly float halfWidth;

        /// <summary>circle 테두리 두께. 0 이면 테두리를 그리지 않는다.</summary>
        public readonly float edgeWidth;

        /// <summary>circle 속을 채우는지. false 면 테두리만 남는 링이다.</summary>
        public readonly bool fill;

        private ResolvedIndicatorShape(Kind kind, Vector3 origin, Vector3 target,
            float radius, float length, float halfWidth, float edgeWidth, bool fill)
        {
            this.kind = kind;
            this.origin = origin;
            this.target = target;
            this.radius = radius;
            this.length = length;
            this.halfWidth = halfWidth;
            this.edgeWidth = edgeWidth;
            this.fill = fill;
        }

        /// <summary>
        /// document layer 가 내는 원. 계약대로 <paramref name="edgeWidth"/> 가 0 이면 속을 채우고,
        /// 0 보다 크면 그 두께의 링을 그린다.
        /// </summary>
        public static ResolvedIndicatorShape Circle(Vector3 center, float radius, float edgeWidth)
        {
            return new ResolvedIndicatorShape(Kind.Circle, center, center, radius, 0f, 0f, edgeWidth,
                edgeWidth <= 0f);
        }

        /// <summary>
        /// 속을 채우면서 테두리도 두르는 원. 이 변경 전 <c>attack_range</c> 원이 그랬으므로 fallback 만 쓴다.
        /// document 계약에는 이 조합이 없다.
        /// </summary>
        public static ResolvedIndicatorShape FilledCircleWithEdge(Vector3 center, float radius, float edgeWidth)
        {
            return new ResolvedIndicatorShape(Kind.Circle, center, center, radius, 0f, 0f, edgeWidth, true);
        }

        public static ResolvedIndicatorShape Lane(Vector3 start, Vector3 target, float length, float halfWidth)
        {
            return new ResolvedIndicatorShape(Kind.Lane, start, target, 0f, length, halfWidth, 0f, true);
        }
    }

    /// <summary>
    /// 마법의 indicator document 와 지금 위치를 받아 그릴 도형 목록을 내놓는다.
    /// <para>
    /// <see cref="FieldSelector"/> 는 입력과 GameObject 수명만 맡고, 무엇을 그릴지는 여기서 정한다.
    /// </para>
    /// <para>
    /// 결과는 호출자가 들고 있는 list 에 담는다. 이 경로는 매 프레임 돌기 때문에
    /// list 를 새로 만들어 돌려주면 프레임마다 GC 가 생긴다.
    /// </para>
    /// </summary>
    public static class MagicIndicatorResolver
    {
        /// <summary>indicator document 가 없을 때 조준 도형의 반지름/절반 폭으로 쓰는 parameter 이름.</summary>
        private const string FallbackRadiusParameter = "radius";

        /// <summary>indicator document 가 없을 때 위협 범위의 크기로 쓰는 parameter 이름.</summary>
        private const string FallbackAttackRangeParameter = "attack_range";

        /// <summary>
        /// <c>attack_range</c> 가 없거나 0 일 때 위협 범위 크기로 대신 읽는 parameter 이름.
        /// grass_generator, repair_totem, shock_trap 은 attack_range 행이 없고 대신 이 값으로
        /// 효과 범위를 알린다 (issue #667).
        /// </summary>
        private const string FallbackEffectRadiusParameter = "effect_radius";

        /// <summary>indicator document 가 없을 때 위협 범위 원을 전방으로 밀어내는 parameter 이름.</summary>
        private const string FallbackAttackOffsetParameter = "attack_offset";

        /// <summary>
        /// indicator document 가 없을 때 위협 범위를 lane 으로 그리는 유일한 마법 이름.
        /// 이 분기를 없애려고 document 를 도입했다 — document 를 받은 마법은 여기에 오지 않는다.
        /// electric_tower, crater, rock_turret, cannon 은 document 를 받기 전까지 원으로 그려진다 (issue #581).
        /// </summary>
        private const string FallbackLaneAttackMagicServerName = "dragon_tower";

        /// <summary>
        /// dragon_tower 위협 lane 의 절반 폭. 실제 폭발 범위는 서버의 dragon_flame.radius 이고
        /// 클라이언트는 그 값을 읽을 수 없다. lane 이 어디로 향하는지 보여 주는 힌트일 뿐이다 (issue #581).
        /// </summary>
        private const float FallbackAttackLaneHalfWidth = 0.4f;

        /// <summary>위협 범위 원의 테두리 굵기. 테두리 없는 설치 지점 원과 구분하는 용도다.</summary>
        private const float FallbackAttackRangeEdgeWidth = 0.08f;

        private const string LeftPlayerMaster = "LeftPlayer";

        // document 없는 마법을 마법당 한 번만 log 하기 위한 표. 프레임마다 경고가 쏟아지면 안 된다.
        private static readonly HashSet<string> LoggedMissingDocument = new HashSet<string>();

        /// <summary>
        /// 시전자가 보는 방향. 필드는 X 축으로 놓여 있다. <c>InteractiveTutorialScene.unity</c> 에서
        /// LeftPlayer 가 x=1, RightPlayer 가 x=17 이고 필드는 월드 X ∈ [0, 18] 이므로,
        /// LeftPlayer 는 X 가 커지는 쪽(<see cref="Vector3.right"/>),
        /// RightPlayer 는 X 가 작아지는 쪽(<see cref="Vector3.left"/>)을 본다.
        /// 서버 WindPushComponent 가 쓰는 규칙과 같다.
        /// </summary>
        public static Vector3 GetForwardDirection()
        {
            return SceneContext.Me == LeftPlayerMaster ? Vector3.right : Vector3.left;
        }

        /// <summary>
        /// <paramref name="shapes"/> 를 비우고 이 프레임에 그릴 도형을 순서대로 채운다.
        /// 값을 풀지 못한 layer 는 건너뛰고 나머지는 그대로 그린다.
        /// </summary>
        /// <param name="range">마법의 cast range. <c>end: "target"</c> lane 의 길이다.</param>
        public static void Resolve(
            CombinedMagicData magic,
            Vector3 casterPosition,
            Vector3 targetPosition,
            Vector3 forward,
            float range,
            List<ResolvedIndicatorShape> shapes)
        {
            shapes.Clear();
            if (magic == null)
            {
                return;
            }

            MagicIndicatorDocument document = magic.indicator;
            if (document == null || !document.Prepare(magic.serverName))
            {
                AppendFallbackShapes(magic, casterPosition, targetPosition, forward, range, shapes);
                return;
            }

            for (int i = 0; i < document.layers.Count; i++)
            {
                MagicIndicatorLayer layer = document.layers[i];
                if (layer == null || !layer.IsReadable)
                {
                    continue;
                }

                if (TryResolveLayer(magic, layer, casterPosition, targetPosition, forward, range,
                        out ResolvedIndicatorShape shape))
                {
                    shapes.Add(shape);
                }
            }
        }

        /// <summary>
        /// document 가 없거나 읽을 수 없는 마법은 이 변경 전 <see cref="FieldSelector"/> 가 그리던 것을
        /// 그대로 그린다. 도형이 오늘과 같은 순서로 나온다.
        /// <list type="number">
        /// <item><description>
        /// 조준 도형. <c>castType == Shoot</c> 이면 시전자에서 조준점까지 lane, 아니면 조준점에 원이고
        /// 크기는 둘 다 <c>radius</c> parameter 다.
        /// </description></item>
        /// <item><description>
        /// 위협 범위. <c>attack_range</c> 가 0 보다 클 때만 붙고, dragon_tower 는 lane, 나머지는
        /// <c>attack_offset</c> 만큼 전방으로 민, 속을 채우고 테두리를 두른 원이다.
        /// </description></item>
        /// </list>
        /// <para>
        /// database 와 lobby 가 아직 <c>indicator</c> 를 보내지 않으므로 지금은 이 경로가 실제로 도는 경로다.
        /// </para>
        /// </summary>
        private static void AppendFallbackShapes(
            CombinedMagicData magic,
            Vector3 casterPosition,
            Vector3 targetPosition,
            Vector3 forward,
            float range,
            List<ResolvedIndicatorShape> shapes)
        {
            if (LoggedMissingDocument.Add(magic.serverName))
            {
                WDebug.LogWarning(
                    $"[MagicIndicator] {magic.serverName}: no usable indicator document, drawing the pre-document shapes.");
            }

            // radius 가 표에 없으면 0 이다. 이 변경 전 FieldSelector 도 못 찾은 radius 를 0 으로 넘겼고,
            // SkillIndicatorShapeRenderer 는 반지름 0 인 원을 감추고 폭 0 인 lane 은 0.01 로 올려 그린다.
            GameParameterResolver.TryGetMagicParameter(magic, FallbackRadiusParameter, out float radius);

            shapes.Add(magic.castType == CardType.Shoot
                ? ResolvedIndicatorShape.Lane(casterPosition, targetPosition, range, radius)
                : ResolvedIndicatorShape.Circle(targetPosition, radius, 0f));

            AppendFallbackAttackShape(magic, targetPosition, forward, shapes);
        }

        /// <summary>
        /// 설치 지점과 별개인 위협 범위. <c>attack_range</c> 가 없거나 0 이면 <c>effect_radius</c> 로
        /// 대신 그린다. 둘 다 없거나 0 이면 아무것도 붙지 않는다.
        /// </summary>
        private static void AppendFallbackAttackShape(
            CombinedMagicData magic,
            Vector3 targetPosition,
            Vector3 forward,
            List<ResolvedIndicatorShape> shapes)
        {
            if (!GameParameterResolver.TryGetMagicParameter(magic, FallbackAttackRangeParameter, out float attackRange) ||
                attackRange <= 0f)
            {
                if (!GameParameterResolver.TryGetMagicParameter(magic, FallbackEffectRadiusParameter, out attackRange) ||
                    attackRange <= 0f)
                {
                    return;
                }
            }

            if (string.Equals(magic.serverName, FallbackLaneAttackMagicServerName, StringComparison.OrdinalIgnoreCase))
            {
                shapes.Add(ResolvedIndicatorShape.Lane(
                    targetPosition, targetPosition + forward, attackRange, FallbackAttackLaneHalfWidth));
                return;
            }

            // attack_offset 이 없으면 0 으로 보고 설치 지점 중심에 그린다.
            GameParameterResolver.TryGetMagicParameter(magic, FallbackAttackOffsetParameter, out float attackOffset);
            shapes.Add(ResolvedIndicatorShape.FilledCircleWithEdge(
                targetPosition + forward * attackOffset, attackRange, FallbackAttackRangeEdgeWidth));
        }

        private static bool TryResolveLayer(
            CombinedMagicData magic,
            MagicIndicatorLayer layer,
            Vector3 casterPosition,
            Vector3 targetPosition,
            Vector3 forward,
            float range,
            out ResolvedIndicatorShape shape)
        {
            shape = default;
            Vector3 origin = layer.Origin == MagicIndicatorOrigin.Caster ? casterPosition : targetPosition;

            if (layer.Shape == MagicIndicatorShape.Circle)
            {
                return TryResolveCircle(magic, layer, origin, forward, out shape);
            }

            return TryResolveLane(magic, layer, origin, targetPosition, forward, range, out shape);
        }

        private static bool TryResolveCircle(
            CombinedMagicData magic,
            MagicIndicatorLayer layer,
            Vector3 origin,
            Vector3 forward,
            out ResolvedIndicatorShape shape)
        {
            shape = default;
            if (!TryResolveValue(magic, layer.radius, out float radius))
            {
                return false;
            }

            // forwardOffset 과 edgeWidth 는 없으면 0 이다. 다만 적혀 있는데 값을 풀지 못하면
            // 0 으로 때우지 않고 layer 를 건너뛴다. 0 으로 그려도 되는 자리는 document 가 fallback 을 적는다.
            if (!TryResolveOptionalValue(magic, layer.forwardOffset, out float forwardOffset))
            {
                return false;
            }

            if (!TryResolveOptionalValue(magic, layer.edgeWidth, out float edgeWidth))
            {
                return false;
            }

            shape = ResolvedIndicatorShape.Circle(origin + forward * forwardOffset, radius, edgeWidth);
            return true;
        }

        private static bool TryResolveLane(
            CombinedMagicData magic,
            MagicIndicatorLayer layer,
            Vector3 origin,
            Vector3 targetPosition,
            Vector3 forward,
            float range,
            out ResolvedIndicatorShape shape)
        {
            shape = default;
            if (!TryResolveValue(magic, layer.halfWidth, out float halfWidth))
            {
                return false;
            }

            if (layer.End == MagicIndicatorLaneEnd.Target)
            {
                // 이 변경 전의 직선 조준과 같다. 조준점 쪽을 향하고 길이는 마법의 cast range 다.
                shape = ResolvedIndicatorShape.Lane(origin, targetPosition, range, halfWidth);
                return true;
            }

            if (!TryResolveValue(magic, layer.length, out float length))
            {
                return false;
            }

            shape = ResolvedIndicatorShape.Lane(origin, origin + forward, length, halfWidth);
            return true;
        }

        /// <summary>
        /// 숫자면 그대로, parameter 이름이면 parameter 표에서 읽는다. object 이름이 함께 있으면
        /// 마법 자신의 game object 가 아니라 그 이름의 game object 에서 읽는다.
        /// 못 찾고 fallback 도 없으면 풀지 못한 것이다.
        /// </summary>
        private static bool TryResolveValue(CombinedMagicData magic, in MagicIndicatorValue value, out float resolved)
        {
            resolved = 0f;
            if (!value.IsPresent)
            {
                return false;
            }

            if (!value.IsParameter)
            {
                resolved = value.Number;
                return true;
            }

            bool found = value.HasObject
                ? GameParameterResolver.TryGetObjectParameter(value.ObjectName, value.ParameterName, out resolved)
                : GameParameterResolver.TryGetMagicParameter(magic, value.ParameterName, out resolved);

            if (found)
            {
                return true;
            }

            if (value.HasFallback)
            {
                resolved = value.FallbackNumber;
                return true;
            }

            resolved = 0f;
            return false;
        }

        /// <summary>적혀 있지 않으면 0 으로 성공. 적혀 있으면 <see cref="TryResolveValue"/> 와 같다.</summary>
        private static bool TryResolveOptionalValue(
            CombinedMagicData magic,
            in MagicIndicatorValue value,
            out float resolved)
        {
            if (!value.IsPresent)
            {
                resolved = 0f;
                return true;
            }

            return TryResolveValue(magic, value, out resolved);
        }
    }
}
