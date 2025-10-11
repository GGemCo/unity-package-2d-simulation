using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [DisallowMultipleComponent]
    public class HitLocationVisualizer : MonoBehaviour
    {
        public AutoTilemapRegistry registry;
        public TileBase previewTile;
        public Color valid = new(1,1,1,0.6f);
        public Color invalid = new(1,0,0,0.6f);

        private readonly HashSet<Vector3Int> _painted = new();

        private void Awake()
        {
            registry = GameObject.FindWithTag(ConfigTags.GetValue(ConfigTags.Keys.GridTileMap))?.GetComponent<AutoTilemapRegistry>();
        }

        public void Apply(HashSet<Vector3Int> ok, HashSet<Vector3Int> ng)
        {
            var tilemap = registry.GetPreview();
            // 지우기
            foreach (var c in _painted)
                if (!ok.Contains(c) && !ng.Contains(c))
                    tilemap.SetTile(c, null);
            _painted.Clear();

            // 그리기
            foreach (var c in ok)
            {
                tilemap.SetTile(c, previewTile);
                tilemap.SetColor(c, valid);
                _painted.Add(c);
            }
            foreach (var c in ng)
            {
                tilemap.SetTile(c, previewTile);
                tilemap.SetColor(c, invalid);
                _painted.Add(c);
            }
        }

        public void Clear()
        {
            registry.GetPreview().ClearAllTiles();
            _painted.Clear();
        }
    }
}