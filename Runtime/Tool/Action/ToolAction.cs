using UnityEngine;

namespace GGemCo2DSimulation
{
    public abstract class ToolAction : ScriptableObject
    {
        public abstract ValidationResult Validate(ToolActionContext ctx);
        public abstract void Execute(ToolActionContext ctx);
    }
}