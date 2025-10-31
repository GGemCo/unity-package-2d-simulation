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
        private Grid _grid;
        private GridInformation _gridInformation;
        private AutoTilemapRegistry _autoTilemapRegistry;
        private SimulationSaveContributor _simulationSaveContributor;
        private SimulationDirtyTracker _simulationDirtyTracker;
        private TableItem _tableItem;
        private GameTimeManager _gameTimeManager;

        private void Awake()
        {
            if (!TableLoaderManager.Instance) return;
            _tableItem = TableLoaderManager.Instance.TableItem;
        }

        private void OnEnable()
        {
            MapManager.OnLoadCompleteMap   += OnMapLoadComplete;
            MapManager.OnLoadTilemapCompleteMap   += OnMapLoadTilemapComplete;
        }

        private void Start()
        {
            if (SceneGame.Instance && SceneGame.Instance.gameTimeManager)
                _gameTimeManager = SceneGame.Instance.gameTimeManager;
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
            
            _grid = gridTileMap.GetComponent<Grid>();
            if (_grid == null)
            {
                GcLogger.LogError($"{nameof(Grid)} 컴포넌트가 없습니다.");
                return;
            }

            if (!_grid.gameObject.GetComponent<AutoTilemapRegistry>())
                _autoTilemapRegistry = _grid.gameObject.AddComponent<AutoTilemapRegistry>();
            
            if (_autoTilemapRegistry == null)
            {
                GcLogger.LogError($"{nameof(AutoTilemapRegistry)} 컴포넌트가 없습니다.");
                return;
            }

            _gridInformation = _grid.gameObject.GetComponent<GridInformation>();
            if (_gridInformation == null)
            {
                _gridInformation = _grid.gameObject.AddComponent<GridInformation>();
                if (_gridInformation == null)
                {
                    GcLogger.LogError($"{nameof(GridInformation)} 컴포넌트가 없습니다.");
                    return;
                }
                
                // 처음 게임 시작시에만 세이브 데이터 로드 
                if (_simulationSaveContributor == null)
                    _simulationSaveContributor = SimulationPackageManager.Instance.simulationSaveContributor;
                SaveRegistry.Register(_simulationSaveContributor);
            }
            else
            {
                // 기존 정보를 지우고
                _gridInformation.Reset();
                // 현재 맵의 Grid Information을 불러오기
                _simulationSaveContributor.Restore();
            }
            _simulationSaveContributor.UpdateToGridInfo(_gridInformation);
            
            if (_simulationDirtyTracker == null)
                _simulationDirtyTracker = SimulationPackageManager.Instance.simulationDirtyTracker;

            var cells = _gridInformation.GetAllPositions(ConfigGridInformationKey.KeyHoed);
            var tilemap = _autoTilemapRegistry.GetTop(ConfigCommonSimulation.TileRole.GroundHoed);
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
            cells = _gridInformation.GetAllPositions(ConfigGridInformationKey.KeyWet);
            tilemap = _autoTilemapRegistry.GetTop(ConfigCommonSimulation.TileRole.GroundWet);
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

            cells = _gridInformation.GetAllPositions(ConfigGridInformationKey.KeySeedItemUid);
            tilemap = _autoTilemapRegistry.GetTop(ConfigCommonSimulation.TileRole.GroundGrowth);
            if (tilemap != null)
            {
                foreach (var cell in cells)
                {
                    int itemUid   = _gridInformation.GetIntSafe(cell, ConfigGridInformationKey.KeySeedItemUid);
                    int seedStep  = _gridInformation.GetIntSafe(cell, ConfigGridInformationKey.KeySeedStep);
                    int countWater= _gridInformation.GetIntSafe(cell, ConfigGridInformationKey.KeyWetCount);
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

                    var item = _tableItem.GetDataByUid(itemUid);
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
                    
                    // GcLogger.Log($"seed current step: {seedStep}");
                    var next = seedStep + 1;
                    if (GrowthEvaluator.TryFindNextGrowableStep(growthBase, _gridInformation, cell,
                            next, out var nextStep, out var reason))
                    {
                        GrowthEvaluator.ApplyStep(growthBase, tilemap, _gridInformation, cell, nextStep, _simulationDirtyTracker);
                        // 물주기 초기화 하기
                        _gridInformation.ErasePositionProperty(cell, ConfigGridInformationKey.KeyWetCount);
                        _simulationDirtyTracker.MarkErased(_gridInformation, cell, ConfigGridInformationKey.KeyWetCount);
                        // 심는 날짜 업데이트
                        _gridInformation.SetPositionProperty(cell, ConfigGridInformationKey.KeySeedStartDate, _gameTimeManager.GetNowDateString() );
                        _simulationDirtyTracker.MarkDirty(_gridInformation, cell);
                        // GcLogger.Log($"seed step up ok: {next}");
                    }
                    else
                    {
                        TileBase tile = growthBase.struckGrowthConditions[seedStep].resultTile;
                        tilemap.SetTile(cell, tile);
                        // GcLogger.Log($"reason: {reason}");
                    }
                }
            }
        }
    }
}