using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    public class TempTest : MonoBehaviour
    {
        /// <summary>
        /// 잠자기
        /// </summary>
        public void OnClickSleep()
        {
            if (!SceneGame.Instance) return;
            var grid = SceneGame.Instance.mapManager.GetGrid();
            if (grid == null) return;
            var grindInfo = grid.GetComponent<GridInformation>();
            if (grindInfo == null) return;
            var autoTilemapRegistry = grid.GetComponent<AutoTilemapRegistry>();
            if (autoTilemapRegistry == null) return;
            var tilemap = autoTilemapRegistry.GetTop(ConfigCommonSimulation.TileRole.GroundGrowth);
            if (tilemap == null) return;
            var cells = grindInfo.GetAllPositions(ConfigGridInformationKey.KeySeedStep);
            
            foreach (var cell in cells)
            {
                int step = grindInfo.GetPositionProperty(cell, ConfigGridInformationKey.KeySeedStep, -1);
                if (step == -1) continue;
                int itemUid = grindInfo.GetPositionProperty(cell, ConfigGridInformationKey.KeySeedItemUid, -1);
                if (itemUid == -1) continue;
                int countWater = grindInfo.GetPositionProperty(cell, ConfigGridInformationKey.KeyWetCount, -1);
                
                var key = $"{ConfigAddressableKey.SimulationGrowth}_{itemUid}";
                GrowthBase growthBase = AddressableLoaderGrowth.Instance.GetGrowthBaseByName(key);
                if (growthBase == null)
                {
                    GcLogger.LogError($"Addressables에 {key} 키로 등록된 GrowthBase 스크립터블 오브젝트가 없습니다. itemUid: {itemUid}");
                    return;
                }

                if (countWater > 0)
                {
                    step++;
                }
                
                if (step >= growthBase.struckGrowthConditions.Count) continue;

                // 다음 step 조건 체크하기
                StruckGrowthNeed nextStruckGrowthNeed = growthBase.struckGrowthConditions[step];
                // 물주기 
                if (nextStruckGrowthNeed.needType == GrowthNeedType.Watering && nextStruckGrowthNeed.needValue > 0)
                {
                    if (countWater >= nextStruckGrowthNeed.needValue)
                    {
                        
                    }
                    else
                    {
                        // GcLogger.LogError($"물주기를 안했습니다.");
                        continue;
                    }
                }
                
                // GcLogger.Log($"잠자기 성공. cell: {cell}, 현재 스텝: {step}");
                TileBase tile = growthBase.struckGrowthConditions[step].tile;
                grindInfo.SetPositionProperty(cell, ConfigGridInformationKey.KeySeedStep, step);
                // 물주기 초기화 하기
                grindInfo.ErasePositionProperty(cell, ConfigGridInformationKey.KeyWetCount);
                
                tilemap.SetTile(cell, tile);
            }
            // 저장하기
            SceneGame.Instance.saveDataManager.SaveData();
        }
    }
}
