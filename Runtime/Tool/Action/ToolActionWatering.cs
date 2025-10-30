using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.ToolActionWatering.MenuName, order = ConfigScriptableObjectSimulation.ToolActionWatering.Ordering)]
    public class ToolActionWatering : ToolAction
    {
        [Header("Duration")]
        [Tooltip("젖은 상태 유지 시간(초). InGameTimeManager가 있으면 그 시간축을 사용.")]
        [Min(0.1f)] public float wetDurationSeconds = 180f;

        public enum DurationMode { Refresh, Extend }
        [Tooltip("같은 셀을 다시 물 줄 때 처리 방식: Refresh(만료시각 갱신) / Extend(기존에 더해 연장)")]
        public DurationMode durationMode = DurationMode.Refresh;
        
        public override ValidationResult Validate(ToolActionContext ctx)
        {
            var vr = new ValidationResult();
            foreach (var cell in ctx.targetCells)
            {
                bool blocked = ctx.registry.AnyTileAt(cell, ctx.tool.blockRoles);
                bool hasHoed = ctx.registry.AnyTileAt(cell, ctx.tool.readRoles);
                if (!blocked && hasHoed) 
                    vr.ValidCells.Add(cell);
                else                       
                    vr.InvalidCells.Add(cell);
            }
            vr.IsValid = vr.ValidCells.Count > 0 && vr.InvalidCells.Count == 0;
            if (!vr.IsValid) vr.Reason = "Blocked or no hoed.";
            return vr;
        }

        public override void Execute(ToolActionContext ctx)
        {
            if (ctx.defaultTileWet == null) return;

            var info = ctx.gridInformation;
            if (!info)
            {
                Debug.LogWarning("[WaterAction] GridInformation이 필요합니다.", ctx.grid);
                return;
            }

            int now = NowSecondsInt();
            int add = Mathf.CeilToInt(wetDurationSeconds);
            
            foreach (var cell in ctx.targetCells)
            {
                // 1) 이전 역할 기록(갈린 땅 우선, 없으면 기본 땅)
                ConfigCommonSimulation.TileRole prevRole = ctx.registry.AnyTileAt(cell, ConfigCommonSimulation.TileRole.GroundHoed)
                    ? ConfigCommonSimulation.TileRole.GroundHoed
                    : ConfigCommonSimulation.TileRole.GroundBase;

                // 2) 젖은 타일 쓰기
                var writeMap = ctx.registry.ResolveWriteTarget(ConfigCommonSimulation.TileRole.GroundWet, cell);
                if (!writeMap) continue;

                writeMap.SetTile(cell, ctx.defaultTileWet);

                // 3) 만료시각 계산 (Refresh / Extend)
                int existed   = info.GetIntSafe(cell, ConfigGridInformationKey.KeyWetUntil);
                int until = (durationMode == DurationMode.Extend && existed > 0)
                    ? existed + add
                    : now + add;

                // 4) 메타 저장
                info.SetPositionProperty(cell, ConfigGridInformationKey.KeyWet, 1);
                info.SetPositionProperty(cell, ConfigGridInformationKey.KeyWetUntil, until);
                info.SetPositionProperty(cell, ConfigGridInformationKey.KeyWetPrevRole, (int)prevRole);
                
                int countWatering   = info.GetIntSafe(cell, ConfigGridInformationKey.KeyWetCount);
                if (countWatering < 0)
                {
                    countWatering = 1;
                }
                else
                {
                    countWatering++;
                }
                info.SetPositionProperty(cell, ConfigGridInformationKey.KeyWetCount, countWatering);
                GcLogger.Log($"action water: {countWatering}");
                // 5) 디케이 시스템 등록
                var decay = WetDecaySystem.TryGetInstance();
                if (decay != null)
                {
                    decay.Register(writeMap, cell, until, prevRole);
                }
                ctx.dirtyTracker.MarkDirty(info, cell);
            }
        }
        
        private static int NowSecondsInt()
        {
            var t = SceneGame.Instance.gameTimeManager;
            if (t == null)
            {
                GcLogger.LogError($"SceneGame에 GameTimeManager가 없습니다.");
                return 0;
            }
            return Mathf.FloorToInt((float)t.NowSeconds());
        }
    }
}