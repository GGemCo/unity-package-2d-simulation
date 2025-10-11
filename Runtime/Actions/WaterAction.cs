using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = "GGemCo/Tools/Action/Water")]
    public class WaterAction : ToolAction
    {
        public override ValidationResult Validate(ActionContext ctx)
        {
            var vr = new ValidationResult();
            foreach (var cell in ctx.targetCells)
            {
                bool blocked = ctx.registry.AnyTileAt(cell, ctx.tool.blockRoles);
                if (!blocked) vr.ValidCells.Add(cell);
                else          vr.InvalidCells.Add(cell);
            }
            vr.IsValid = vr.ValidCells.Count > 0 && vr.InvalidCells.Count == 0;
            return vr;
        }

        public override void Execute(ActionContext ctx)
        {
            foreach (var cell in ctx.targetCells)
            {
                var tm = ctx.registry.ResolveWriteTarget(TileRole.GroundWet, cell);
                TileBase wet = ctx.tileset ? ctx.tileset.wetTile : null;
                if (tm && wet) tm.SetTile(cell, wet);
            }
        }
    }
}