// Runtime/Data/ToolRuntimeTiles.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// 도구 액션이 실제로 SetTile할 때 사용할 타일 레퍼런스 컨테이너.
    /// - 역할(Role) → 타일(TileBase) 매핑을 SO 데이터로 관리
    /// - 기본 필드(hoed/wet 등) + 확장 매핑(RoleTiles) 동시 지원
    ///
    /// 사용 예)
    /// var tile = tileset.GetTile(tool.writeRole) ?? tileset.hoedTile;
    /// targetTilemap.SetTile(cell, tile);
    /// </summary>
    [CreateAssetMenu(menuName = "GGemCo/Simulation/Tool Runtime Tiles", fileName = "ToolRuntimeTiles")]
    public class ToolRuntimeTiles : ScriptableObject
    {
        [Header("Common Defaults")]
        [Tooltip("경작(괭이) 상태에 쓸 기본 타일")]
        public TileBase hoedTile;

        [Tooltip("젖음(물주기) 상태에 쓸 기본 타일")]
        public TileBase wetTile;

        [Tooltip("필요시 기본 바닥 대체 타일")]
        public TileBase groundBaseTile;

        [Header("Role → Tile 확장 매핑")]
        [Tooltip("writeRole(예: GroundHoed/GroundWet 등)을 보다 구체적으로 타일에 매핑")]
        public List<RoleTile> roleTiles = new();

        [Serializable]
        public struct RoleTile
        {
            public GGemCo2DSimulation.TileRole role;
            public TileBase tile;

            [Tooltip("선택: Addressables(또는 외부 로더) 키. 런타임에 동적 로드가 필요할 때 사용")]
            public string addressKey;
        }

        private readonly Dictionary<GGemCo2DSimulation.TileRole, TileBase> _cache =
            new Dictionary<GGemCo2DSimulation.TileRole, TileBase>();

        /// <summary>
        /// Role에 대응하는 타일을 반환. 없으면 기본 필드(hoed/wet/groundBase)로 폴백.
        /// </summary>
        public TileBase GetTile(GGemCo2DSimulation.TileRole role)
        {
            // 캐시 조회
            if (_cache.TryGetValue(role, out var cached) && cached)
                return cached;

            // 리스트 검색(상위 비트 포함 가능성 고려)
            for (int i = 0; i < roleTiles.Count; i++)
            {
                var rt = roleTiles[i];
                if (rt.tile && (role & rt.role) == rt.role)
                {
                    _cache[role] = rt.tile;
                    return rt.tile;
                }
            }

            // 역할에 따른 기본 폴백
            if ((role & GGemCo2DSimulation.TileRole.GroundHoed) != 0 && hoedTile)   return hoedTile;
            if ((role & GGemCo2DSimulation.TileRole.GroundWet)  != 0 && wetTile)    return wetTile;
            if ((role & GGemCo2DSimulation.TileRole.AnyGround)  != 0 && groundBaseTile) return groundBaseTile;

            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 간단한 중복/누락 알림 (유지보수 편의)
            // 같은 role의 다중 매핑이 있으면 경고
            var seen = new HashSet<GGemCo2DSimulation.TileRole>();
            foreach (var rt in roleTiles)
            {
                if (rt.role == GGemCo2DSimulation.TileRole.None) continue;
                if (!seen.Add(rt.role))
                    Debug.LogWarning($"[ToolRuntimeTiles] Duplicate role mapping: {rt.role}", this);
            }
        }
#endif
    }
}
