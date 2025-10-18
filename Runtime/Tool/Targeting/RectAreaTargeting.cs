using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DSimulation
{
    public enum RectPivot { Cursor, Origin, CenteredOnCursor }

    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.ToolTargetingRect.MenuName, order = ConfigScriptableObjectSimulation.ToolTargetingRect.Ordering)]
    public class RectAreaTargeting : TargetingPolicy
    {
        [Header("Rect Area Settings")]

        [Tooltip("사각형의 가로 크기(타일 단위)입니다. 값이 1이면 폭이 1칸인 세로줄 형태가 됩니다.")]
        [Min(1)] public int width = 3;

        [Tooltip("사각형의 세로 크기(타일 단위)입니다. 값이 1이면 높이가 1칸인 가로줄 형태가 됩니다.")]
        [Min(1)] public int height = 3;

        [Tooltip("사각형의 기준점을 결정합니다.\n" +
                 "- Cursor : 커서 위치를 좌하단 기준으로 사용\n" +
                 "- Origin : 플레이어 위치를 좌하단 기준으로 사용\n" +
                 "- CenteredOnCursor : 커서를 중심으로 정렬")]
        public RectPivot pivot = RectPivot.CenteredOnCursor;

        [Tooltip("원점에서 커서 방향(상/하/좌/우)에 따라 사각형을 회전시킬지 여부입니다.\n" +
                 "활성화하면 도구의 방향에 맞춰 폭과 높이가 자동으로 맞춰집니다.")]
        public bool alignToDominantAxis = true;

        protected override IEnumerable<Vector3Int> OnGetCellsInternal(ToolActionContext ctx)
        {
            var cells = new List<Vector3Int>();
            var axis = DominantAxis(ctx.originCell, ctx.cursorCell); // (1,0),(-1,0),(0,1),(0,-1)

            int w = Mathf.Max(1, width);
            int h = Mathf.Max(1, height);

            // 축 정렬: 가로/세로를 우세축에 맞게 스왑
            bool horizontal = axis.x != 0;
            if (alignToDominantAxis && !horizontal)
            {
                var t = w; w = h; h = t;
            }

            Vector3Int start;
            switch (pivot)
            {
                case RectPivot.Cursor:
                    start = ctx.cursorCell;
                    break;
                case RectPivot.Origin:
                    start = ctx.originCell;
                    break;
                default: // CenteredOnCursor
                    start = new Vector3Int(ctx.cursorCell.x - (w / 2), ctx.cursorCell.y - (h / 2), 0);
                    break;
            }

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                cells.Add(new Vector3Int(start.x + x, start.y + y, 0));
            }

            return cells;
        }
    }
}