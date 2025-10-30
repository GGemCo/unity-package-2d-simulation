using System;
using System.Text;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo2DSimulation
{
    public static class GrowthEvaluator
    {
        private static bool CanGrowAt(
            GrowthBase growth,
            GridInformation gi,
            Vector3Int cell,
            int step,
            out string reason)
        {
            var sb = new StringBuilder();

            if (growth == null) { reason = "GrowthBase is null."; return false; }
            if (gi == null)     { reason = "GridInformation is null."; return false; }
            if (step < 0 || step >= growth.struckGrowthConditions.Count)
            {
                reason = $"Invalid step {step}.";
                return false;
            }

            var cond = growth.struckGrowthConditions[step];

            if (cond is { needs: not null })
            {
                foreach (var need in cond.needs)
                {
                    if (!CheckNeedSatisfied(gi, cell, need, out var needMsg))
                        sb.AppendLine(needMsg);
                }
            }

            if (sb.Length > 0)
            {
                reason = sb.ToString().TrimEnd();
                return false;
            }

            reason = null;
            return true;
        }

        public static bool TryFindNextGrowableStep(
            GrowthBase growth,
            GridInformation gi,
            Vector3Int cell,
            int startStepInclusive, // 체크할 다음 단계
            out int nextStep, // 실제 다음 단계
            out string reason)
        {
            nextStep = -1; reason = null;
            if (growth == null || gi == null)
            {
                reason = "Missing refs."; return false;
            }

            if (startStepInclusive >= growth.struckGrowthConditions.Count)
            {
                reason = "최종 단계 입니다.";
                return false;
            }

            if (CanGrowAt(growth, gi, cell, startStepInclusive, out var r))
            {
                nextStep = startStepInclusive;
                return true;
            }
            reason = r;
            return false;
        }
        /// <summary>
        /// 조건을 만족하는 step을 실제로 적용: resultTile 적용, step 값 갱신, dirty 마킹
        /// </summary>
        public static void ApplyStep(
            GrowthBase growth,
            Tilemap writeTilemap,
            GridInformation gi,
            Vector3Int cell,
            int step,
            SimulationDirtyTracker dirtyTracker = null)
        {
            if (growth == null || writeTilemap == null || gi == null) return;
            if (step < 0 || step >= growth.struckGrowthConditions.Count) return;

            var cond = growth.struckGrowthConditions[step];

            // 1) 결과 타일 적용
            if (cond.resultTile != null)
                writeTilemap.SetTile(cell, cond.resultTile);

            // 2) seed step 갱신
            gi.SetPositionProperty(cell, ConfigGridInformationKey.KeySeedStep, step);

            // 3) 더티 마킹
            dirtyTracker?.MarkDirty(gi, cell);
        }
        private static bool CheckNeedSatisfied(GridInformation gi, Vector3Int cell, GrowthNeedEntry need, out string message)
        {
            message = null;
            if (need == null || need.type == GrowthNeedType.None)
                return true;

            int current = 0;
            string label = need.type.ToString();
            
            switch (need.type)
            {
                case GrowthNeedType.Watering:
                    current = gi.GetIntSafe(cell, ConfigGridInformationKey.KeyWetCount, 0);
                    break;

                case GrowthNeedType.Day: // 경과일 체크
                {
                    // 심은 날짜 읽기
                    if (!gi.TryGetDateSafe(cell, ConfigGridInformationKey.KeySeedStartDate, out var startDate))
                    {
                        message = "Seed start date not set.";
                        return false;
                    }

                    // 현재 인게임 날짜 (시각 제외)
                    var timeMgr = SceneGame.Instance?.gameTimeManager;
                    if (timeMgr == null)
                    {
                        message = "GameTimeManager not found.";
                        return false;
                    }

                    DateTime today = timeMgr.Now.Date;
                    int elapsedDays = (today - startDate.Date).Days;
                    current = Mathf.Max(0, elapsedDays); // 음수 방지 (이상치 대비)
                    break;
                }

                default:
                    // 미지원 타입 → 조건 없음으로 처리(필요시 false로 바꾸세요)
                    return true;
            }

            GcLogger.Log($"{label} (current: {current} / needValue:{need.value})");
            if (current < need.value)
            {
                message = $"{label} need not met. (current: {current} / needValue: {need.value})";
                return false;
            }
            return true;
        }
    }
}
