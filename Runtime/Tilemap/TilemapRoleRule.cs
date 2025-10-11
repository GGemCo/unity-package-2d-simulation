using System.Text.RegularExpressions;
using UnityEngine;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = "GGemCo/Tilemap/RoleRule")]
    public class TilemapRoleRule : ScriptableObject
    {
        [Header("Target Role")] public TileRole role;

        [Header("Name / Sorting / Tag / Layer")]
        public string nameRegex; // 예: "(?i)ground|floor"

        public string sortingLayerName; // 예: "Ground"
        public int sortingOrderMin = int.MinValue;
        public int sortingOrderMax = int.MaxValue;
        public string unityTag; // Unity Tag
        public int unityLayer = -1; // Unity Layer(-1=무시)

        [Header("Components")] public bool requireCollider; // TilemapCollider2D 존재 필수?
        public bool forbidCollider; // 있으면 제외?
        public bool requireRenderer = true;

        [Header("Scoring")] [Range(1, 100)] public int weight = 10; // 규칙이 맞으면 가산점

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
                    // return 0;
                }
            }

            // 2) Sorting Layer Name (명시된 경우에만 판단)
            var r = go.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
            if (!string.IsNullOrEmpty(sortingLayerName))
            {
                if (r && r.sortingLayerName == sortingLayerName)
                {
                    score += weight;
                    matchedKey = true;
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
                if (go.CompareTag(unityTag)) { score += weight; matchedKey = true; }
                else
                {
                    // 필요 시 강제 탈락:
                    // return 0;
                }
            }
            if (unityLayer >= 0)
            {
                if (go.layer == unityLayer) { score += weight; matchedKey = true; }
                else
                {
                    // 필요 시 강제 탈락:
                    // return 0;
                }
            }

            // 4) Sorting Order 범위는 “보조 조건”으로만 사용
            if (r)
            {
                if (r.sortingOrder >= sortingOrderMin && r.sortingOrder <= sortingOrderMax)
                    score += weight; // 선택적 가산점
            }
            else if (requireRenderer) return 0;

            // 5) Collider 제약
            var col = go.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
            if (requireCollider && !col) return 0;
            if (forbidCollider  &&  col) return 0;
            if (requireCollider &&  col) score += weight;
            if (forbidCollider  && !col) score += weight;

            // 6) 핵심 키가 하나도 안 맞으면 “노이즈 매칭” 방지
            if (!matchedKey) return 0;

            return score;
        }
    }
}