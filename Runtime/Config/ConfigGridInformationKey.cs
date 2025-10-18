using System.Collections.Generic;

namespace GGemCo2DSimulation
{
    /// <summary>
    /// GridInformation에 저장시 사용하는 Key 정의
    /// </summary>
    public static class ConfigGridInformationKey
    {
        public const string KeyHoed = "Hoed";
        
        public const string KeySeedItemUid = "SeedItemUid";
        public const string KeySeedStep = "SeedStep";
        
        public const string KeyWet   = "Wet";
        public const string KeyWetUntil   = "WetUntil";
        public const string KeyWetPrevRole = "WetPrevRole";
        public const string KeyWetCount = "WetCount";
        
        public static readonly IReadOnlyList<string> All = new List<string>
        {
            KeyHoed,
            KeySeedItemUid, KeySeedStep,
            KeyWet, KeyWetUntil, KeyWetPrevRole, KeyWetCount,
        };
    }
}