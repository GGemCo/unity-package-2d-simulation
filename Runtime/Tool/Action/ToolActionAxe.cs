using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.ToolActionAxe.MenuName, order = ConfigScriptableObjectSimulation.ToolActionAxe.Ordering)]
    public class ToolActionAxe : ToolAction
    {
        public override ValidationResult Validate(ToolActionContext ctx)
        {
            var vr = new ValidationResult();
            foreach (var cell in ctx.targetCells)
            {
                bool blocked = ctx.registry.AnyTileAt(cell, ctx.tool.blockRoles);
                bool hasGround = ctx.registry.AnyTileAt(cell, ctx.tool.readRoles);
                if (!blocked && hasGround) vr.ValidCells.Add(cell);
                else                       vr.InvalidCells.Add(cell);
            }
            vr.IsValid = vr.ValidCells.Count > 0 && vr.InvalidCells.Count == 0;
            if (!vr.IsValid) vr.Reason = "Blocked or no ground.";
            return vr;
        }

        public override void Execute(ToolActionContext ctx)
        {
            foreach (var cell in ctx.targetCells)
            {
                var tm = ctx.registry.ResolveWriteTarget(ctx.tool.writeRole, cell);
                if (!tm) continue;
            }
        }
    }
}