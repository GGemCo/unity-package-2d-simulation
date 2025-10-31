using GGemCo2DCore;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.ToolActionSickle.MenuName, order = ConfigScriptableObjectSimulation.ToolActionSickle.Ordering)]
    public class ToolActionSickle : ToolAction
    {
        private TableItem _tableItem;
        
        public override ValidationResult Validate(ToolActionContext ctx)
        {
            _tableItem ??= TableLoaderManager.Instance.TableItem;
            
            var vr = new ValidationResult();
            foreach (var cell in ctx.targetCells)
            {
                bool blocked = ctx.registry.AnyTileAt(cell, ctx.tool.blockRoles);
                bool hasGround = ctx.registry.AnyTileAt(cell, ctx.tool.readRoles);
                if (!blocked && hasGround)
                {
                    int seedItemUid   = ctx.gridInformation.GetIntSafe(cell, ConfigGridInformationKey.KeySeedItemUid);
                    int seedStep   = ctx.gridInformation.GetIntSafe(cell, ConfigGridInformationKey.KeySeedStep);
                    
                    if (seedItemUid != -1 && seedStep != -1)
                    {
                        var info = _tableItem.GetDataByUid(seedItemUid);
                        if (info.IsSubCategoryScytheHarvestable())
                        {
                            var key = $"{ConfigAddressableKey.SimulationGrowth}_{seedItemUid}";
                            GrowthBase growthBase = AddressableLoaderGrowth.Instance.GetGrowthBaseByName(key);
                            if (growthBase == null)
                            {
                                GcLogger.LogError($"성장 정보 스크립터블 오브젝트가 없습니다. 씨앗 item Uid: {seedItemUid}");
                                vr.InvalidCells.Add(cell);
                                continue;
                            }

                            if (seedStep >= growthBase.struckGrowthConditions.Count - 1)
                            {
                                vr.ValidCells.Add(cell);
                            }
                            else
                            {
                                vr.InvalidCells.Add(cell);
                            }
                        }
                        else
                        {
                            vr.InvalidCells.Add(cell);
                        }
                    }
                    else
                    {
                        vr.InvalidCells.Add(cell);
                    }
                }
                else
                {
                    vr.InvalidCells.Add(cell);
                }
            }
            vr.IsValid = vr.ValidCells.Count > 0 && vr.InvalidCells.Count == 0;
            if (!vr.IsValid) vr.Reason = "Blocked or no ground.";
            return vr;
        }

        public override void Execute(ToolActionContext ctx)
        {
            var info = ctx.gridInformation;
            if (!info)
            {
                Debug.LogWarning("[SickleAction] GridInformation이 필요합니다.");
                return;
            }
            
            foreach (var cell in ctx.targetCells)
            {
                var tm = ctx.registry.ResolveWriteTarget(ctx.tool.writeRole, cell);
                if (!tm) continue;

                int seedItemUid   = info.GetIntSafe(cell, ConfigGridInformationKey.KeySeedItemUid);
                int seedStep   = info.GetIntSafe(cell, ConfigGridInformationKey.KeySeedStep);
                if (seedItemUid == -1 || seedStep == -1) continue;
                var infoItem = _tableItem.GetDataByUid(seedItemUid);
                if (!infoItem.IsSubCategoryScytheHarvestable()) continue;
                
                GcLogger.Log($"seedItemUid: {seedItemUid}");
                
                var key = $"{ConfigAddressableKey.SimulationGrowth}_{seedItemUid}";
                GrowthBase growthBase = AddressableLoaderGrowth.Instance.GetGrowthBaseByName(key);
                if (growthBase == null)
                {
                    GcLogger.LogError($"성장 정보 스크립터블 오브젝트가 없습니다. 씨앗 item Uid: {seedItemUid}");
                    return;
                }

                if (seedStep < growthBase.struckGrowthConditions.Count - 1)
                {
                    GcLogger.LogError($"아직 다 성장하지 않았습니다. 씨앗 item Uid: {seedItemUid}, seedStep: {seedStep} < Count: {growthBase.struckGrowthConditions.Count - 1}");
                    return;
                }

                // 프로젝트 타일셋에서 실제 타일 주입
                TileBase empty = ctx.defaultTileEmpty ? ctx.defaultTileEmpty : null;
                if (!empty) return;
                
                tm.SetTile(cell, empty);
                
                info.ErasePositionProperty(cell, ConfigGridInformationKey.KeySeedItemUid);
                info.ErasePositionProperty(cell, ConfigGridInformationKey.KeySeedStep);
                info.ErasePositionProperty(cell, ConfigGridInformationKey.KeySeedStartDate);
                info.ErasePositionProperty(cell, ConfigGridInformationKey.KeyWet);
                ctx.dirtyTracker.MarkErased(info, cell, ConfigGridInformationKey.KeySeedItemUid);
                ctx.dirtyTracker.MarkErased(info, cell, ConfigGridInformationKey.KeySeedStep);
                ctx.dirtyTracker.MarkErased(info, cell, ConfigGridInformationKey.KeySeedStartDate);
                ctx.dirtyTracker.MarkErased(info, cell, ConfigGridInformationKey.KeyWet);
                
                Vector2 point = ctx.grid.GetCellCenterWorld(cell);
                SceneGame.Instance.ItemManager.MakeDropItem(point, growthBase.resultUid, 1);
            }
        }
    }
}