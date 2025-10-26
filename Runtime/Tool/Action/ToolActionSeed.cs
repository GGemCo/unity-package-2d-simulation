using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.ToolActionSeed.MenuName, order = ConfigScriptableObjectSimulation.ToolActionSeed.Ordering)]
    public class ToolActionSeed : ToolAction
    {
        public override ValidationResult Validate(ToolActionContext ctx)
        {
            var vr = new ValidationResult();
            foreach (var cell in ctx.targetCells)
            {
                // 이미 씨앗이 있으면 false
                var seedItemUid = ctx.gridInformation.GetPositionProperty(cell, ConfigGridInformationKey.KeySeedItemUid, -1);
                if (seedItemUid != -1)
                {
                    vr.InvalidCells.Add(cell);
                    continue;
                }

                bool blocked = ctx.registry.AnyTileAt(cell, ctx.tool.blockRoles);
                bool hasGround = ctx.registry.AnyTileAt(cell, ctx.tool.readRoles);
                if (!blocked && hasGround) vr.ValidCells.Add(cell);
                else                       vr.InvalidCells.Add(cell);
            }
            vr.IsValid = vr.ValidCells.Count > 0 && vr.InvalidCells.Count == 0;
            if (!vr.IsValid) vr.Reason = "Blocked or no hoed.";
            return vr;
        }

        public override void Execute(ToolActionContext ctx)
        {
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
                if (ctx.itemUid <= 0)
                {
                    GcLogger.LogError($"씨앗을 들고 있지 않습니다.");
                    continue;
                }
                
                var key = $"{ConfigAddressableKey.SimulationGrowth}_{ctx.itemUid}";
                GrowthBase growthBase = AddressableLoaderGrowth.Instance.GetGrowthBaseByName(key);
                if (growthBase == null)
                {
                    GcLogger.LogError($"Addressables에 {key} 키로 등록된 GrowthBase 스크립터블 오브젝트가 없습니다. itemUid: {ctx.itemUid}");
                    continue;
                }

                int startStep = 0;
                if (startStep >= growthBase.struckGrowthConditions.Count) continue;
                
                TileBase tile = growthBase.struckGrowthConditions[startStep].tile;

                if (!tile) continue;
                info.SetPositionProperty(cell, ConfigGridInformationKey.KeySeedStep, startStep);
                info.SetPositionProperty(cell, ConfigGridInformationKey.KeySeedItemUid, ctx.itemUid);
                tm.SetTile(cell, tile);
                ctx.dirtyTracker.MarkDirty(info, cell);
            }
        }
    }
}