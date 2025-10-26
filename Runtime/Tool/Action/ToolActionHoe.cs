using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.ToolActionHoe.MenuName, order = ConfigScriptableObjectSimulation.ToolActionHoe.Ordering)]
    public class ToolActionHoe : ToolAction
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
            if (ctx.defaultTileHoe == null) return;

            var info = ctx.gridInformation;
            if (!info)
            {
                Debug.LogWarning("[WaterAction] GridInformation이 필요합니다.", ctx.grid);
                return;
            }
            
            foreach (var cell in ctx.targetCells)
            {
                var tm = ctx.registry.ResolveWriteTarget(ctx.tool.writeRole, cell);
                if (!tm) continue;

                // 프로젝트 타일셋에서 실제 타일 주입
                TileBase hoed = ctx.defaultTileHoe ? ctx.defaultTileHoe : null;
                if (!hoed) return;
                tm.SetTile(cell, hoed);
                info.SetPositionProperty(cell, ConfigGridInformationKey.KeyHoed, 1);
                ctx.dirtyTracker.MarkDirty(info, cell);
            }
        }
    }
}