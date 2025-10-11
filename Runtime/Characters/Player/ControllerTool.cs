using GGemCo2DCore;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GGemCo2DSimulation
{
    [DisallowMultipleComponent]
    public class ControllerTool : MonoBehaviour
    {
        [Header("Refs")]
        public Transform user;
        public Grid grid;
        public GridProbe probe;
        public AutoTilemapRegistry registry;
        public HitLocationVisualizer visualizer;

        [Header("Tool")]
        public ToolDefinition currentTool;
        public TargetingPolicy defaultTargeting; // 없을 때 사용
        public ToolRuntimeTiles tileset;         // 실제 쓰기 타일 모음

        [Header("UI Behavior")]
        public bool alwaysShow = true;
        public bool hideWhenMoving = true;

        private void Start()
        {
            if (!SceneGame.Instance) return;
            var sceneGame = SceneGame.Instance;
            user = sceneGame.player.transform;
            grid = GameObject.FindWithTag(ConfigTags.GetValue(ConfigTags.Keys.GridTileMap))?.GetComponent<Grid>();
            probe = sceneGame.player.GetComponent<GridProbe>();
            registry = GameObject.FindWithTag(ConfigTags.GetValue(ConfigTags.Keys.GridTileMap))?.GetComponent<AutoTilemapRegistry>();
            visualizer = sceneGame.player.GetComponent<HitLocationVisualizer>();
        }

        void Update()
        {
            if (!currentTool || !probe || !registry || !visualizer || !grid || !user)
                return;

            // 1) 포인터 위치 → 그리드 셀
            var cursor = probe.GetCursorCell(GetPointerScreenPosition());

            // 2) 사거리 체크
            var origin = grid.WorldToCell(user.position);
            if (!GridProbe.InRange(origin, cursor, currentTool.range, currentTool.metric))
            {
                visualizer.Apply(new(), new() { cursor });
                return;
            }

            // 3) 타게팅/검증
            var targeting = currentTool.targeting ? currentTool.targeting : defaultTargeting;
            var ctx = new ActionContext
            {
                user = user,
                grid = grid,
                originCell = origin,
                cursorCell = cursor,
                registry = registry,
                probe = probe,
                tool = currentTool,
                deltaTime = Time.deltaTime,
                tileset = tileset
            };
            var cells = targeting.GetCells(ctx);
            ctx.targetCells = cells;

            var result = currentTool.action.Validate(ctx);

            // 4) 미리보기
            if (alwaysShow && !(hideWhenMoving && IsMoving()))
                visualizer.Apply(result.ValidCells, result.InvalidCells);
            else
                visualizer.Clear();

            // 5) 실행(프라이머리 액션)
            if (IsPrimaryActionPressedThisFrame() && result.IsValid)
                currentTool.action.Execute(ctx);
        }

        // ----------------------------
        // Input Abstraction
        // ----------------------------

        private Vector3 GetPointerScreenPosition()
        {
#if ENABLE_INPUT_SYSTEM
            // 마우스 우선, 터치/패드 포인터 폴백
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                return Touchscreen.current.primaryTouch.position.ReadValue();
            // 게임패드엔 기본 포인터가 없으므로 마지막 마우스 위치 폴백
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Input.mousePosition;
#else
            return Input.mousePosition;
#endif
        }

        private bool IsPrimaryActionPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            // 마우스 좌클릭 / 터치 탭 / 게임패드 South 버튼(A/×)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) return true;
            return false;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private bool IsMoving()
        {
#if ENABLE_INPUT_SYSTEM
            float x = 0f, y = 0f;

            // 키보드 WASD/화살표
            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                x += kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f;
                x -= kb.aKey.isPressed || kb.leftArrowKey.isPressed  ? 1f : 0f;
                y += kb.wKey.isPressed || kb.upArrowKey.isPressed    ? 1f : 0f;
                y -= kb.sKey.isPressed || kb.downArrowKey.isPressed  ? 1f : 0f;
            }

            // 게임패드 좌스틱
            if (Gamepad.current != null)
            {
                var ls = Gamepad.current.leftStick.ReadValue();
                x = Mathf.Abs(ls.x) > Mathf.Abs(x) ? ls.x : x;
                y = Mathf.Abs(ls.y) > Mathf.Abs(y) ? ls.y : y;
            }

            // 터치 스와이프 기반 이동을 쓰는 프로젝트라면 여기서 추가 처리
            return Mathf.Abs(x) + Mathf.Abs(y) > 0.01f;
#else
            var h = Input.GetAxisRaw("Horizontal");
            var v = Input.GetAxisRaw("Vertical");
            return Mathf.Abs(h) + Mathf.Abs(v) > 0.01f;
#endif
        }
    }
}
