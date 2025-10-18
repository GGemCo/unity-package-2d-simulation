using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    [DisallowMultipleComponent]
    public class HitLocationVisualizer : MonoBehaviour
    {
        private AutoTilemapRegistry _registry;
        private TileBase _previewTile;
        private Color _valid = new(1,1,1,0.6f);
        private Color _invalid = new(1,0,0,0.6f);

        private readonly HashSet<Vector3Int> _painted = new();

        private void Awake()
        {
            _registry = GameObject.FindWithTag(ConfigTags.GetValue(ConfigTags.Keys.GridTileMap))?.GetComponent<AutoTilemapRegistry>();
            
            _previewTile = AddressableLoaderSettingsSimulation.Instance.simulationSettings.previewTile;
            _valid = AddressableLoaderSettingsSimulation.Instance.simulationSettings.valid;
            _invalid = AddressableLoaderSettingsSimulation.Instance.simulationSettings.invalid;
        }

        public void Apply(HashSet<Vector3Int> ok, HashSet<Vector3Int> ng)
        {
            var tilemap = _registry.GetPreview();
            // 지우기
            foreach (var c in _painted)
                if (!ok.Contains(c) && !ng.Contains(c))
                    tilemap.SetTile(c, null);
            _painted.Clear();

            // 그리기
            foreach (var c in ok)
            {
                tilemap.SetTile(c, _previewTile);
                tilemap.SetColor(c, _valid);
                _painted.Add(c);
            }
            foreach (var c in ng)
            {
                tilemap.SetTile(c, _previewTile);
                tilemap.SetColor(c, _invalid);
                _painted.Add(c);
            }
        }

        public void Clear()
        {
            _registry.GetPreview().ClearAllTiles();
            _painted.Clear();
        }
    }
}