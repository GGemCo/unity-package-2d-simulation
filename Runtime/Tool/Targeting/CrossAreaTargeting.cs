// Runtime/Targeting/CrossAreaTargeting.cs
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = "GGemCo/Tools/Targeting/Cross Area")]
    public class CrossAreaTargeting : TargetingPolicy
    {
        [Header("Cross")]
        [Min(1)] public int radius = 1;
        [Tooltip("중앙(커서) 포함 여부")] public bool includeCenterCell = true;

        protected override IEnumerable<Vector3Int> OnGetCellsInternal(ActionContext ctx)
        {
            var c = ctx.cursorCell;
            if (includeCenterCell) yield return c;

            for (int i = 1; i <= radius; i++)
            {
                yield return new Vector3Int(c.x + i, c.y, 0);
                yield return new Vector3Int(c.x - i, c.y, 0);
                yield return new Vector3Int(c.x, c.y + i, 0);
                yield return new Vector3Int(c.x, c.y - i, 0);
            }
        }
    }
}