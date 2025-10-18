using System;
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// 시뮬레이션 패키지 전역 설정
    /// - 타일 미리보기 색상/타일
    /// - 기본 타일(괭이/젖음/빈 타일)
    /// - 타일맵 역할 매칭 규칙(rules)
    /// - 프리뷰 타일맵 렌더 설정
    /// - 역할별 커스텀 타일 매핑(옵션)
    /// </summary>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectSimulation.SimulationSettings.FileName,
        menuName = ConfigScriptableObjectSimulation.SimulationSettings.MenuName,
        order    = ConfigScriptableObjectSimulation.SimulationSettings.Ordering)]
    public class GGemCoSimulationSettings : ScriptableObject, ISettingsChangeNotifier
    {
        // 런타임/에디터 공용 알림 (직렬화되지 않음)
        public event Action Changed;

#if UNITY_EDITOR
        // 인스펙터 값 변경 시 호출(에디터 전용)
        private void OnValidate()
        {
            // 색상 알파가 0~1 범위 밖으로 나갈 가능성 방지(실수 대비)
            valid.a   = Mathf.Clamp01(valid.a);
            invalid.a = Mathf.Clamp01(invalid.a);

            ClearCache();
            Changed?.Invoke();
        }
#endif

        /// <summary>툴/코드에서 강제 알림</summary>
        public void RaiseChanged()
        {
            ClearCache();
            Changed?.Invoke();
        }

        // ─────────────────────────────────────────────────────────────────────
        // 타일 미리보기
        // ─────────────────────────────────────────────────────────────────────
        [Header("타일 미리보기")]
        [Tooltip("미리 보기에 사용될 타일(룰타일 포함 가능)")]
        public TileBase previewTile;

        [Tooltip("도구 사용 가능한 타일 프리뷰 색상(알파 포함)")]
        public Color valid = new(1f, 1f, 1f, 0.6f);

        [Tooltip("도구 사용 불가능한 타일 프리뷰 색상(알파 포함)")]
        public Color invalid = new(1f, 0f, 0f, 0.6f);

        // ─────────────────────────────────────────────────────────────────────
        // 기본 타일셋
        // ─────────────────────────────────────────────────────────────────────
        [Header("타일셋 (기본 폴백)")]
        [Tooltip("경작(괭이) 상태에 사용할 기본 타일")]
        public TileBase hoedTile;

        [Tooltip("젖음(물주기) 상태에 사용할 기본 타일")]
        public TileBase wetTile;

        [Tooltip("비어 있는 상태에 사용할 기본 타일")]
        public TileBase emptyTile;

        // ─────────────────────────────────────────────────────────────────────
        // 역할 매칭 규칙
        // ─────────────────────────────────────────────────────────────────────
        [Header("Rules (위 → 아래 순서로 평가)")]
        [Tooltip("Tilemap을 역할(Role)로 분류하기 위한 규칙 목록입니다. 위에서 아래 순서로 평가됩니다.")]
        public List<TilemapRoleRule> rules = new();

        // ─────────────────────────────────────────────────────────────────────
        // 프리뷰 타일맵 렌더 설정
        // ─────────────────────────────────────────────────────────────────────
        [Header("프리뷰 타일맵 렌더 설정")]
        [Tooltip("프리뷰 Tilemap GameObject 이름")]
        public string previewObjectName = $"{ConfigDefine.NameSDK}_Tilemap_Preview";

        [Tooltip("프리뷰 Tilemap Sorting Layer 키")]
        public ConfigSortingLayer.Keys previewSortingLayer = ConfigSortingLayer.Keys.MapTerrain;

        [Tooltip("프리뷰 Tilemap Sorting Order (높을수록 위)")]
        public int previewSortingOrder = 999;

        // ─────────────────────────────────────────────────────────────────────
        // 내부 캐시
        // ─────────────────────────────────────────────────────────────────────
        private readonly Dictionary<TileRole, TileBase> _cache = new();

        /// <summary>
        /// Role에 대응하는 타일을 반환합니다.
        /// 우선순위: 커스텀(RoleTile) 지정 타일 → 기본 폴백(hoed/wet/empty) → null
        /// </summary>
        public TileBase GetTile(TileRole role)
        {
            if (_cache.TryGetValue(role, out var cached) && cached)
                return cached;

            if ((role & TileRole.GroundHoed) != 0 && hoedTile)  return Cache(role, hoedTile);
            if ((role & TileRole.GroundWet)  != 0 && wetTile)   return Cache(role, wetTile);
            if ((role & TileRole.Empty)      != 0 && emptyTile) return Cache(role, emptyTile);

            return null;
        }

        private TileBase Cache(TileRole role, TileBase tile)
        {
            if (tile) _cache[role] = tile;
            return tile;
        }

        /// <summary>내부 타일 캐시 비우기(설정 변경/로드 후 호출 권장)</summary>
        public void ClearCache() => _cache.Clear();

#if UNITY_EDITOR
        [ContextMenu("Rebuild Cache & Raise Changed")]
        private void RebuildCacheAndNotify()
        {
            ClearCache();
            Changed?.Invoke();
        }
#endif
    }
}
