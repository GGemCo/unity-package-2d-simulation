using System.Text.RegularExpressions;
using UnityEngine;

namespace GGemCo2DSimulation
{
    public enum ColliderRequirement
    {
        Ignore,         // 무시 (있든 없든 상관없음)
        Require,        // 반드시 있어야 함
        Forbid          // 있으면 안 됨
    }
    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.TilemapRoleRule.MenuName, order = ConfigScriptableObjectSimulation.TilemapRoleRule.Ordering)]
    public class TilemapRoleRule : ScriptableObject
    {
        [Header("Target Role")]
        [Tooltip("이 규칙이 적용될 Tilemap의 역할 (예: Ground, Object, House 등)")]
        public ConfigCommonSimulation.TileRole role;

        [Header("Name Filter (Regex)")]
        [Tooltip("Tilemap 오브젝트 이름에 매칭될 정규식. 예: (?i)ground|floor")]
        public string nameRegex;

        [Header("Collider Requirement")]
        [Tooltip("TilemapCollider2D 존재 조건\n- Ignore: 상관 없음\n- Require: 반드시 있어야 함\n- Forbid: 있으면 안 됨")]
        public ColliderRequirement colliderRequirement = ColliderRequirement.Ignore;

        [Header("Scoring Settings")]
        [Tooltip("규칙이 일치할 때 가산할 점수 (1~100)")]
        [Range(1, 100)] public int weight = 10;

        // [Tooltip] TilemapRenderer의 Sorting Layer 이름 (명시한 경우에만 검사)
        [HideInInspector] public string sortingLayerName;

        // [Tooltip] Sorting Order 최소값 (보조 조건)
        [HideInInspector] public int sortingOrderMin = 0;

        // [Tooltip] Sorting Order 최대값 (보조 조건)
        [HideInInspector] public int sortingOrderMax = 0;

        // [Tooltip] Unity Tag (명시한 경우에만 검사)
        [HideInInspector] public string unityTag;

        // [Tooltip] Unity Layer (-1 = 무시)
        [HideInInspector] public int unityLayer = -1;

        // [Tooltip] TilemapRenderer가 반드시 존재해야 하는가? (없으면 무시됨)
        [HideInInspector] public bool requireRenderer = true;

        // TilemapRoleRule.cs 내부
        public int Score(GameObject go)
        {
            int score = 0;
            bool matchedKey = false;   // 핵심 키 필드(Name/Tag/Layer/SortingLayer) 중 1개 이상 매치했는가

            // 1) Name Regex
            if (!string.IsNullOrEmpty(nameRegex))
            {
                if (Regex.IsMatch(go.name, nameRegex))
                {
                    score += weight;
                    matchedKey = true;
                }
                else
                {
                    // 이름을 명시했다면 불일치 시 바로 탈락시키고 싶다면 주석 해제:
                    return 0;
                }
            }

            // 2) Sorting Layer Name (명시된 경우에만 판단)
            var r = go.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
            if (!string.IsNullOrEmpty(sortingLayerName))
            {
                if (r && r.sortingLayerName == sortingLayerName)
                {
                    // 사용하지 않는다.
                    // score += weight;
                    // matchedKey = true;
                }
                else
                {
                    // 정렬 레이어를 강제하고 싶으면 탈락:
                    // return 0;
                }
            }

            // 3) Tag / Layer
            if (!string.IsNullOrEmpty(unityTag))
            {
                if (go.CompareTag(unityTag))
                {
                    // 사용하지 않는다.
                    // score += weight; matchedKey = true;
                }
                else
                {
                    // 필요 시 강제 탈락:
                    // return 0;
                }
            }
            if (unityLayer >= 0)
            {
                // 사용하지 않는다.
                if (go.layer == unityLayer)
                {
                    // score += weight; matchedKey = true;
                }
                else
                {
                    // 필요 시 강제 탈락:
                    // return 0;
                }
            }

            // 4) Sorting Order 범위는 “보조 조건”으로만 사용
            if (r)
            {
                // 사용하지 않는다.
                // if (r.sortingOrder >= sortingOrderMin && r.sortingOrder <= sortingOrderMax)
                //     score += weight; // 선택적 가산점
            }
            else if (requireRenderer) return 0;

            // 5) Collider 제약
            var col = go.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
            switch (colliderRequirement)
            {
                case ColliderRequirement.Require:
                    if (!col) return 0;
                    score += weight;
                    break;

                case ColliderRequirement.Forbid:
                    if (col) return 0;
                    score += weight;
                    break;
            }

            // 6) 핵심 키가 하나도 안 맞으면 “노이즈 매칭” 방지
            if (!matchedKey) return 0;

            return score;
        }
    }
}