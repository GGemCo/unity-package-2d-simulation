using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// Control 패키지의 메인 메니저
    /// SceneGame 과 같은 개념
    /// </summary>
    public class SimulationPackageManager : MonoBehaviour
    {
        public static SimulationPackageManager Instance { get; private set; }
        
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
            // gameObject.AddComponent<BootstrapperMap>();
        }

    }
}