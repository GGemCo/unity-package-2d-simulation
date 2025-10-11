using System.Collections.Generic;
using System.Linq;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [DisallowMultipleComponent]
    public class AutoTilemapRegistry : MonoBehaviour
    {
        [Header("Rules (위→아래 순서로 평가)")]
        public List<TilemapRoleRule> rules = new();

        [Header("Preview (자동 생성)")]
        public Material previewMaterial;        // 선택
        public ConfigSortingLayer.Keys previewSortingLayer = ConfigSortingLayer.Keys.MapTerrain;
        public int      previewSortingOrder = 999;
        public string   previewObjectName   = "Tilemap_Preview";

        private readonly Dictionary<TileRole, List<Tilemap>> _byRole = new();

        private Tilemap _preview;

        // === 재진입/내부변경 가드 플래그 ===
        private bool _isDiscovering;
        private bool _suppressChildrenChanged;   // 자식 변동 콜백 무시
        private bool _isCreatingPreview;         // 미리보기 생성 중

        public Tilemap GetPreview()
        {
            if (_preview) return _preview;

            // 이미 존재하면 재사용 (에디터/런타임 모두 안전)
            var exist = transform.Find(previewObjectName);
            if (exist)
            {
                _preview = exist.GetComponent<Tilemap>() ?? exist.gameObject.AddComponent<Tilemap>();
                var r = exist.GetComponent<TilemapRenderer>() ?? exist.gameObject.AddComponent<TilemapRenderer>();
                r.sortingLayerName = ConfigSortingLayer.GetValue(previewSortingLayer);
                r.sortingOrder     = previewSortingOrder;
                if (previewMaterial) r.material = previewMaterial;
                return _preview;
            }

            // 새로 생성 — 이 구간 동안 자식 변경 콜백/Discover 재진입 금지
            _isCreatingPreview = true;
            _suppressChildrenChanged = true;
            try
            {
                var go = new GameObject(previewObjectName)
                {
                    layer = gameObject.layer
                };

                _preview = go.AddComponent<Tilemap>();
                var r = go.AddComponent<TilemapRenderer>();
                r.sortingLayerName = ConfigSortingLayer.GetValue(previewSortingLayer);
                r.sortingOrder     = previewSortingOrder;
                if (previewMaterial) r.material = previewMaterial;
                go.transform.SetParent(transform, false);
                // Collider 불필요 (미리보기 전용)
            }
            finally
            {
                _isCreatingPreview = false;
                _suppressChildrenChanged = false;
            }

            return _preview;
        }

        private IEnumerable<Tilemap> GetByRole(TileRole role)
            => _byRole.TryGetValue(role, out var list) ? list : Enumerable.Empty<Tilemap>();

        private Tilemap GetTop(TileRole role)
        {
            Tilemap top = null; var best = int.MinValue;
            foreach (var tm in GetByRole(role))
            {
                var r = tm ? tm.GetComponent<TilemapRenderer>() : null;
                var so = r ? r.sortingOrder : 0;
                if (so > best) { best = so; top = tm; }
            }
            return top;
        }

        public bool AnyTileAt(Vector3Int cell, TileRole mask)
        {
            foreach (var role in EnumUtil.Flags(mask))
                foreach (var tm in GetByRole(role))
                    if (tm && tm.HasTile(cell)) return true;
            return false;
        }

        public Tilemap ResolveWriteTarget(TileRole desiredRole, Vector3Int cell)
        {
            var target = GetTop(desiredRole);
            if (target) return target;
            if ((desiredRole & TileRole.AnyGround) != 0)
                return GetTop(TileRole.GroundBase);
            return null;
        }

        private void Discover(bool includeInactive = false)
        {
            if (_isDiscovering) return;
            _isDiscovering = true;

            try
            {
                _byRole.Clear();

                // 씬의 모든 Grid 하위 Tilemap 수집
                var grids = GetComponentsInChildren<Grid>(includeInactive);
                var tilemaps = new List<Tilemap>();
                foreach (var g in grids)
                    tilemaps.AddRange(g.GetComponentsInChildren<Tilemap>(includeInactive));

                // Preview 제외
                tilemaps.RemoveAll(t =>
                    !t || t == _preview || t.gameObject.name == previewObjectName);

                // 규칙 점수화 → 역할 매핑
                foreach (var tm in tilemaps)
                {
                    var go = tm.gameObject;
                    var roleScores = new Dictionary<TileRole, int>();

                    foreach (var rule in rules)
                    {
                        if (!rule) continue;
                        int s = rule.Score(go);
                        if (s <= 0) continue;

                        roleScores.TryAdd(rule.role, 0);
                        roleScores[rule.role] += s;
                    }

                    if (roleScores.Count == 0)
                    {
                        AddRole(TileRole.Decor, tm);
                        continue;
                    }

                    foreach (var kv in roleScores)
                        AddRole(kv.Key, tm);
                }

                // 마지막에 Preview 확보 (가드 덕분에 재귀 없음)
                GetPreview();
            }
            finally
            {
                _isDiscovering = false;
            }
        }

        private void AddRole(TileRole role, Tilemap tm)
        {
            if (!_byRole.TryGetValue(role, out var list))
            {
                list = new List<Tilemap>();
                _byRole[role] = list;
            }
            if (!list.Contains(tm)) list.Add(tm);
        }

        private void OnEnable() => Discover();

        private void OnTransformChildrenChanged()
        {
            // 내부 생성/재배치로 인한 이벤트는 무시
            if (_suppressChildrenChanged || _isCreatingPreview) return;

            // 외부 변경만 재탐색
            Discover();
        }
    }
    /// 간단 Flag 열거 유틸
    public static class EnumUtil
    {
        public static IEnumerable<TileRole> Flags(TileRole mask)
        {
            foreach (TileRole v in System.Enum.GetValues(typeof(TileRole)))
                if (v != TileRole.None && (mask & v) == v) yield return v;
        }
    }
}
