using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    public static class GridInformationExtensions
    {
        public static int GetIntSafe(this GridInformation gi, Vector3Int cell, string key, int defaultValue = -1)
        {
            if (ConfigGridInformationKey.TryReadWithGet(gi, cell, key, out var kv))
                if (int.TryParse(kv.value, out var result))
                    return result;
            return defaultValue;
        }

        public static string GetStringSafe(this GridInformation gi, Vector3Int cell, string key, string defaultValue = "")
        {
            if (ConfigGridInformationKey.TryReadWithGet(gi, cell, key, out var kv))
                return kv.value ?? defaultValue;
            return defaultValue;
        }
        /// <summary>
        /// "yyyy-MM-dd"를 안전하게 DateTime으로 변환
        /// </summary>
        /// <param name="gi"></param>
        /// <param name="cell"></param>
        /// <param name="key"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool TryGetDateSafe(this GridInformation gi, Vector3Int cell, string key, out DateTime date)
        {
            date = default;
            if (!ConfigGridInformationKey.TryReadWithGet(gi, cell, key, out var kv) || string.IsNullOrEmpty(kv.value))
                return false;

            return DateTime.TryParseExact(
                kv.value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date
            );
        }
    }
}