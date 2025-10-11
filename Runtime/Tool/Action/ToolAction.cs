using UnityEngine;

namespace GGemCo2DSimulation
{
    public abstract class ToolAction : ScriptableObject
    {
        public abstract ValidationResult Validate(ActionContext ctx);
        public abstract void Execute(ActionContext ctx);
    }
}