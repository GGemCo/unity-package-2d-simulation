using GGemCo2DCore;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// 조작 키 변경하기 패널
    /// </summary>
    public class UIPanelOptionSimulation : UIPanelOptionBase
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("시뮬레이션 도구 미리보기 타일 항상 보임 On/off")]
        [SerializeField] private Toggle toolPreviewAlwaysShow;
        [Tooltip("시뮬레이션 도구 미리보기 타일, 이동 중에는 보이지 않음")]
        [SerializeField] private Toggle toolPreviewHideWhenMoving;

        private GGemCoOptionSettings _optionSettings;
#if UNITY_EDITOR
        private void OnValidate()
        {
            UIAssertionsChecker.Require(this, toolPreviewAlwaysShow, nameof(toolPreviewAlwaysShow));
            UIAssertionsChecker.Require(this, toolPreviewHideWhenMoving, nameof(toolPreviewHideWhenMoving));
        }
#endif
        
        protected override void Awake()
        {
            base.Awake();
            if (AddressableLoaderSettings.Instance)
            {
                _optionSettings = AddressableLoaderSettings.Instance.optionSettings;
            }
            toolPreviewAlwaysShow?.onValueChanged.AddListener(OnChangedToolPreviewAlwaysShow);
            toolPreviewHideWhenMoving?.onValueChanged.AddListener(OnChangedToolPreviewHideWhenMoving);
        }

        private void OnChangedToolPreviewHideWhenMoving(bool chk)
        {
            MarkDirty(true);
        }

        private void OnChangedToolPreviewAlwaysShow(bool chk)
        {
            MarkDirty(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            toolPreviewAlwaysShow?.onValueChanged.RemoveAllListeners();
            toolPreviewHideWhenMoving?.onValueChanged.RemoveAllListeners();
        }
        /// <summary>
        /// 초기 셋팅
        /// </summary>
        private void Initialize()
        {
        }
        /// <summary>
        /// UIWindowOption 셋팅하기
        /// </summary>
        /// <param name="puiWindowOption"></param>
        public override void SetWindowOption(UIWindowOption puiWindowOption)
        {
            base.SetWindowOption(puiWindowOption);
        }

        /// <summary>
        /// 옵션 설정 저장하기
        /// </summary>
        public override bool TryApply()
        {
            if (toolPreviewAlwaysShow)
                PlayerPrefsManager.SaveToolPreviewAlwaysShow(toolPreviewAlwaysShow.isOn);
            if (toolPreviewHideWhenMoving)
                PlayerPrefsManager.SaveToolPreviewHideWhenMoving(toolPreviewHideWhenMoving.isOn);

            var controlTool = SceneGame.Instance.player?.GetComponent<ControllerTool>();
            if (controlTool)
            {
                controlTool.ChangeAlwaysShow(toolPreviewAlwaysShow.isOn);
                controlTool.ChangeHideWhenMoving(toolPreviewHideWhenMoving.isOn);
            }
            return true;
        }

        /// <summary>
        /// 변경한 것이 있을때, 취소하기
        /// 취소 한 후 언어나 볼륨의 크기도 변경해야 하기 때문에 TryApply 호출
        /// </summary>
        public override void Revert()
        {
            RefreshFromModel();
            TryApply();
        }

        /// <summary>
        /// 디폴트 값으로 되돌리기. 저장하지 않음
        /// </summary>
        protected override void ResetToDefault()
        {
            if (!_optionSettings) return;
            if (toolPreviewAlwaysShow)
                toolPreviewAlwaysShow.isOn = _optionSettings.toolPreviewAlwaysShow;
            if (toolPreviewHideWhenMoving)
                toolPreviewHideWhenMoving.isOn = _optionSettings.toolPreviewHideWhenMoving;
        }
        
        /// <summary>
        /// 현재 저장되어있는 값으로 다시 셋팅하기
        /// </summary>
        protected override void RefreshFromModel()
        {
            if (toolPreviewAlwaysShow)
                toolPreviewAlwaysShow.isOn = PlayerPrefsManager.LoadToolPreviewAlwaysShow();
            if (toolPreviewHideWhenMoving)
                toolPreviewHideWhenMoving.isOn = PlayerPrefsManager.LoadToolPreviewHideWhenMoving();
        }
    }
}
