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
        // Tile 대신 보다 일반적인 TileBase 권장 (RuleTile 등 확장 포함)
        public TileBase tile;
        public GrowthNeedType needType = GrowthNeedType.None;
        public int needValue = 0;
    }

    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.Growth.MenuName, order = ConfigScriptableObjectSimulation.Growth.Ordering)]
    public class GrowthBase : ScriptableObject
    {
        [Header("Seed / Result")]
        public int itemUid = 0;
        public GrowthResultType growthResultType = GrowthResultType.Item;
        public int resultUid = 0;

        [Header("Growth Needs")]
        // 리스트는 즉시 초기화하여 null 인스펙터 바인딩 방지
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