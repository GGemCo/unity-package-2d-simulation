using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// 타게팅 정책의 추상 기반.
    /// - ToolDefinition.range / metric 을 준수(respectRange)할 수 있음
    /// - 모든 구현은 OnGetCellsInternal 에서 셀을 산출하고, 공통 필터/중복 제거 수행
    /// </summary>
    public abstract class TargetingPolicy : ScriptableObject
    {
        [Header("Common")]
        [Tooltip("true면 ToolDefinition.range/metric을 적용하여 범위를 초과한 셀은 제외합니다.")]
        public bool respectRange = true;

        [Tooltip("커서 셀을 결과에 반드시 포함할지 여부")]
        public bool includeCursor = true;

        /// <summary> 결과 셀 집합을 계산합니다. </summary>
        public HashSet<Vector3Int> GetCells(ToolActionContext ctx)
        {
            var raw = OnGetCellsInternal(ctx);
            var result = new HashSet<Vector3Int>();

            foreach (var c in raw)
            {
                if (respectRange && !GridProbe.InRange(ctx.originCell, c, ctx.tool.range, ctx.tool.metric))
                    continue;
                result.Add(c);
            }

            if (includeCursor)
                result.Add(ctx.cursorCell);

            return result;
        }

        /// <summary> 파생 클래스가 실제 모양을 산출합니다. range/metric 필터는 상위에서 처리. </summary>
        protected abstract IEnumerable<Vector3Int> OnGetCellsInternal(ToolActionContext ctx);

        /// <summary> 원점→커서의 4방향(상/하/좌/우) 정렬 축을 반환합니다. </summary>
        protected static Vector2Int DominantAxis(Vector3Int origin, Vector3Int cursor)
        {
            var dx = cursor.x - origin.x;
            var dy = cursor.y - origin.y;
            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
                return new Vector2Int(dx >= 0 ? 1 : -1, 0);
            return new Vector2Int(0, dy >= 0 ? 1 : -1);
        }
    }
}