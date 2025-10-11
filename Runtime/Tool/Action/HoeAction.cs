using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = "GGemCo/Tools/Action/Hoe")]
    public class HoeAction : ToolAction
    {
        public override ValidationResult Validate(ActionContext ctx)
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

        public override void Execute(ActionContext ctx)
        {
            foreach (var cell in ctx.targetCells)
            {
                var tm = ctx.registry.ResolveWriteTarget(ctx.tool.writeRole, cell);
                if (!tm) continue;

                // 프로젝트 타일셋에서 실제 타일 주입
                TileBase hoed = ctx.tileset ? ctx.tileset.hoedTile : null;
                if (hoed) tm.SetTile(cell, hoed);
            }
        }
    }
}