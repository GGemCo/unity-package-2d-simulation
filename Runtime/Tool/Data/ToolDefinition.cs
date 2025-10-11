using UnityEngine;

namespace GGemCo2DSimulation
{
    public enum DistanceMetric { Manhattan, Chebyshev, Euclidean }

    [CreateAssetMenu(menuName = "GGemCo/Tools/ToolDefinition")]
    public class ToolDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string toolId;
        public Sprite icon;

        [Header("Targeting")]
        public int range = 1;
        public DistanceMetric metric = DistanceMetric.Manhattan;
        public TargetingPolicy targeting;

        [Header("Action")]
        public ToolAction action;

        [Header("Roles")]
        public TileRole readRoles   = TileRole.AnyGround;
        public TileRole blockRoles  = TileRole.Blocking;
        public TileRole writeRole   = TileRole.GroundHoed; // 예: 괭이
    }
}