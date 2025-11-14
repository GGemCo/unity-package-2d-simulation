using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DSimulationEditorr;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DSimulationEditor
{
    /// <summary>
    /// 인트로 씬 설정 툴
    /// </summary>
    public class SceneEditorGameSimulation : DefaultSceneEditorSimulatioin
    {
        private const string Title = "게임 씬 셋팅하기";
        private GameObject _objGGemCoCore;
        
        [MenuItem(ConfigEditorSimulation.NameToolSettingSceneGame, false, (int)ConfigEditorSimulation.ToolOrdering.SettingSceneGame)]
        public static void ShowWindow()
        {
            GetWindow<SceneEditorGameSimulation>(Title);
        }

        private void OnGUI()
        {
            if (!CheckCurrentLoadedScene(ConfigDefine.SceneNameGame))
            {
                EditorGUILayout.HelpBox($"게임 씬을 불러와 주세요.", MessageType.Error);
            }
            else
            {
                DrawRequiredSection();
            }
        }
        private void DrawRequiredSection()
        {
            HelperEditorUI.OnGUITitle("필수 항목");
            EditorGUILayout.HelpBox($"* SimulationPackageManager 오브젝트\n", MessageType.Info);
            if (GUILayout.Button("필수 항목 셋팅하기"))
            {
                SetupRequiredObjects();
            }
        }
        /// <summary>
        /// 필수 항목 셋팅
        /// </summary>
        private void SetupRequiredObjects()
        {
            string sceneName = nameof(SceneGame);
            GGemCo2DCore.SceneGame scene = CreateUIComponent.Find(sceneName, ConfigPackageInfo.PackageType.Core)?.GetComponent<SceneGame>();
            if (scene == null) 
            {
                GcLogger.LogError($"{sceneName} 이 없습니다.\nGGemCoTool > 설정하기 > 게임 씬 셋팅하기에서 필수 항목 셋팅하기를 실행해주세요.");
                return;
            }
            _objGGemCoCore = GetOrCreateRootPackageGameObject();
            // GGemCo2DControl.ControlPackageManager GameObject 만들기
            GGemCo2DSimulation.SimulationPackageManager simulationPackageManager =
                CreateOrAddComponent<GGemCo2DSimulation.SimulationPackageManager>(nameof(GGemCo2DSimulation.SimulationPackageManager));
            
            // ControlPackageManager 은 싱글톤으로 활용하고 있어 root 로 이동
            simulationPackageManager.gameObject.transform.SetParent(null);
            
            // 반드시 SetDirty 처리해야 저장됨
            EditorUtility.SetDirty(scene);
        }
    }
}