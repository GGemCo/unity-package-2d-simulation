using UnityEngine;

namespace GGemCo2DSimulation
{
    public enum DistanceMetric { Manhattan, Chebyshev, Euclidean }
    
    /// <summary>
    /// 도구(또는 씨앗 등) 정의 데이터
    /// - 범위, 거리 계산 방식, 타겟 정책, 액션, 타일 조건 등을 정의
    /// </summary>
    [CreateAssetMenu(
        menuName = ConfigScriptableObjectSimulation.ToolDefinition.MenuName,
        order    = ConfigScriptableObjectSimulation.ToolDefinition.Ordering)]
    public class ToolDefinition : ScriptableObject
    {
        // ─────────────────────────────────────────────────────────────────────
        // Targeting
        // ─────────────────────────────────────────────────────────────────────
        [Header("Targeting (타겟팅 설정)")]

        [Tooltip("도구의 최대 사용 거리(타일 단위)\n예: 1 = 주변 1칸, 2 = 주변 2칸")]
        [Min(0)]
        public int range = 1;

        [Tooltip("거리 계산 방식\n- Manhattan: 상하좌우 거리(직선 기준)\n- Chebyshev: 대각 포함 거리\n- Euclidean: 실제 거리(제곱근 계산)")]
        public DistanceMetric metric = DistanceMetric.Manhattan;
        
        [Tooltip("도구가 작용할 셀(타일) 범위를 결정하는 타게팅 정책입니다.\n" +
                 "SingleCellTargeting : 커서 위치 1칸만 대상\n" +
                 "LineTargeting       : 플레이어→커서 방향으로 직선 영역 대상\n" +
                 "CrossAreaTargeting  : 십자형(상하좌우) 영역 대상, radius 및 중심 포함 옵션\n" +
                 "RectAreaTargeting   : 사각형 영역 대상, 크기(width/height)와 정렬(pivot) 지정 가능\n\n" +
                 "각 타게팅 정책은 ToolDefinition.range / metric 설정을 따라 거리를 제한할 수 있습니다.")]
        public TargetingPolicy targeting;


        // ─────────────────────────────────────────────────────────────────────
        // Action
        // ─────────────────────────────────────────────────────────────────────
        [Header("Action (행동 설정)")]

        [Tooltip("실행할 도구 액션 타입 (예: Hoe, Water, Axe 등)")]
        public ToolAction action;

        // [Tooltip("도구 사용 시 재생할 애니메이션 이름\nAnimator 또는 Spine 등에서 트리거할 애니메이션 클립 이름")]
        // public string animationName;

        // ─────────────────────────────────────────────────────────────────────
        // Tile Role Rules
        // ─────────────────────────────────────────────────────────────────────
        [Header("Tile Role Rules (타일 역할 설정)")]

        [Tooltip("읽기(대상) 가능한 타일 역할 마스크\n도구가 영향을 줄 수 있는 타일 유형 지정\n예: AnyGround = 모든 땅, GroundHoed 등")]
        public TileRole readRoles = TileRole.AnyGround;

        [Tooltip("차단(불가능) 역할 마스크\n이 역할에 해당하는 타일이 있으면 도구 사용 불가\n예: Blocking = 바위, 벽 등")]
        public TileRole blockRoles = TileRole.Blocking;

        [Tooltip("도구 사용 후 변경될 타일 역할\n예: 괭이 = GroundHoed, 물뿌리개 = GroundWet")]
        public TileRole writeRole = TileRole.GroundHoed;
    }
}
