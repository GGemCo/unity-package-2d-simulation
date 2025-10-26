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
        private GridInformation _gridInformation;
        private GridProbe _probe;
        private AutoTilemapRegistry _registry;
        private HitLocationVisualizer _visualizer;

        [Header("Tool")]
        // 현재 사용중인 도구, 씨앗
        private ToolDefinition _currentTool;
        // 도구, 씨앗 ItemUid
        private int _currentItemUid;
        // 손으로 수확하기
        private ToolDefinition _toolHandHarvest;
        // 이번 프레임에 채택된 후보(손 수확 또는 장착 도구)
        private ToolDefinition _activeToolForThisFrame;
        
        // 실제 쓰기 타일 모음
        private bool _alwaysShow = true;
        private bool _hideWhenMoving = true;

        private Player _player;
        private bool _isValid;
        private ToolActionContext _currentToolActionContext;
        private SimulationDirtyTracker _simulationDirtyTracker;
        private TileBase _defaultTileHoe;
        private TileBase _defaultTileWet;
        private TileBase _defaultTileEmpty;

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
                _defaultTileEmpty = AddressableLoaderSettingsSimulation.Instance.simulationSettings.emptyTile;
                if (!_defaultTileEmpty)
                {
                    GcLogger.LogError(
                        $"GGemCoSimulationSettings 스크립터블 오브젝트에 {nameof(AddressableLoaderSettingsSimulation.Instance.simulationSettings.emptyTile)}을 등록해주세요.");
                }
                _toolHandHarvest = AddressableLoaderSettingsSimulation.Instance.simulationSettings.toolHandHarvest;
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

            _grid = SceneGame.Instance.mapManager.GetGrid();
            if (!_grid)
            {
                GcLogger.LogError($"{nameof(Grid)}가 없습니다.");
                return;
            }
            _registry = _grid.gameObject.GetComponent<AutoTilemapRegistry>();
            if (!_registry)
            {
                GcLogger.LogError($"Grid 오브젝트에 {nameof(AutoTilemapRegistry)}가 없습니다.");
                return;
            }
            _gridInformation = _grid.gameObject.GetComponent<GridInformation>();
            if (!_gridInformation)
            {
                GcLogger.LogError($"Grid 오브젝트에 {nameof(GridInformation)}가 없습니다.");
                return;
            }
            _simulationDirtyTracker = SimulationPackageManager.Instance.simulationDirtyTracker;
        }

        private void Update()
        {
            // 공통 전제 검사
            if (!_probe || !_registry || !_visualizer || !_grid || !_user)
            {
                _currentToolActionContext = null;
                _isValid = false;
                return;
            }

            // UI나 애니메이션 등으로 도구 입력이 막힌 경우
            if (_player.IsStatusSimulationTool())
            {
                return;
            }

            var cursor = _probe.GetCursorCell(PositionHelper.GetPointerScreenPosition());
            var origin = _grid.WorldToCell(_user.position);

            _activeToolForThisFrame = null;
            _currentToolActionContext = null;
            _isValid = false;

            ValidationResult vr;
            ToolActionContext ctx;

            // ① 손 수확(선점) 시도
            if (_toolHandHarvest != null
                && TryValidateTool(_toolHandHarvest, origin, cursor, out vr, out ctx)
                && vr.IsValid)
            {
                AcceptResult(_toolHandHarvest, vr, ctx);
                return; // 손 수확이 잡히면 장착 도구로 내려가지 않음
            }

            // ② 장착 도구 시도(손 수확 실패/부적합 시)
            if (_currentTool != null
                && _currentTool.targeting != null
                && TryValidateTool(_currentTool, origin, cursor, out vr, out ctx))
            {
                AcceptResult(_currentTool, vr, ctx);
                return;
            }

            // ③ 둘 다 불가 → 미리보기/상태 정리
            _visualizerBehavior_Invalid(cursor);
        }
        private bool TryValidateTool(
            ToolDefinition tool, Vector3Int origin, Vector3Int cursor,
            out ValidationResult result, out ToolActionContext ctx)
        {
            result = null; ctx = null;

            if (!GridProbe.InRange(origin, cursor, tool.range, tool.metric))
                return false;

            var targeting = tool.targeting;
            if (targeting == null) return false;

            ctx = new ToolActionContext
            {
                user = _user, grid = _grid,
                originCell = origin, cursorCell = cursor,
                registry = _registry, probe = _probe,
                tool = tool, deltaTime = Time.deltaTime,
                defaultTileHoe = _defaultTileHoe,
                defaultTileWet = _defaultTileWet,
                defaultTileEmpty = _defaultTileEmpty,
                dirtyTracker = _simulationDirtyTracker,
                itemUid = _currentItemUid,
                gridInformation = _gridInformation
            };

            var cells = targeting.GetCells(ctx);
            ctx.targetCells = cells;

            result = tool.action.Validate(ctx);
            return true; // 검증 자체는 수행했고, 유효성은 result.IsValid로 판단
        }
        private void AcceptResult(ToolDefinition picked, ValidationResult vr, ToolActionContext ctx)
        {
            _activeToolForThisFrame = picked;
            _isValid = vr.IsValid;
            _currentToolActionContext = vr.IsValid ? ctx : null;

            if (_alwaysShow)
            {
                if (_hideWhenMoving && _player.IsStatusRun()) _visualizer.Clear();
                else _visualizer.Apply(vr.ValidCells, vr.InvalidCells);
            }
            else _visualizer.Clear();
        }
        private void _visualizerBehavior_Invalid(Vector3Int cursor)
        {
            _isValid = false; _currentToolActionContext = null;
            if (_alwaysShow)
            {
                if (_hideWhenMoving && _player.IsStatusRun()) _visualizer.Clear();
                else _visualizer.Apply(new(), new() { cursor });
            }
            else _visualizer.Clear();
        }
        /// <summary>
        /// 도구 사용하기
        /// </summary>
        public void UseTool()
        {
            if (!_isValid || _currentToolActionContext == null || _activeToolForThisFrame == null) return;
            _activeToolForThisFrame.action.Execute(_currentToolActionContext); // 채택된 툴 실행
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

        public bool IsValidByHandHarvest()
        {
            return _activeToolForThisFrame == _toolHandHarvest && _isValid;
        }
    }
}
