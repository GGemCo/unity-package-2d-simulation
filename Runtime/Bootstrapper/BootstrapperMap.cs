using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// Core의 캐릭터 생성 이벤트를 구독하여 ControlBase 를 자동 부착
    /// </summary>
    public class BootstrapperMap : MonoBehaviour
    {
        private void OnEnable()
        {
            MapManager.OnLoadCompleteMap   += OnMapLoadComplete;
            MapManager.OnLoadTilemapCompleteMap   += OnMapLoadTilemapComplete;
        }

        private void OnDisable()
        {
            MapManager.OnLoadCompleteMap   -= OnMapLoadComplete;
            MapManager.OnLoadTilemapCompleteMap   -= OnMapLoadTilemapComplete;
        }

        private void OnMapLoadComplete(MapTileCommon mapTileCommon, GameObject gridTileMap)
        {
            if (SceneGame.Instance)
            {
                if (SceneGame.Instance.gameTimeManager)
                {
                    SceneGame.Instance.gameTimeManager.SetPause(false);
                }
            }
        }

        private void OnMapLoadTilemapComplete(MapTileCommon mapTileCommon, GameObject gridTileMap)
        {
            // GcLogger.Log($"OnMapLoadTilemapComplete");
            
            if (!SceneGame.Instance) return;
            var grid = SceneGame.Instance.mapManager.GetGrid();
            if (grid == null) return;
            var grindInfo = grid.GetComponent<GridInformation>();
            if (grindInfo == null)
            {
                grindInfo = grid.gameObject.AddComponent<GridInformation>();
            }
            var autoTilemapRegistry = grid.GetComponent<AutoTilemapRegistry>();
            if (autoTilemapRegistry == null)
            {
                autoTilemapRegistry = grid.gameObject.AddComponent<AutoTilemapRegistry>();
            }
            
            // 세이브 데이터 로드 
            SimulationPackageManager.Instance.simulationSaveContributor.UpdateToGridInfo(grindInfo);

            var cells = grindInfo.GetAllPositions(ConfigGridInformationKey.KeyHoed);
            var tilemap = autoTilemapRegistry.GetTop(ConfigCommonSimulation.TileRole.GroundHoed);
            if (tilemap != null)
            {
                var hoedTile = AddressableLoaderSettingsSimulation.Instance.simulationSettings.hoedTile;
                if (hoedTile == null)
                {
                    GcLogger.LogError($"SimulationSettings 스크립터블 오브젝트에 {nameof(hoedTile)}이 연결되어 있지 않습니다.");
                    return;
                }
                foreach (var cell in cells)
                {
                    tilemap.SetTile(cell, hoedTile);
                }
            }
            cells = grindInfo.GetAllPositions(ConfigGridInformationKey.KeyWet);
            tilemap = autoTilemapRegistry.GetTop(ConfigCommonSimulation.TileRole.GroundWet);
            if (tilemap != null)
            {
                var wetTile = AddressableLoaderSettingsSimulation.Instance.simulationSettings.wetTile;
                if (wetTile == null)
                {
                    GcLogger.LogError($"SimulationSettings 스크립터블 오브젝트에 {nameof(wetTile)}이 연결되어 있지 않습니다.");
                    return;
                }
                foreach (var cell in cells)
                {
                    tilemap.SetTile(cell, wetTile);
                }
            }

            cells = grindInfo.GetAllPositions(ConfigGridInformationKey.KeySeedItemUid);
            var tableItem = TableLoaderManager.Instance.TableItem;
            tilemap = autoTilemapRegistry.GetTop(ConfigCommonSimulation.TileRole.GroundGrowth);
            if (tilemap != null)
            {
                foreach (var cell in cells)
                {
                    int itemUid = grindInfo.GetPositionProperty(cell, ConfigGridInformationKey.KeySeedItemUid, -1);
                    int seedStep = grindInfo.GetPositionProperty(cell, ConfigGridInformationKey.KeySeedStep, -1);
                    if (itemUid <= 0)
                    {
                        GcLogger.LogError($"Cell 정보에 저장된 Seed 아이템 Uid가 {itemUid} 입니다.");
                        continue;
                    }

                    if (seedStep < 0)
                    {
                        GcLogger.LogError($"Cell 정보에 저장된 Seed Step이 {seedStep} 입니다.");
                        continue;
                    }

                    var item = tableItem.GetDataByUid(itemUid);
                    if (item == null)
                    {
                        continue;
                    }

                    var key = $"{ConfigAddressableKey.SimulationGrowth}_{item.Uid}";
                    GrowthBase growthBase = AddressableLoaderGrowth.Instance.GetGrowthBaseByName(key);
                    if (growthBase == null)
                    {
                        GcLogger.LogError(
                            $"Addressables에 {key} 키로 등록된 GrowthBase 스크립터블 오브젝트가 없습니다. itemUid: {item.Uid}");
                        continue;
                    }

                    if (seedStep >= growthBase.struckGrowthConditions.Count)
                    {
                        GcLogger.LogError($"최대 Step입니다. itemUid: {item.Uid}, Seed Step:{seedStep}");
                        continue;
                    }

                    TileBase tile = growthBase.struckGrowthConditions[seedStep].tile;
                    tilemap.SetTile(cell, tile);
                }
            }
        }
    }
}