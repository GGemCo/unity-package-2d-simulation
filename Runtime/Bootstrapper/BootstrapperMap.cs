using GGemCo2DControl;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// Core의 캐릭터 생성 이벤트를 구독하여 ControlBase 를 자동 부착
    /// </summary>
    [DefaultExecutionOrder((int)ConfigCommonSimulation.ExecutionOrdering.Simulation)]
    public class BootstrapperMap : MonoBehaviour
    {
        [SerializeField] private bool addIfMissing = true;

        private void OnEnable()
        {
            MapManager.OnLoadCompleteMap   += OnMapLoadComplete;
        }

        private void OnDisable()
        {
            MapManager.OnLoadCompleteMap   -= OnMapLoadComplete;
        }
        private void OnMapLoadComplete(MapTileCommon mapTileCommon, GameObject gridTileMap)
        {
            var autoTilemapRegistry = gridTileMap.GetComponent<AutoTilemapRegistry>();
            if (autoTilemapRegistry == null)
                gridTileMap.AddComponent<AutoTilemapRegistry>();
        }
    }
}