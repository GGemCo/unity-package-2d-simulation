namespace GGemCo2DSimulation
{
    [System.Flags]
    public enum TileRole
    {
        None          = 0,
        GroundBase    = 1 << 0,   // 기본 바닥
        GroundHoed    = 1 << 1,   // 경작 상태
        GroundWet     = 1 << 2,   // 젖은 상태
        GroundGrowth  = 1 << 3,   // 씨앗 심은 상태
        Blocking      = 1 << 4,   // 충돌/점유 레이어(나무/바위/집/오브젝트)
        Decor         = 1 << 5,   // 장식(논리 무관)
        Empty         = 1 << 6,   // 안보이는 비어있는 상태  
        Preview       = 1 << 7,   // 미리보기 전용(런타임 생성/캐싱)
        AnyGround     = GroundBase | GroundHoed | GroundWet,
    }
}