using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DSimulation
{
    public class GridProbe : MonoBehaviour
    {
        private Grid _grid;
        private Camera _cam;

        private void Awake()
        {
            _grid = GameObject.FindWithTag(ConfigTags.GetValue(ConfigTags.Keys.GridTileMap))?.GetComponent<Grid>();
        }

        public void Start()
        {
            if (!SceneGame.Instance) return;
            _cam = SceneGame.Instance.mainCamera;
        }

        public Vector3Int GetCursorCell(Vector2 screenPos)
        {
            var world = _cam.ScreenToWorldPoint(screenPos);
            return _grid.WorldToCell(world);
        }

        public static bool InRange(Vector3Int a, Vector3Int b, int range, DistanceMetric metric)
        {
            var dx = Mathf.Abs(a.x - b.x);
            var dy = Mathf.Abs(a.y - b.y);
            return metric switch
            {
                DistanceMetric.Manhattan => (dx + dy) <= range,
                DistanceMetric.Chebyshev => Mathf.Max(dx, dy) <= range,
                _ => (new Vector2Int(dx, dy)).magnitude <= range
            };
        }
    }
}