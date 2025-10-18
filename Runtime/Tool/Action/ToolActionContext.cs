using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    public sealed class ToolActionContext
    {
        public Transform user;
        public Grid grid;
        public Vector3Int originCell;
        public Vector3Int cursorCell;
        public IReadOnlyCollection<Vector3Int> targetCells;
        public float deltaTime;

        // 레지스트리/프로브/툴 참조
        public AutoTilemapRegistry registry;
        public GridProbe probe;
        public ToolDefinition tool;
        public SimulationDirtyTracker dirtyTracker;

        // 런타임 타일 모음(SO/MB)
        public TileBase defaultTileHoe;
        public TileBase defaultTileWet;

        // 도구, 씨앗 item Uid
        public int itemUid;
    }
}