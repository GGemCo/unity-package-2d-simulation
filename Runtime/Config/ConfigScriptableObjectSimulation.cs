using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// ScriptableObject 관련 설정 정의
    /// </summary>
    public static class ConfigScriptableObjectSimulation
    {
        public static class SimulationSettings
        {
            public const string FileName = ConfigScriptableObject.BaseName + "SimulationSettings";
            public const string MenuName = ConfigScriptableObject.BasePath + FileName;
            public const int Ordering = (int)ConfigScriptableObject.MenuOrdering.SimulationSettings;
        }

        public static readonly Dictionary<string, Type> SettingsTypes = new()
        {
            { SimulationSettings.FileName, typeof(GGemCoSimulationSettings) },
        };
    }
}