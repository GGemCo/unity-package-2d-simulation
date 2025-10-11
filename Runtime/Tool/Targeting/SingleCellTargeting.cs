// Runtime/Targeting/SingleCellTargeting.cs
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = "GGemCo/Tools/Targeting/Single Cell")]
    public class SingleCellTargeting : TargetingPolicy
    {
        protected override IEnumerable<Vector3Int> OnGetCellsInternal(ActionContext ctx)
        {
            // 커서 셀만 대상으로. includeCursor 옵션이 true면 상위에서 중복 처리됨.
            yield return ctx.cursorCell;
        }
    }
}