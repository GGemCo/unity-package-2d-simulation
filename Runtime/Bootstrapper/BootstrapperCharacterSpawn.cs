using GGemCo2DControl;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// Core의 캐릭터 생성 이벤트를 구독하여 시뮬레이션 툴 처리를 자동 부착
    /// </summary>
    public class BootstrapperCharacterSpawn : MonoBehaviour
    {
        private void OnEnable()
        {
            CharacterManager.OnCharacterSpawned   += OnCharacterSpawned;
            CharacterManager.OnCharacterDestroyed += OnCharacterDestroyed;
            CharacterBase.OnCharacterUseTool += OnCharacterUseTool;
            ToolController.OnPlayerEquipSimulationTool += OnPlayerEquipSimulationTool;
            ToolController.OnPlayerUnEquipSimulationTool += OnPlayerUnEquipSimulationTool;
            
            CharacterBase.OnCharacterUseSeed += OnCharacterUseSeed;
        }
        private void OnDisable()
        {
            CharacterManager.OnCharacterSpawned   -= OnCharacterSpawned;
            CharacterManager.OnCharacterDestroyed -= OnCharacterDestroyed;
            CharacterBase.OnCharacterUseTool -= OnCharacterUseTool;
            ToolController.OnPlayerEquipSimulationTool -= OnPlayerEquipSimulationTool;
            ToolController.OnPlayerUnEquipSimulationTool -= OnPlayerUnEquipSimulationTool;
                
            CharacterBase.OnCharacterUseSeed -= OnCharacterUseSeed;
        }

        private void OnCharacterSpawned(CharacterBase ch)
        {
# if GGEMCO_USE_SPINE
            
#else

#endif
            // 플레이어 타입이 아니면 return 처리
            SetPlayerSetting(ch);
            SetNpcSetting(ch);
        }

        private void SetNpcSetting(CharacterBase ch)
        {
        }

        private void SetPlayerSetting(CharacterBase ch)
        {
            if (!ch.IsPlayer()) return; 
            if (ch.GetComponent<GridProbe>() == null)
            {
                var gridProbe = ch.gameObject.AddComponent<GridProbe>();
            }
            // ControllerTool 추가하기
            if (ch.GetComponent<ControllerTool>() == null)
            {
                var controllerTool = ch.gameObject.AddComponent<ControllerTool>();
            }
            
            // HitLocationVisualizer 추가하기
            if (ch.GetComponent<HitLocationVisualizer>() == null)
            {
                var hitLocationVisualizer = ch.gameObject.AddComponent<HitLocationVisualizer>();
            }

            if (ch.GetComponent<ActionSimulationTool>() == null)
            {
                var actionSimulationTool = ch.gameObject.AddComponent<ActionSimulationTool>();
                var inputManager = ch.gameObject.GetComponent<InputManager>();
                if (inputManager == null)
                {
                    GcLogger.LogError($"플레이어에 {nameof(InputManager)}가 없습니다.");
                }
                else
                {
                    inputManager.SetToolAction(actionSimulationTool);
                }
            }
        }

        private void OnCharacterDestroyed(CharacterBase ch)
        {
            // 필요 시 언바인드/풀 반환/로그 등 처리
        }
        /// <summary>
        /// 애니메이션 Event UseTool 호출 시 
        /// </summary>
        /// <param name="ch"></param>
        private void OnCharacterUseTool(CharacterBase ch)
        {
            if (!ch) return;
            var controllerTool = ch.GetComponent<ControllerTool>();
            if (!controllerTool) return;
            controllerTool.UseTool();
        }

        private void OnPlayerEquipSimulationTool(CharacterBase ch, StruckTableItem equippedTool)
        {
            if (!AddressableLoaderToolDefinition.Instance)
            {
                GcLogger.LogError($"AddressableLoaderToolDefinition 오브젝트가 만들어지지 않았습니다.");
                return;
            }
            if (!ch) return;
            var controllerTool = ch.GetComponent<ControllerTool>();
            if (!controllerTool)
            {
                GcLogger.LogError($"플레이어에 {nameof(ControllerTool)}이 없습니다.");
                return;
            }
            
            if (equippedTool == null) return;
            
            var key = $"{ConfigAddressableLabel.SimulationToolDefinition}_{equippedTool.Uid}";
            var toolDefinition = AddressableLoaderToolDefinition.Instance.GetToolDefinitionByLabel(key);
            
            if (!toolDefinition)
            {
                GcLogger.LogError($"Tool Definition 스크립터블 오브젝트가 없습니다. itemUid:{equippedTool.Uid}, Addressables Key:{key}");
                return;
            }
            // 씨앗을 들었다면 wait 애니메이션을 바꿔주어야 하기 때문에 Stop 호출
            ch.Stop(true);
            ch.ChangePickUpSprite();
            
            controllerTool.ChangeTool(toolDefinition, equippedTool.Uid);
        }

        private void OnPlayerUnEquipSimulationTool(CharacterBase ch, StruckTableItem equippedItems)
        {
            // 씨앗을 들었다면 wait 애니메이션을 바꿔주어야 하기 때문에 Stop 호출
            ch.Stop(true);
            ch.ChangePickUpSprite();
        }

        /// <summary>
        /// 애니메이션 Event UseSeed 호출 시 
        /// </summary>
        /// <param name="ch"></param>
        private void OnCharacterUseSeed(CharacterBase ch)
        {
            if (!ch) return;
            var controllerTool = ch.GetComponent<ControllerTool>();
            if (!controllerTool) return;
            controllerTool.UseSeed();
        }
    }
}