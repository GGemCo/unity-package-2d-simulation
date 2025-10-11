using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DSimulation
{
    public sealed class ActionContext
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

        // 런타임 타일 모음(SO/MB)
        public ToolRuntimeTiles tileset;
    }
}