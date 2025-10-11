// Runtime/Targeting/LineTargeting.cs
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = "GGemCo/Tools/Targeting/Line")]
    public class LineTargeting : TargetingPolicy
    {
        [Header("Line")]
        [Tooltip("원점에서 출발하여 커서 방향으로 직선을 긋습니다.")]
        public bool includeOrigin = false;

        protected override IEnumerable<Vector3Int> OnGetCellsInternal(ActionContext ctx)
        {
            var from = includeOrigin ? ctx.originCell : StepTowards(ctx.originCell, ctx.cursorCell);
            var to   = ctx.cursorCell;

            foreach (var p in Bresenham(from, to))
                yield return p;
        }

        // 한 칸 전진 (원점과 커서가 같은 셀이면 원점 반환)
        private static Vector3Int StepTowards(Vector3Int a, Vector3Int b)
        {
            var dx = Mathf.Clamp(b.x - a.x, -1, 1);
            var dy = Mathf.Clamp(b.y - a.y, -1, 1);
            return new Vector3Int(a.x + dx, a.y + dy, 0);
        }

        // Bresenham line on grid
        private static IEnumerable<Vector3Int> Bresenham(Vector3Int a, Vector3Int b)
        {
            int x0 = a.x, y0 = a.y, x1 = b.x, y1 = b.y;
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                yield return new Vector3Int(x0, y0, 0);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }
    }
}