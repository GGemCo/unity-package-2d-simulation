using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// 플레이어 도구, 씨앗 사용처리
    /// </summary>
    [DisallowMultipleComponent]
    public class ControllerTool : MonoBehaviour
    {
        [Header("Refs")]
        private Transform _user;
        private Grid _grid;
        private GridProbe _probe;
        private AutoTilemapRegistry _registry;
        private HitLocationVisualizer _visualizer;

        [Header("Tool")]
        // 현재 사용중인 도구, 씨앗
        private ToolDefinition _currentTool;
        // 도구, 씨앗 ItemUid
        private int _currentItemUid;
        
        // 실제 쓰기 타일 모음
        private bool _alwaysShow = true;
        private bool _hideWhenMoving = true;

        private Player _player;
        private bool _isValid;
        private ToolActionContext _currentToolActionContext;
        private SimulationDirtyTracker _simulationDirtyTracker;
        private TileBase _defaultTileHoe;
        private TileBase _defaultTileWet;

        private void Awake()
        {
            _alwaysShow = PlayerPrefsManager.LoadToolPreviewAlwaysShow();
            _hideWhenMoving = PlayerPrefsManager.LoadToolPreviewHideWhenMoving();
            _currentTool = null;
            if (AddressableLoaderSettingsSimulation.Instance)
            {
                if (!AddressableLoaderSettingsSimulation.Instance.simulationSettings)
                {
                    GcLogger.LogError($"GGemCoSimulationSettings 스크립터블 오브젝트가 없습니다.");
                    return;
                }
                _defaultTileHoe = AddressableLoaderSettingsSimulation.Instance.simulationSettings.hoedTile;
                if (!_defaultTileHoe)
                {
                    GcLogger.LogError(
                        $"GGemCoSimulationSettings 스크립터블 오브젝트에 {nameof(AddressableLoaderSettingsSimulation.Instance.simulationSettings.hoedTile)}을 등록해주세요.");
                }
                _defaultTileWet = AddressableLoaderSettingsSimulation.Instance.simulationSettings.wetTile;
                if (!_defaultTileWet)
                {
                    GcLogger.LogError(
                        $"GGemCoSimulationSettings 스크립터블 오브젝트에 {nameof(AddressableLoaderSettingsSimulation.Instance.simulationSettings.wetTile)}을 등록해주세요.");
                }
            }
        }

        private void Start()
        {
            if (!SceneGame.Instance) return;
            _isValid = false;
            _currentToolActionContext = null;
            _user = transform;
            _player = GetComponent<Player>();
            if (!_player)
            {
                GcLogger.LogError($"{nameof(Player)} 스크립트가 없습니다.");
                return;
            }
            _probe = GetComponent<GridProbe>();
            if (!_probe)
            {
                GcLogger.LogError($"{nameof(GridProbe)} 스크립트가 없습니다.");
                return;
            }
            _visualizer = GetComponent<HitLocationVisualizer>();
            if (!_visualizer)
            {
                GcLogger.LogError($"{nameof(HitLocationVisualizer)} 스크립트가 없습니다.");
                return;
            }
            
            _grid = GameObject.FindWithTag(ConfigTags.GetValue(ConfigTags.Keys.GridTileMap))?.GetComponent<Grid>();
            _registry = GameObject.FindWithTag(ConfigTags.GetValue(ConfigTags.Keys.GridTileMap))?.GetComponent<AutoTilemapRegistry>();
            _simulationDirtyTracker = SimulationPackageManager.Instance.simulationDirtyTracker;
        }

        private void Update()
        {
            if (_currentTool == null || !_currentTool.targeting || !_probe || !_registry || !_visualizer || !_grid || !_user)
            {
                _currentToolActionContext = null;
                _isValid = false;
                return;
            }

            // 유저가 툴 사용을 시작하면 중지하기
            if (_player.IsStatusSimulationTool()) return;

            // 1) 포인터 위치 → 그리드 셀
            var cursor = _probe.GetCursorCell(PositionHelper.GetPointerScreenPosition());

            // 2) 사거리 체크
            var origin = _grid.WorldToCell(_user.position);
            if (!GridProbe.InRange(origin, cursor, _currentTool.range, _currentTool.metric))
            {
                _currentToolActionContext = null;
                _isValid = false;

                if (_alwaysShow)
                {
                    if (_hideWhenMoving && _player.IsStatusRun())
                    {
                        _visualizer.Clear();
                    }
                    else
                    {
                        _visualizer.Apply(new(), new() { cursor });
                    }
                }
                else
                {
                    _visualizer.Clear();
                }
                return;
            }
            
            // 3) 타게팅/검증
            var targeting = _currentTool.targeting;
            var ctx = new ToolActionContext
            {
                user = _user,
                grid = _grid,
                originCell = origin,
                cursorCell = cursor,
                registry = _registry,
                probe = _probe,
                tool = _currentTool,
                deltaTime = Time.deltaTime,
                defaultTileHoe = _defaultTileHoe,
                defaultTileWet = _defaultTileWet,
                dirtyTracker = _simulationDirtyTracker,
                itemUid = _currentItemUid
            };
            var cells = targeting.GetCells(ctx);
            ctx.targetCells = cells;

            var result = _currentTool.action.Validate(ctx);
            _isValid = result.IsValid;
            _currentToolActionContext = result.IsValid ? ctx : null;
            
            // 4) 미리보기
            if (_alwaysShow)
            {
                if (_hideWhenMoving && _player.IsStatusRun())
                {
                    _visualizer.Clear();
                }
                else
                {
                    _visualizer.Apply(result.ValidCells, result.InvalidCells);
                }
            }
            else
            {
                _visualizer.Clear();
            }
        }
        /// <summary>
        /// 도구 사용하기
        /// </summary>
        public void UseTool()
        {
            if (!_isValid || _currentToolActionContext == null) return;
            _currentTool.action.Execute(_currentToolActionContext);
        }

        public bool IsValid() => _isValid;
        /// <summary>
        /// 씨앗 사용하기
        /// </summary>
        public void UseSeed()
        {
            if (!_isValid || _currentToolActionContext == null) return;
            _currentTool.action.Execute(_currentToolActionContext);
        }

        public void ChangeAlwaysShow(bool value)
        {
            _alwaysShow = value;
        }
        public void ChangeHideWhenMoving(bool value)
        {
            _hideWhenMoving = value;
        }

        public void ChangeTool(ToolDefinition toolDefinition, int itemUid)
        {
            _currentTool = toolDefinition;
            _currentItemUid = itemUid;
        }
    }
}
