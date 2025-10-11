// Runtime/Targeting/RectAreaTargeting.cs
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DSimulation
{
    public enum RectPivot { Cursor, Origin, CenteredOnCursor }

    [CreateAssetMenu(menuName = "GGemCo/Tools/Targeting/Rect Area")]
    public class RectAreaTargeting : TargetingPolicy
    {
        [Header("Rect")]
        [Min(1)] public int width = 3;
        [Min(1)] public int height = 3;

        [Tooltip("사각형 기준점: Cursor(좌하 기준), Origin(좌하 기준), CenteredOnCursor(중심 정렬)")]
        public RectPivot pivot = RectPivot.CenteredOnCursor;

        [Tooltip("원점→커서 우세축(4방향)에 맞춰 사각형을 회전할지 여부")]
        public bool alignToDominantAxis = true;

        protected override IEnumerable<Vector3Int> OnGetCellsInternal(ActionContext ctx)
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