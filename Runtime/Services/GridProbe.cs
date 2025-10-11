using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DSimulation
{
    public class GridProbe : MonoBehaviour
    {
        public Grid grid;
        public Camera cam;

        private void Awake()
        {
            grid = GameObject.FindWithTag(ConfigTags.GetValue(ConfigTags.Keys.GridTileMap))?.GetComponent<Grid>();
        }

        public void Start()
        {
            if (!SceneGame.Instance) return;
            cam = SceneGame.Instance.mainCamera;
        }

        public Vector3Int GetCursorCell(Vector2 screenPos)
        {
            var world = cam.ScreenToWorldPoint(screenPos);
            return grid.WorldToCell(world);
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