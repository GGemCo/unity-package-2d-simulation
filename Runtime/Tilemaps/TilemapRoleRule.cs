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

        public int Score(GameObject go)
        {
            int score = 0;

            // 이름 패턴
            if (!string.IsNullOrEmpty(nameRegex))
            {
                if (Regex.IsMatch(go.name, nameRegex)) score += weight;
            }

            // Sorting Layer/Order
            var r = go.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
            if (r)
            {
                if (!string.IsNullOrEmpty(sortingLayerName) &&
                    r.sortingLayerName == sortingLayerName) score += weight;

                if (r.sortingOrder >= sortingOrderMin && r.sortingOrder <= sortingOrderMax) score += weight;
            }
            else if (requireRenderer) return 0;

            // Tag/Layer
            if (!string.IsNullOrEmpty(unityTag) && go.CompareTag(unityTag)) score += weight;
            if (unityLayer >= 0 && go.layer == unityLayer) score += weight;

            // Collider 요구/금지
            var col = go.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
            if (requireCollider && col) score += weight;
            if (forbidCollider && !col) score += weight;
            if (requireCollider && !col) return 0;
            if (forbidCollider && col) return 0;

            return score;
        }
    }
}