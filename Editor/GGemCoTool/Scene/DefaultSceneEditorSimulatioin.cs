using GGemCo2DCore;

namespace GGemCo2DCoreEditor
{
    public class DefaultSceneEditorSimulatioin : DefaultSceneEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            packageType = ConfigPackageInfo.PackageType.Simulation;
        }
    }
}