using System.Collections.Generic;
using GGemCo2DCore;
using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace GGemCo2DSimulationEditor
{
    /// <summary>
    /// 설정 ScriptableObject 등록하기
    /// </summary>
    public class SettingToolDefinition : DefaultAddressable
    {
        private const string Title = "툴 Definition ScriptableObject 추가하기";
        private readonly AddressableEditorSimulation _addressableEditorSimulation;

        public SettingToolDefinition(AddressableEditorSimulation addressableEditorSimulationWindow)
        {
            _addressableEditorSimulation = addressableEditorSimulationWindow;
            targetGroupName = ConfigAddressableGroupName.SimulationToolDefinition;
        }

        public void OnGUI()
        {
            // Common.OnGUITitle(Title);

            if (_addressableEditorSimulation.tableSimulationTool == null)
            {
                EditorGUILayout.HelpBox($"{ConfigAddressableTable.SimulationTool} 테이블이 없습니다.", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button(Title, GUILayout.Width(_addressableEditorSimulation.buttonWidth),
                        GUILayout.Height(_addressableEditorSimulation.buttonHeight)))
                {
                    Setup();
                }
            }
        }
        /// <summary>
        /// Addressable 설정하기
        /// </summary>
        private void Setup()
        {
            bool result = EditorUtility.DisplayDialog(TextDisplayDialogTitle, TextDisplayDialogMessage, "네", "아니요");
            if (!result) return;
            
            Dictionary<int, StruckTableSimulationTool> dictionary = _addressableEditorSimulation.tableSimulationTool.GetDatas();
            
            // AddressableSettings 가져오기 (없으면 생성)
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                Debug.LogWarning("Addressable 설정을 찾을 수 없습니다. 새로 생성합니다.");
                settings = CreateAddressableSettings();
            }

            // 그룹 가져오기 또는 생성
            AddressableAssetGroup group = GetOrCreateGroup(settings, targetGroupName);

            if (!group)
            {
                Debug.LogError($"'{targetGroupName}' 그룹을 설정할 수 없습니다.");
                return;
            }
            
            ClearGroupEntries(settings, group);

            // foreach 문을 사용하여 딕셔너리 내용을 출력
            foreach (KeyValuePair<int, StruckTableSimulationTool> outerPair in dictionary)
            {
                var info = outerPair.Value;
                if (info.Uid <= 0 || info.ItemUid <= 0 || string.IsNullOrEmpty(info.DefinitionFileName)) continue;

                string path = $"{ConfigAddressablePath.Simulation.ToolDefinition}/{info.DefinitionFileName}.asset";
                string lable = $"{ConfigAddressableLabel.SimulationToolDefinition}_{info.ItemUid}";
                var entry = Add(settings, group, $"{ConfigAddressableKey.SimulationTool}_{info.DefinitionFileName}", path,
                    lable);
                
                entry.SetLabel(ConfigAddressableLabel.SimulationToolDefinition, true, true);
            }

            // 설정 저장
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(Title, "Addressable 설정 완료", "OK");
        }
    }
}
