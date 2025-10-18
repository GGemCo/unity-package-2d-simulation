using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    public class SimulationDirtyTracker : MonoBehaviour
    {
        private readonly Dictionary<GridInformation, HashSet<Vector3Int>> _dirty = new();

        // 지워진 항목 큐: (GridInfo, Cell) → 지운 Key 집합(null 또는 빈 Set = 전체 키 삭제)
        private readonly Dictionary<GridInformation, Dictionary<Vector3Int, HashSet<string>>> _erased
            = new();

        public void MarkDirty(GridInformation gi, Vector3Int cell)
        {
            if (!gi) return;
            if (!_dirty.TryGetValue(gi, out var set))
            {
                set = new HashSet<Vector3Int>();
                _dirty[gi] = set;
            }
            set.Add(cell);
        }

        /// <summary>
        /// 특정 키만 지움 표시. key == null 이면 '셀의 모든 키 삭제'를 의미.
        /// ※ GridInformation.ErasePositionProperty(셀, 키), ErasePositionProperties(셀) 호출 지점에서 같이 호출하세요.
        /// </summary>
        public void MarkErased(GridInformation gi, Vector3Int cell, string key = null)
        {
            if (!gi) return;

            if (!_erased.TryGetValue(gi, out var cellMap))
            {
                cellMap = new Dictionary<Vector3Int, HashSet<string>>();
                _erased[gi] = cellMap;
            }
            if (!cellMap.TryGetValue(cell, out var keys))
            {
                keys = new HashSet<string>();
                cellMap[cell] = keys;
            }

            // key == null: 전체 삭제 플래그로 표현하기 위해 null 대신 빈 Set 클리어 & 특수 토큰 사용
            if (key == null)
            {
                // 전체 삭제 표시: 기존 키 집합을 특별 상태로 대체
                keys.Clear();
                keys.Add(AllKeysToken);
            }
            else
            {
                // 부분 삭제: 전체 삭제 토큰이 이미 있다면 그대로 유지
                if (!keys.Contains(AllKeysToken))
                    keys.Add(key);
            }
        }

        // “전체 키 삭제” 토큰 (실제 키로 쓰일 수 없는 예약 문자열)
        private const string AllKeysToken = "<__ALL__>";

        public IEnumerable<Vector3Int> GetDirtyCellsFor(GridInformation gi)
        {
            if (!_dirty.TryGetValue(gi, out var set)) yield break;
            foreach (var c in set) yield return c;
        }

        /// <summary>해당 GridInformation의 '지움 큐' 전체를 가져온다.</summary>
        public Dictionary<Vector3Int, HashSet<string>> ConsumeErasedFor(GridInformation gi)
        {
            if (!_erased.TryGetValue(gi, out var map)) return null;
            // Capture에서 처리 후 제거할 것이므로 반환 후 바로 remove
            _erased.Remove(gi);
            return map;
        }

        public void Clear(GridInformation gi)
        {
            _dirty.Remove(gi);
            _erased.Remove(gi);
        }

        public void ClearAll()
        {
            _dirty.Clear();
            _erased.Clear();
        }
    }
}
