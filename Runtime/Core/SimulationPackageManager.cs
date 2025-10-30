using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// Control 패키지의 메인 메니저
    /// SceneGame 과 같은 개념
    /// </summary>
    [DefaultExecutionOrder((int)ConfigCommon.ExecutionOrdering.Simulation)]
    public class SimulationPackageManager : MonoBehaviour
    {
        public static SimulationPackageManager Instance { get; private set; }
        
        [Header("GridInformation 타겟")]
        
        [HideInInspector] public WetDecaySystem wetDecaySystem;
        [HideInInspector] public SimulationDirtyTracker simulationDirtyTracker;
        [HideInInspector] public SimulationSaveContributor simulationSaveContributor;
        
        private Dictionary<string, GridInformation> _pathToGrid;
        
        private void Awake()
        {
            // 게임씬이 로드 되지 않았다면 return;
            if (TableLoaderManager.Instance == null)
            {
                return;
            }
            // 게임 신 싱글톤으로 사용하기.
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            gameObject.AddComponent<BootstrapperCharacterSpawn>();
            gameObject.AddComponent<BootstrapperMap>();
            wetDecaySystem = gameObject.AddComponent<WetDecaySystem>();
            simulationDirtyTracker = gameObject.AddComponent<SimulationDirtyTracker>();
            
            // Core에 저장 기여자 등록
            simulationSaveContributor = new SimulationSaveContributor(simulationDirtyTracker, this);
            SaveRegistry.Register(simulationSaveContributor);
        }

        private void Start()
        {
            if (SceneGame.Instance)
                SceneGame.Instance.OnSceneGameDestroyed += OnDestroyBySceneGame;
        }

        private void OnDestroyBySceneGame()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (SceneGame.Instance) 
                SceneGame.Instance.OnSceneGameDestroyed -= OnDestroyBySceneGame;
            if (simulationSaveContributor != null)
                SaveRegistry.Unregister(simulationSaveContributor);
        }
        
        // 새 게임 시작 시 호출
        public void ResetAccumulatedSave()
        {
            simulationSaveContributor?.ClearAccumulated();
            SaveRegistry.ClearPendingRestore(); // 선택적
        }
    }
}