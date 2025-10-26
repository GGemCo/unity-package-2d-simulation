using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    public enum GrowthResultType { Item = 0 }
    public enum GrowthNeedType { None = 0, Watering = 1 }

    [System.Serializable]
    public class StruckGrowthNeed
    {
        [Tooltip("성장 조건에 필요한 타일 (예: 물뿌리기 후 젖은 타일). RuleTile 등도 지원.")]
        public TileBase tile;

        [Tooltip("해당 타일에서 요구되는 조건의 종류 (예: 물주기 필요 여부).")]
        public GrowthNeedType needType = GrowthNeedType.None;

        [Tooltip("필요 조건의 정량적 값 (예: 물주기 횟수 등). 음수는 자동 보정됩니다.")]
        public int needValue = 0;
    }

    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.Growth.MenuName, order = ConfigScriptableObjectSimulation.Growth.Ordering)]
    public class GrowthBase : ScriptableObject
    {
        [Header("씨앗 정보 (Seed Info)")]
        [Tooltip("이 성장 데이터와 연결된 씨앗 아이템의 Uid입니다. (item 테이블 참조)")]
        public int itemUid = 0;

        [Header("결과 정보 (Growth Result)")]
        [Tooltip("성장 완료 시 산출되는 결과의 종류입니다. (현재 Item만 지원)")]
        public GrowthResultType growthResultType = GrowthResultType.Item;

        [Tooltip("성장 완료 시 생성되는 결과 아이템의 Uid입니다. (item 테이블 참조)")]
        public int resultUid = 0;

        [Header("성장 조건 (Growth Conditions)")]
        [Tooltip("성장을 위해 필요한 조건 목록입니다. 각 조건은 타일 상태, 필요 타입, 필요 값으로 구성됩니다.")]
        public List<StruckGrowthNeed> struckGrowthConditions = new();

        // 에디터에서 값이 바뀌거나 리로드될 때마다 데이터 정리
        private void OnValidate()
        {
            // 리스트 null 방지
            if (struckGrowthConditions == null)
                struckGrowthConditions = new List<StruckGrowthNeed>();

            // 항목 정리 및 기본값 보정
            for (int i = struckGrowthConditions.Count - 1; i >= 0; i--)
            {
                var e = struckGrowthConditions[i];
                if (e == null)
                {
                    struckGrowthConditions.RemoveAt(i);
                    continue;
                }

                // 음수 방지
                if (e.needValue < 0) e.needValue = 0;

                // tile이 null인데 Need가 유효로 표기된 경우, Need를 None으로 보정
                if (e.tile == null && e.needType != GrowthNeedType.None)
                    e.needType = GrowthNeedType.None;
            }
        }
    }
}