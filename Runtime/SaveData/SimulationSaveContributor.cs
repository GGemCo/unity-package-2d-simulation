using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// 누적 스냅샷을 내부에 보관한다.
    /// - Capture(): Dirty 셀만 읽어 누적본(_accumDto)에 반영
    /// - Save 시: 항상 누적본 전체를 Envelope에 넣어 SaveDataManager로 전달
    /// </summary>
    public sealed class SimulationSaveContributor : ISaveContributor
    {
        public const string Section = "simulation.gridinfo";
        public string SectionKey => Section;
        public int Priority => 200;

        private SimulationDirtyTracker _dirty;
        private SimulationPackageManager _manager;
        private GridInformation _gridInformation;
        // SimulationDirtyTracker의 "전체 삭제 토큰"과 일치해야 함
        private const string SimulationDirtyTrackerAllKeysToken = "<__ALL__>";

        // 누적 DTO (항상 최신 상태를 보관)
        private readonly SimulationSaveDTO _accumDto = new SimulationSaveDTO
        {
            version = 1,
            grids = new List<GridInfoSnapshot>()
        };

        // gridPath → snapshot 인덱스(빠른 접근용)
        private readonly Dictionary<string, GridInfoSnapshot> _gridIndex = new();

        public SimulationSaveContributor(SimulationDirtyTracker dirty, SimulationPackageManager manager)
        {
            _dirty = dirty;
            _manager = manager;
        }

        /// <summary>외부에서(새 게임/슬롯 삭제 시 등) 누적본 초기화가 필요하면 호출</summary>
        public void ClearAccumulated()
        {
            _accumDto.version = 1;
            _accumDto.grids.Clear();
            _gridIndex.Clear();
        }

        public void Capture(SaveEnvelope env)
        {
            bool anyChanged = false;

            var gi = SceneGame.Instance.mapManager.GetGridInformation();
            if (!gi) return;

            var gridPath = gi.gameObject.GetHierarchyPath();
            var gridSnap = EnsureGridSnapshot(gridPath);
            var cellIndex = EnsureCellIndex(gridSnap);

            // ---------- [1] ERASE 우선 처리 ----------
            var erasedMap = _dirty.ConsumeErasedFor(gi); // (cell → keys)
            if (erasedMap != null && erasedMap.Count > 0)
            {
                foreach (var kv in erasedMap)
                {
                    var cell = kv.Key;
                    var keys = kv.Value; // null일 일은 없음. 전체 삭제는 ALL_KEYS_TOKEN 포함

                    if (cellIndex.TryGetValue(cell, out var kvDict))
                    {
                        if (keys.Contains(SimulationDirtyTrackerAllKeysToken))
                        {
                            // 전체 키 삭제
                            kvDict.Clear();
                            RemoveCellIfEmpty(gridSnap, cellIndex, cell);
                        }
                        else
                        {
                            // 일부 키 삭제
                            foreach (var key in keys)
                                kvDict.Remove(key);
                            RemoveCellIfEmpty(gridSnap, cellIndex, cell);
                        }
                        anyChanged = true;
                    }
                    else
                    {
                        // 누적에 없지만 ERASE가 들어온 경우: 무시(일관성 상 문제 없음)
                    }
                }
            }
            // ---------- [2] DIRTY 업데이트 반영 ----------
            foreach (var cell in _dirty.GetDirtyCellsFor(gi))
            {
                var upserts = new List<GridInfoKV>();
                foreach (var key in ConfigGridInformationKey.All)
                {
                    // 존재하지 않는(혹은 초기값 -1로 간주) 키는 "지움 처리"로 해석 → 누적에서 제거
                    if (gi.GetPositionProperty(cell, key, -1) == -1)
                    {
                        // 누적에 있으면 제거
                        if (cellIndex.TryGetValue(cell, out var kvDict) && kvDict.Remove(key))
                        {
                            RemoveCellIfEmpty(gridSnap, cellIndex, cell);
                            anyChanged = true;
                        }
                        continue;
                    }

                    if (TryReadWithGet(gi, cell, key, out var kv))
                        upserts.Add(kv);
                }

                if (upserts.Count == 0) continue;

                if (!cellIndex.TryGetValue(cell, out var dict))
                {
                    dict = new Dictionary<string, GridInfoKV>();
                    cellIndex[cell] = dict;
                }
                foreach (var e in upserts)
                    dict[e.key] = e;

                anyChanged = true;
            }

            // Dirty가 처리됐으면 비우기
            if (anyChanged) _dirty.Clear(gi);

            // ---------- [3] 항상 누적본 전체를 전달 ----------
            env.SetSection(Section, BuildDtoFromIndex());
        }
        // 삭제 유틸: 셀 딕셔너리가 비면 인덱스/스냅샷에서 제거
        private static void RemoveCellIfEmpty(
            GridInfoSnapshot gridSnap,
            Dictionary<Vector3Int, Dictionary<string, GridInfoKV>> cellIndex,
            Vector3Int cell)
        {
            if (!cellIndex.TryGetValue(cell, out var dict)) return;
            if (dict.Count > 0) return;

            cellIndex.Remove(cell);

            // 스냅샷 리스트도 정리(선택적이지만 메모리/정합상 권장)
            if (gridSnap.cells != null)
            {
                for (int i = 0; i < gridSnap.cells.Count; i++)
                {
                    if (gridSnap.cells[i].cell == cell)
                    {
                        gridSnap.cells.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        public void Restore(SaveEnvelope env)
        {
            // 파일에서 읽은 최신 스냅샷을 누적본으로 반영(=메모리 최신화)
            if (!env.TryGetSection(Section, out SimulationSaveDTO dto) || dto?.grids == null) return;
            
            ClearAccumulated();
            // 1) DTO -> 누적 인덱스 채우기
            foreach (var g in dto.grids)
            {
                var snap = EnsureGridSnapshot(g.gridPath);
                var cellIndex = EnsureCellIndex(snap);

                if (g.cells == null) continue;
                foreach (var c in g.cells)
                {
                    if (!cellIndex.TryGetValue(c.cell, out var kvDict))
                    {
                        kvDict = new Dictionary<string, GridInfoKV>();
                        cellIndex[c.cell] = kvDict;
                    }
                    if (c.entries == null) continue;
                    foreach (var e in c.entries)
                        kvDict[e.key] = e;
                }
            }
            
            // 2) 인덱스에서 snap.cells 재구성 (여기 빠지면 snap.cells가 비어있음)
            foreach (var snap in _accumDto.grids)
                SyncSnapshotCellsFromIndex(snap);
        }
        
        public void UpdateToGridInfo(GridInformation gridInformation)
        {
            // 3) 실제 GridInformation에도 반영 (이제 snap.cells 사용 가능)
            foreach (var snap in _accumDto.grids)
            {
                if (!gridInformation || snap.cells == null) continue;

                foreach (var cell in snap.cells)
                {
                    if (cell.entries == null) continue;
                    foreach (var e in cell.entries)
                    {
                        switch (e.type)
                        {
                            case "bool":
                                if (bool.TryParse(e.value, out var b))
                                    gridInformation.SetPositionProperty(cell.cell, e.key, b ? 1 : 0); // bool→int(0/1)
                                break;
                            case "int":
                                if (int.TryParse(e.value, out var i))
                                    gridInformation.SetPositionProperty(cell.cell, e.key, i);
                                break;
                            case "float":
                                if (float.TryParse(e.value, out var f))
                                    gridInformation.SetPositionProperty(cell.cell, e.key, f);
                                break;
                            case "string":
                                gridInformation.SetPositionProperty(cell.cell, e.key, e.value ?? string.Empty);
                                break;
                            case "Vector3Int":
                                gridInformation.SetPositionProperty(cell.cell, e.key, e.value ?? "0,0,0"); // V3Int는 string 저장
                                break;
                        }
                    }
                }
            }
        }

        // ------------------------------------------------------------
        // 내부 유틸 (누적본 관리 & 값 읽기)
        // ------------------------------------------------------------
        // 인덱스 -> snap.cells 재구성
        private static void SyncSnapshotCellsFromIndex(GridInfoSnapshot snap)
        {
            var idx = EnsureCellIndex(snap);
            snap.cells = new List<CellInfo>(idx.Count);

            foreach (var pair in idx)
            {
                // pair.Key : Vector3Int(셀), pair.Value : Dictionary<string, GridInfoKV>
                var entries = new List<GridInfoKV>(pair.Value.Count);
                foreach (var kv in pair.Value.Values)
                    entries.Add(kv);

                snap.cells.Add(new CellInfo
                {
                    cell = pair.Key,
                    entries = entries
                });
            }
        }
        private GridInfoSnapshot EnsureGridSnapshot(string gridPath)
        {
            if (!_gridIndex.TryGetValue(gridPath, out var snap))
            {
                snap = new GridInfoSnapshot { gridPath = gridPath, cells = new List<CellInfo>() };
                _accumDto.grids.Add(snap);
                _gridIndex[gridPath] = snap;
            }
            return snap;
        }

        // gridSnap.cells 를 Dictionary<Vector3Int, Dictionary<string, GridInfoKV>>로 매핑
        private static Dictionary<Vector3Int, Dictionary<string, GridInfoKV>> EnsureCellIndex(GridInfoSnapshot gridSnap)
        {
            // cells 리스트를 최초 한 번 해시화해서 캐시 (간단 구현을 위해 Tag로 보관)
            // 실제 프로젝트에서는 별도 캐시 필드/클래스로 빼세요.
            if (gridSnap == null) return null;
            if (_cellsCache.TryGetValue(gridSnap, out var map)) return map;

            map = new Dictionary<Vector3Int, Dictionary<string, GridInfoKV>>();
            if (gridSnap.cells != null)
            {
                foreach (var c in gridSnap.cells)
                {
                    var dict = new Dictionary<string, GridInfoKV>();
                    if (c.entries != null)
                        foreach (var kv in c.entries) dict[kv.key] = kv;
                    map[c.cell] = dict;
                }
            }
            _cellsCache[gridSnap] = map;
            return map;
        }

        // 누적 인덱스를 DTO 구조로 재구성
        private SimulationSaveDTO BuildDtoFromIndex()
        {
            // 이미 _accumDto는 인덱스 기반으로 관리되고 있으므로,
            // 최신 인덱스를 리스트 형태로 재구성해 반환
            var outDto = new SimulationSaveDTO { version = _accumDto.version, grids = new List<GridInfoSnapshot>(_accumDto.grids.Count) };

            foreach (var g in _accumDto.grids)
            {
                var cellIndex = EnsureCellIndex(g);
                var cellsOut = new List<CellInfo>(cellIndex.Count);
                foreach (var kv in cellIndex)
                {
                    var entriesOut = new List<GridInfoKV>(kv.Value.Count);
                    foreach (var e in kv.Value.Values) entriesOut.Add(e);
                    cellsOut.Add(new CellInfo { cell = kv.Key, entries = entriesOut });
                }

                outDto.grids.Add(new GridInfoSnapshot
                {
                    gridPath = g.gridPath,
                    cells = cellsOut
                });
            }
            return outDto;
        }

        // 간이 캐시(데모용). 실제 프로젝트에선 별도 클래스로 이전 권장.
        private static readonly Dictionary<GridInfoSnapshot, Dictionary<Vector3Int, Dictionary<string, GridInfoKV>>> _cellsCache
            = new();

        private enum TypeHint { Bool, Int, Float, String, Vector3Int, Unknown }

        private static TypeHint GetTypeHint(string key)
        {
            switch (key)
            {
                case ConfigGridInformationKey.KeyHoed:           return TypeHint.Bool;
        
                case ConfigGridInformationKey.KeyWet:            return TypeHint.Bool;
                case ConfigGridInformationKey.KeyWetUntil:       return TypeHint.Int;
                case ConfigGridInformationKey.KeyWetPrevRole:    return TypeHint.Int;
                case ConfigGridInformationKey.KeyWetCount:       return TypeHint.Int;
                
                case ConfigGridInformationKey.KeySeedItemUid:    return TypeHint.Int;
                case ConfigGridInformationKey.KeySeedStep:       return TypeHint.Int;
                default:               return TypeHint.Unknown;
            }
        }

        /// <summary>
        /// GridInformation의 형식별 오버로드를 사용해 값을 읽고, 누적용 KV를 만든다.
        /// - bool → int(0/1)로 저장/복원
        /// - Vector3Int → string("x,y,z")로 저장/복원
        /// </summary>
        private static bool TryReadWithGet(GridInformation gi, Vector3Int cell, string key, out GridInfoKV kv)
        {
            kv = default;

            switch (GetTypeHint(key))
            {
                case TypeHint.Bool:
                {
                    int raw = gi.GetPositionProperty(cell, key, 0);
                    bool v = raw != 0;
                    kv = new GridInfoKV { key = key, type = "bool", value = v.ToString() };
                    return true;
                }
                case TypeHint.Int:
                {
                    int v = gi.GetPositionProperty(cell, key, 0);
                    kv = new GridInfoKV { key = key, type = "int", value = v.ToString() };
                    return true;
                }
                case TypeHint.Float:
                {
                    float v = gi.GetPositionProperty(cell, key, 0f);
                    kv = new GridInfoKV { key = key, type = "float", value = v.ToString("R") };
                    return true;
                }
                case TypeHint.String:
                {
                    string v = gi.GetPositionProperty(cell, key, string.Empty);
                    kv = new GridInfoKV { key = key, type = "string", value = v ?? string.Empty };
                    return true;
                }
                case TypeHint.Vector3Int:
                {
                    // Vector3Int 오버로드 없음 → string 보관
                    string s = gi.GetPositionProperty(cell, key, string.Empty);
                    if (string.IsNullOrEmpty(s)) s = "0,0,0";
                    kv = new GridInfoKV { key = key, type = "Vector3Int", value = s };
                    return true;
                }
                default:
                    return false;
            }
        }
    }
}
