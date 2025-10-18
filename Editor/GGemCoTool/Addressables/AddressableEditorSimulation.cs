using GGemCo2DCore;
using UnityEditor;
using UnityEngine;
using GGemCo2DCoreEditor;
using GGemCo2DSimulationEditorr;

namespace GGemCo2DSimulationEditor
{
    public class AddressableEditorSimulation : DefaultEditorWindow
    {
        private const string Title = "Addressable 셋팅하기";
        public TableSimulationTool tableSimulationTool;
        public TableSimulationGrowth tableSimulationGrowth;
        public float buttonWidth;
        public float buttonHeight;
        
        private SettingScriptableObjectSimulation _settingScriptableObjectSimulation;
        private SettingToolDefinition _settingToolDefinition;
        private SettingGrowth _settingGrowth;
        private Vector2 _scrollPosition;

        [MenuItem(ConfigEditorSimulation.NameToolSettingAddressable, false, (int)ConfigEditorSimulation.ToolOrdering.SettingAddressable)]
        public static void ShowWindow()
        {
            GetWindow<AddressableEditorSimulation>(Title);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            // _settingMap 에서 테이블을 사용하기 때문에 테이블 먼저 로드해야 함
            LoadTables();
            
            buttonHeight = 40f;
            _settingScriptableObjectSimulation = new SettingScriptableObjectSimulation(this);
            _settingToolDefinition = new SettingToolDefinition(this);
            _settingGrowth = new SettingGrowth(this);
        }

        private void LoadTables()
        {
            tableSimulationTool = TableLoaderManager.LoadSimulationToolTable();
            tableSimulationGrowth = TableLoaderManager.LoadSimulationGrowthTable();
        }

        private void OnGUI()
        {
            buttonWidth = position.width / 2f - 10f;
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            // EditorGUILayout.HelpBox("캐릭터 추가 후 맵을 추가해야 맵별 배치되어있는 캐릭터 정보가 반영됩니다.", MessageType.Error);
            
            EditorGUILayout.BeginHorizontal();
            _settingScriptableObjectSimulation.OnGUI();
            _settingToolDefinition.OnGUI();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            _settingGrowth.OnGUI();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(20);
            EditorGUILayout.EndScrollView();
        }
    }
}