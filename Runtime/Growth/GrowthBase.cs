using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    public enum GrowthResultType { Item = 0 }
    public enum GrowthNeedType { None = 0, Watering = 1, Day = 2 }

    [System.Serializable]
    public class GrowthNeedEntry
    {
        [Tooltip("필요한 조건의 타입. Watering:물주기, Day: 일수")]
        public GrowthNeedType type = GrowthNeedType.None;

        [Tooltip("필요한 횟수나 수치. 음수는 자동 보정됩니다.")]
        public int value = 0;
    }

    [System.Serializable]
    public class StruckGrowthNeed
    {
        [Tooltip("모든 조건을 만족하면 셀에 적용할 타일 (결과 타일).")]
        public TileBase resultTile;

        [Tooltip("이 타일에 필요한 조건의 리스트")]
        public List<GrowthNeedEntry> needs = new();

        public void OnValidate()
        {
            if (needs == null)
                needs = new List<GrowthNeedEntry>();

            for (int i = needs.Count - 1; i >= 0; i--)
            {
                var n = needs[i];
                if (n == null)
                {
                    needs.RemoveAt(i);
                    continue;
                }
                if (n.value < 0) n.value = 0;
            }
        }
    }

    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.Growth.MenuName, order = ConfigScriptableObjectSimulation.Growth.Ordering)]
    public class GrowthBase : ScriptableObject
    {
        [Header("씨앗 정보 (Seed Info)")]
        public int itemUid = 0;

        [Header("결과 정보 (Growth Result)")]
        public GrowthResultType growthResultType = GrowthResultType.Item;
        public int resultUid = 0;

        [Header("성장 조건 (Growth Conditions)")]
        public List<StruckGrowthNeed> struckGrowthConditions = new();

        private void OnValidate()
        {
            if (struckGrowthConditions == null)
                struckGrowthConditions = new List<StruckGrowthNeed>();

            foreach (var cond in struckGrowthConditions)
            {
                if (cond == null) continue;
                cond.OnValidate();

                // tile이 null이면 needs 비우기
                if (cond.resultTile == null && cond.needs.Count > 0)
                    cond.needs.Clear();
            }
        }
    }
}
