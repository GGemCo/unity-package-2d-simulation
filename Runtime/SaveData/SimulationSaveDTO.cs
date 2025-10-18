using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DSimulation
{
    [Serializable]
    public struct GridInfoKV
    {
        public string key;   // 예: "Wet","Fertilized","GrowthStage"
        public string type;  // "bool","int","float","string","Vector3Int"
        public string value; // 문자열 직렬화 값
    }

    [Serializable]
    public struct CellInfo
    {
        public Vector3Int cell;
        public List<GridInfoKV> entries;
    }

    [Serializable]
    public class GridInfoSnapshot
    {
        public string gridPath;    // GridInformation가 붙은 GameObject의 계층 경로
        public List<CellInfo> cells;
    }

    [Serializable]
    public class SimulationSaveDTO
    {
        public int version;
        public List<GridInfoSnapshot> grids;
    }
}