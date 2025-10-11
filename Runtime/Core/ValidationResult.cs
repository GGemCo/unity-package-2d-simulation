using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DSimulation
{
    public sealed class ValidationResult
    {
        public bool IsValid;
        public HashSet<Vector3Int> ValidCells = new();
        public HashSet<Vector3Int> InvalidCells = new();
        public string Reason;
    }
}