using System;
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
        // [Header("Rules (위→아래 순서로 평가)")]
        // public List<TilemapRoleRule> rules = new();
        private List<TilemapRoleRule> _rules = new List<TilemapRoleRule>();

        // [Header("Preview (자동 생성)")]
        private Material _previewMaterial;
        private ConfigSortingLayer.Keys _previewSortingLayer;
        private int _previewSortingOrder;
        private string _previewObjectName;

        private readonly Dictionary<TileRole, List<Tilemap>> _byRole = new();

        private Tilemap _preview;

        // === 재진입/내부변경 가드 플래그 ===
        private bool _isDiscovering;
        private bool _suppressChildrenChanged;   // 자식 변동 콜백 무시
        private bool _isCreatingPreview;         // 미리보기 생성 중

        private void Awake()
        {
            if (!AddressableLoaderSettingsSimulation.Instance)
            {
                // GcLogger.LogError($"{nameof(AddressableLoaderSettingsSimulation)} 싱글톤이 없습니다.");
                return;
            }

            if (!AddressableLoaderSettingsSimulation.Instance.simulationSettings)
            {
                GcLogger.LogError($"{nameof(GGemCoSimulationSettings)} 스크립터블 오브젝트가 없습니다.");
                return;
            }

            var simulationSettings = AddressableLoaderSettingsSimulation.Instance.simulationSettings;
            _rules = simulationSettings.rules;
            if (_rules == null)
            {
                GcLogger.LogError($"{nameof(GGemCoSimulationSettings)} 스크립터블 오브젝트에 Rules가 없습니다.");
                return;
            }
            if (_rules.Count == 0)
            {
                GcLogger.LogError($"{nameof(GGemCoSimulationSettings)} 스크립터블 오브젝트에 Rules를 등록해주세요.");
                return;
            }

            _previewSortingLayer = simulationSettings.previewSortingLayer;
            _previewSortingOrder = simulationSettings.previewSortingOrder;
            _previewObjectName = simulationSettings.previewObjectName;
        }

        public Tilemap GetPreview()
        {
            if (_preview) return _preview;
            if (_previewObjectName == null) return null;

            // 이미 존재하면 재사용 (에디터/런타임 모두 안전)
            var exist = transform.Find(_previewObjectName);
            if (exist)
            {
                _preview = exist.GetComponent<Tilemap>() ?? exist.gameObject.AddComponent<Tilemap>();
                var r = exist.GetComponent<TilemapRenderer>() ?? exist.gameObject.AddComponent<TilemapRenderer>();
                r.sortingLayerName = ConfigSortingLayer.GetValue(_previewSortingLayer);
                r.sortingOrder     = _previewSortingOrder;
                if (_previewMaterial) r.material = _previewMaterial;
                return _preview;
            }

            // 새로 생성 — 이 구간 동안 자식 변경 콜백/Discover 재진입 금지
            _isCreatingPreview = true;
            _suppressChildrenChanged = true;
            try
            {
                var go = new GameObject(_previewObjectName)
                {
                    layer = gameObject.layer
                };

                _preview = go.AddComponent<Tilemap>();
                var r = go.AddComponent<TilemapRenderer>();
                r.sortingLayerName = ConfigSortingLayer.GetValue(_previewSortingLayer);
                r.sortingOrder     = _previewSortingOrder;
                if (_previewMaterial) r.material = _previewMaterial;
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

        public Tilemap GetTop(TileRole role)
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
            foreach (var role in EnumHelper.Flags(mask))
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
                    !t || t == _preview || t.gameObject.name == _previewObjectName);

                // 규칙 점수화 → 역할 매핑
                foreach (var tm in tilemaps)
                {
                    var go = tm.gameObject;
                    var roleScores = new Dictionary<TileRole, int>();

                    foreach (var rule in _rules)
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
}
