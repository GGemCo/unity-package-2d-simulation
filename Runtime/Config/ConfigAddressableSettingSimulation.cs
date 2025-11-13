using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DSimulation
{
    public static class ConfigAddressableSettingSimulation
    {
        public static readonly AddressableAssetInfo SimulationSettings = ConfigAddressableSetting.Make(nameof(SimulationSettings));
        
        /// <summary>
        /// 로딩 씬에서 로드해야 하는 리스트
        /// </summary>
        public static readonly List<AddressableAssetInfo> NeedLoadInLoadingScene = new()
        {
            SimulationSettings,
        };
    }
}