using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [DisallowMultipleComponent]
    public class WetDecaySystem : MonoBehaviour
    {
        private struct Entry
        {
            public Tilemap map;
            public Vector3Int cell;
            public int until;         // 만료 시각(초, int)
            public TileRole prevRole; // GroundHoed or GroundBase
        }

        private void Awake() => _instance = this;
        public static WetDecaySystem TryGetInstance() => _instance;
        
        [Header("Refs")]
        private Grid _grid;
        private AutoTilemapRegistry _registry;

        [Header("Tick")]
        [Min(0.01f)] public float checkInterval = 0.25f;

        private readonly List<Entry> _entries = new();
        private float _acc;
        private static WetDecaySystem _instance;

        private GameTimeManager _gameTimeManager;
        private SimulationDirtyTracker _simulationDirtyTracker;
        private TileBase _defaultTileEmpty;

        private void Start()
        {
            if (!SceneGame.Instance)
            {
                GcLogger.LogError($"SceneGame.Instance이 없습니다.");
                return;
            }
            _gameTimeManager = SceneGame.Instance.gameTimeManager;
            if (!_gameTimeManager)
            {
                GcLogger.LogError($"{nameof(SceneGame.Instance.gameTimeManager)}이 없습니다.");
            }
            if (!SceneGame.Instance.mapManager)
            {
                GcLogger.LogError($"{nameof(SceneGame.Instance.mapManager)}가 없습니다.");
                return;
            }
            _grid = SceneGame.Instance.mapManager.GetGrid();
            if (!_grid)
            {
                GcLogger.LogError($"{nameof(_grid)}가 없습니다.");
                return;
            }
            if (AddressableLoaderSettingsSimulation.Instance)
            {
                _defaultTileEmpty = AddressableLoaderSettingsSimulation.Instance.simulationSettings.emptyTile;
            }

            _simulationDirtyTracker = SimulationPackageManager.Instance.simulationDirtyTracker;
        }

        public void Register(Tilemap map, Vector3Int cell, int until, TileRole prevRole)
        {
            if (!_registry)
            {
                _registry = _grid.GetComponent<AutoTilemapRegistry>();
                if (!_registry)
                {
                    GcLogger.LogError($"Grid 오브젝트에 {nameof(AutoTilemapRegistry)}가 없습니다.");
                    return;
                }
            }
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].map == map && _entries[i].cell == cell)
                {
                    _entries[i] = new Entry { map = map, cell = cell, until = until, prevRole = prevRole };
                    return;
                }
            }
            _entries.Add(new Entry { map = map, cell = cell, until = until, prevRole = prevRole });
        }

        private void Update()
        {
            _acc += Time.unscaledDeltaTime;
            if (_acc < checkInterval) return;
            _acc = 0f;

            if (_grid == null || _registry == null || _defaultTileEmpty == null) return;

            var info = _grid.GetComponent<GridInformation>();
            if (!info) return;

            int now = NowSecondsInt();

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var e = _entries[i];
                if (now < e.until) continue;

                // 복귀 타일 결정
                var writeMap = _registry.ResolveWriteTarget(TileRole.GroundWet, e.cell) ?? e.map;
                writeMap.SetTile(e.cell, _defaultTileEmpty);

                // 메타 제거
                info.ErasePositionProperty(e.cell, ConfigGridInformationKey.KeyWet);
                info.ErasePositionProperty(e.cell, ConfigGridInformationKey.KeyWetUntil);
                info.ErasePositionProperty(e.cell, ConfigGridInformationKey.KeyWetPrevRole);
                
                _simulationDirtyTracker.MarkErased(info, e.cell, ConfigGridInformationKey.KeyWet);
                _simulationDirtyTracker.MarkErased(info, e.cell, ConfigGridInformationKey.KeyWetUntil);
                _simulationDirtyTracker.MarkErased(info, e.cell, ConfigGridInformationKey.KeyWetPrevRole);

                _entries.RemoveAt(i);
            }
        }

        // (선택) 저장값 복구는 프로젝트의 세이브 시스템에서 담당하는 것을 권장
        void OnEnable()
        {
            // 필요 시, 세이브 데이터에서 Wet 셀 목록을 불러와 Register 호출.
            // GridInformation은 개별 키 조회만 제공하므로 전수 스캔은 비추천.
        }

        private int NowSecondsInt()
        {
            if (!_gameTimeManager) return 0;
            return Mathf.FloorToInt((float)_gameTimeManager.NowSeconds());
        }
    }
}
