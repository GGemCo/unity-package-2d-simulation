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
        public const string NameBase = ConfigDefine.NameSDK + "_Simulation";
        
        private const string PathBase = ConfigDefine.NameSDK + "/Simulation";
        private const string PathToolAction = PathBase + "/Tool Action";
        private const string PathToolTargeting = PathBase + "/Tool Targeting";
        
        /// <summary>
        /// 메뉴 순서 정의
        /// </summary>
        public enum MenuOrdering
        {
            None,
            ToolDefinition,
            ToolActionAxe,
            ToolActionHoe,
            ToolActionWatering,
            ToolTargetingRect,
            ToolTargetingLine,
            ToolTargetingCrossArea,
            ToolTargetingSingleCell,
            ToolActionSeed,
            TilemapRoleRule,
            Growth,
            ToolActionPickax,
            ToolActionSickle,
            ToolActionHandHarvest
        }
        
        public static class SimulationSettings
        {
            public const string FileName = ConfigScriptableObject.BaseName + "SimulationSettings";
            public const string MenuName = ConfigScriptableObject.BasePath + FileName;
            public const int Ordering = (int)ConfigScriptableObject.MenuOrdering.SimulationSettings;
        }
        
        public static class ToolDefinition
        {
            public const string MenuName = PathBase + "/ToolDefinition";
            public const int Ordering = (int)MenuOrdering.ToolDefinition;
        }
        public static class ToolActionAxe
        {
            public const string MenuName = PathToolAction + "/Axe";
            public const int Ordering = (int)MenuOrdering.ToolActionAxe;
        }
        public static class ToolActionHoe
        {
            public const string MenuName = PathToolAction + "/Hoe";
            public const int Ordering = (int)MenuOrdering.ToolActionHoe;
        }
        public static class ToolActionWatering
        {
            public const string MenuName = PathToolAction + "/Watering";
            public const int Ordering = (int)MenuOrdering.ToolActionWatering;
        }
        public static class ToolActionSeed
        {
            public const string MenuName = PathToolAction + "/Seed";
            public const int Ordering = (int)MenuOrdering.ToolActionSeed;
        }
        public static class ToolActionPickAxe
        {
            public const string MenuName = PathToolAction + "/PickAxe";
            public const int Ordering = (int)MenuOrdering.ToolActionPickax;
        }
        public static class ToolActionSickle
        {
            public const string MenuName = PathToolAction + "/Sickle";
            public const int Ordering = (int)MenuOrdering.ToolActionSickle;
        }

        public static class ToolActionHandHarvest
        {
            public const string MenuName = PathToolAction + "/HandHarvest";
            public const int Ordering = (int)MenuOrdering.ToolActionHandHarvest;
        }
        public static class ToolTargetingRect
        {
            public const string MenuName = PathToolTargeting + "/Rect Area";
            public const int Ordering = (int)MenuOrdering.ToolTargetingRect;
        }
        public static class ToolTargetingLine
        {
            public const string MenuName = PathToolTargeting + "/Line";
            public const int Ordering = (int)MenuOrdering.ToolTargetingLine;
        }
        public static class ToolTargetingCrossArea
        {
            public const string MenuName = PathToolTargeting + "/Cross Area";
            public const int Ordering = (int)MenuOrdering.ToolTargetingCrossArea;
        }
        public static class ToolTargetingSingleCell
        {
            public const string MenuName = PathToolTargeting + "/Single Cell";
            public const int Ordering = (int)MenuOrdering.ToolTargetingSingleCell;
        }
        public static class TilemapRoleRule
        {
            public const string MenuName = PathBase + "/Tilemap Role Rule";
            public const int Ordering = (int)MenuOrdering.TilemapRoleRule;
        }
        public static class Growth
        {
            public const string MenuName = PathBase + "/Growth";
            public const int Ordering = (int)MenuOrdering.Growth;
        }

        public static readonly Dictionary<string, Type> SettingsTypes = new()
        {
            { SimulationSettings.FileName, typeof(GGemCoSimulationSettings) },
        };
    }
}