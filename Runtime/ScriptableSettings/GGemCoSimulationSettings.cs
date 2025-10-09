using System;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// 플레이어 action 설정
    /// </summary>
    [CreateAssetMenu(fileName = ConfigScriptableObjectSimulation.SimulationSettings.FileName, menuName = ConfigScriptableObjectSimulation.SimulationSettings.MenuName, order = ConfigScriptableObjectSimulation.SimulationSettings.Ordering)]
    public class GGemCoSimulationSettings : ScriptableObject, ISettingsChangeNotifier
    {
        // 에디터/플레이모드에서만 쓰일 런타임 이벤트 (직렬화 방지)
        public event Action Changed;

#if UNITY_EDITOR
        // 인스펙터 값 변경 시 호출(에디터 전용)
        private void OnValidate()
        {
            // 값 클램핑/정규화도 여기서 처리하면 편함
            // if (jumpHeight < 0f) jumpHeight = 0f;

            Changed?.Invoke();
        }
#endif
        
        public void RaiseChanged() => Changed?.Invoke(); // 툴/코드에서 강제 호출 가능
        
        [Header("시간")]
        [Tooltip("현실 세계 1초를 몇 초로 할지 설정")]
        public float timeByWorldSecond;

    }
}