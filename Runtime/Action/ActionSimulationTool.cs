using System;
using GGemCo2DCore;
using GGemCo2DControl;            // IToolAction, InputManager 인터페이스용
using UnityEngine;
using UnityEngine.InputSystem;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// 시뮬레이션 툴 액션 (Simulation 측 구현)
    /// - Control의 ActionBase 의존을 제거하고, IToolAction 최소 계약만 구현합니다.
    /// - 캐릭터/컨트롤러/카메라는 Initialize(...)에서 주입받습니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ActionSimulationTool : MonoBehaviour, IToolAction
    {
        public static event Action<int> OnPreUseSeed;

        private CharacterBase _ch;
        private ControllerTool _controllerTool;
        private CharacterBaseController _ctrl;
        private Camera _cam;
        private ConfigCommon.FacingDirectionType _facingDirection; 

        public bool IsActive { get; private set; }

        // === IPlayerAction ===
        public void Initialize(MonoBehaviour owner, CharacterBase character, CharacterBaseController controller)
        {
            _ch   = character;
            _controllerTool = character.GetComponent<ControllerTool>();
            _ctrl = controller;
            _cam = SceneGame.Instance.mainCamera;
            _facingDirection = ConfigCommon.FacingDirectionType.TwoWay;
            if (AddressableLoaderSettings.Instance && AddressableLoaderSettings.Instance.settings)
            {
                _facingDirection = AddressableLoaderSettings.Instance.settings.facingDirectionType;
            }
        }

        public void Update()
        {
            // 필요 시 프리뷰/하이라이트 등 지속 처리 (없으면 no-op)
        }

        public void OnDestroy()
        {
            Cancel();
        }

        public void Cancel()
        {
            // 프리뷰, 임시 상태 정리
            IsActive = false;
        }

        // === IToolAction ===
        public void UseTool(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            if (_ch == null) return;

            if (_ch.IsStatusAttack()) return;
            if (_ch.IsStatusDead()) return;

            // 도구/씨앗 장착 여부
            if (!_ch.IsEquipSimulationTool() && !_ch.IsEquipSeed() && !_controllerTool.IsValidByHandHarvest()) return;
            
            // 동작이 가능한 타일인지
            if (_controllerTool && !_controllerTool.IsValid())
            {
                SceneGame.Instance.systemMessageManager.ShowMessageWarning("Simulation_Tool_CannotUseHere");
                return;
            }

            // 포인터 기준 바라보기 방향 계산
            var originWorld = _ch.transform.position;
            var targetWorld = GetPointerWorldPosition();
            var dir = (targetWorld - originWorld);
            if (dir.sqrMagnitude > 0.001f)
                _ch.SetFacing(dir);

            string animName = string.Empty;

            // 손으로 수확할 수 있는 아이템인지 제일 먼저 검사
            if (_controllerTool.IsValidByHandHarvest())
            {
                _ch.SetStatusSimulationTool();
                animName = ICharacterAnimationController.PickUpAnim;

                if (_facingDirection == ConfigCommon.FacingDirectionType.FourWay ||
                    _facingDirection == ConfigCommon.FacingDirectionType.EightWay)
                {
                    if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Up)
                        animName = ICharacterAnimationController.PickUpUpAnim;
                    else if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Down)
                        animName = ICharacterAnimationController.PickUpDownAnim;
                }
            }
            else if (_ch.IsEquipHoe())
            {
                _ch.SetStatusSimulationTool();
                animName = ICharacterAnimationController.HoeAnim;
                if (_facingDirection == ConfigCommon.FacingDirectionType.FourWay ||
                    _facingDirection == ConfigCommon.FacingDirectionType.EightWay)
                {
                    if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Up)
                        animName = ICharacterAnimationController.HoeUpAnim;
                    else if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Down)
                        animName = ICharacterAnimationController.HoeDownAnim;
                }
            }
            else if (_ch.IsEquipAxe())
            {
                _ch.SetStatusSimulationTool();
                animName = ICharacterAnimationController.AxeAnim;
                if (_facingDirection == ConfigCommon.FacingDirectionType.FourWay ||
                    _facingDirection == ConfigCommon.FacingDirectionType.EightWay)
                {
                    if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Up)
                        animName = ICharacterAnimationController.AxeUpAnim;
                    else if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Down)
                        animName = ICharacterAnimationController.AxeDownAnim;
                }
            }
            else if (_ch.IsEquipWatering())
            {
                _ch.SetStatusSimulationTool();
                animName = ICharacterAnimationController.WateringAnim;
                if (_facingDirection == ConfigCommon.FacingDirectionType.FourWay ||
                    _facingDirection == ConfigCommon.FacingDirectionType.EightWay)
                {
                    if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Up)
                        animName = ICharacterAnimationController.WateringUpAnim;
                    else if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Down)
                        animName = ICharacterAnimationController.WateringDownAnim;
                }
            }
            else if (_ch.IsEquipPickAxe())
            {
                _ch.SetStatusSimulationTool();
                animName = ICharacterAnimationController.PickAxeAnim;
                if (_facingDirection == ConfigCommon.FacingDirectionType.FourWay ||
                    _facingDirection == ConfigCommon.FacingDirectionType.EightWay)
                {
                    if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Up)
                        animName = ICharacterAnimationController.PickAxeUpAnim;
                    else if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Down)
                        animName = ICharacterAnimationController.PickAxeDownAnim;
                }
            }
            else if (_ch.IsEquipSickle())
            {
                _ch.SetStatusSimulationTool();
                animName = ICharacterAnimationController.SickleAnim;
                if (_facingDirection == ConfigCommon.FacingDirectionType.FourWay ||
                    _facingDirection == ConfigCommon.FacingDirectionType.EightWay)
                {
                    if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Up)
                        animName = ICharacterAnimationController.SickleUpAnim;
                    else if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Down)
                        animName = ICharacterAnimationController.SickleDownAnim;
                }
            }
            else if (_ch.IsEquipSeed())
            {
                // 퀵슬롯/저장 접근은 여전히 Core/SceneGame 경유 (Simulation 소속)
                var uiQuick = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowQuickSlotSimulation>(
                    UIWindowConstants.WindowUid.QuickSlotSimulation);

                if (!uiQuick)
                {
                    GcLogger.LogError($"{nameof(UIWindowQuickSlotSimulation)} 윈도우가 없습니다.");
                    return;
                }

                var icon = uiQuick.GetSelectedIcon();
                if (icon == null)
                {
                    GcLogger.LogError("장착된 씨앗이 없습니다.");
                    return;
                }

                if (!SceneGame.Instance.saveDataManager ||
                    SceneGame.Instance.saveDataManager.QuickSlotSimulation == null)
                {
                    GcLogger.LogError("saveDataManager.QuickSlotSimulation 이 없습니다.");
                    return;
                }

                // 사전 이벤트(예: 씨앗 UID 알림) 필요하면 발행
                OnPreUseSeed?.Invoke(icon.uid);

                var result =
                    SceneGame.Instance.saveDataManager.QuickSlotSimulation.MinusItem(icon.slotIndex, icon.uid, 1);
                uiQuick.SetIcons(result);
                if (result == null || !result.IsSuccess()) return;

                _ch.SetStatusSimulationTool();
                animName = ICharacterAnimationController.SeedAnim;

                if (_facingDirection == ConfigCommon.FacingDirectionType.FourWay ||
                    _facingDirection == ConfigCommon.FacingDirectionType.EightWay)
                {
                    if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Up)
                        animName = ICharacterAnimationController.SeedUpAnim;
                    else if (_ch.CurrentFacing == CharacterConstants.FacingDirection8.Down)
                        animName = ICharacterAnimationController.SeedDownAnim;
                }
            }

            if (!string.IsNullOrEmpty(animName))
            {
                _ch.CharacterAnimationController?.PlayCharacterAnimation(animName);
                IsActive = true;
            }
        }

        // ===== Helpers =====
        private Vector3 GetPointerWorldPosition()
        {
            return _cam.ScreenToWorldPoint(PositionHelper.GetPointerScreenPosition());
        }
    }
}
