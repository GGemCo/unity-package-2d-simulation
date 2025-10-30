using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        public const string KeySeedStartDate = "SeedStartDate";
        
        public const string KeyWet   = "Wet";
        public const string KeyWetUntil   = "WetUntil";
        public const string KeyWetPrevRole = "WetPrevRole";
        public const string KeyWetCount = "WetCount";
        
        public static readonly IReadOnlyList<string> All = new List<string>
        {
            KeyHoed,
            KeySeedItemUid, KeySeedStep, KeySeedStartDate,
            KeyWet, KeyWetUntil, KeyWetPrevRole, KeyWetCount,
        };
        
        public enum TypeHint { Bool, Int, Float, String, Vector3Int, Unknown }

        public static TypeHint GetTypeHint(string key)
        {
            switch (key)
            {
                case KeyHoed:           return TypeHint.Bool;
        
                case KeyWet:            return TypeHint.Bool;
                case KeyWetUntil:       return TypeHint.Int;
                case KeyWetPrevRole:    return TypeHint.Int;
                case KeyWetCount:       return TypeHint.Int;
                
                case KeySeedItemUid:    return TypeHint.Int;
                case KeySeedStep:       return TypeHint.Int;
                case KeySeedStartDate:  return TypeHint.String;
                default:                return TypeHint.Unknown;
            }
        }
        
        /// <summary>
        /// GridInformation의 형식별 오버로드를 사용해 값을 읽고, 누적용 KV를 만든다.
        /// - bool → int(0/1)로 저장/복원
        /// - Vector3Int → string("x,y,z")로 저장/복원
        /// </summary>
        public static bool TryReadWithGet(GridInformation gi, Vector3Int cell, string key, out GridInfoKV kv)
        {
            kv = default;

            switch (GetTypeHint(key))
            {
                case TypeHint.Bool:
                {
                    int raw = gi.GetPositionProperty(cell, key, 0);
                    bool v = raw != 0;
                    kv = new GridInfoKV { key = key, type = "bool", value = v.ToString() };
                    return true;
                }
                case TypeHint.Int:
                {
                    int v = gi.GetPositionProperty(cell, key, -1);
                    kv = new GridInfoKV { key = key, type = "int", value = v.ToString() };
                    return true;
                }
                case TypeHint.Float:
                {
                    float v = gi.GetPositionProperty(cell, key, -1f);
                    kv = new GridInfoKV { key = key, type = "float", value = v.ToString("R") };
                    return true;
                }
                case TypeHint.String:
                {
                    string v = gi.GetPositionProperty(cell, key, string.Empty);
                    kv = new GridInfoKV { key = key, type = "string", value = v ?? string.Empty };
                    return true;
                }
                case TypeHint.Vector3Int:
                {
                    // Vector3Int 오버로드 없음 → string 보관
                    string s = gi.GetPositionProperty(cell, key, string.Empty);
                    if (string.IsNullOrEmpty(s)) s = "0,0,0";
                    kv = new GridInfoKV { key = key, type = "Vector3Int", value = s };
                    return true;
                }
                default:
                    return false;
            }
        }
    }
}