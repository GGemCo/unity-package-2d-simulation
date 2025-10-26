using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DSimulation
{
    [CreateAssetMenu(menuName = ConfigScriptableObjectSimulation.ToolActionPickAxe.MenuName, order = ConfigScriptableObjectSimulation.ToolActionPickAxe.Ordering)]
    public class ToolActionPickAxe : ToolAction
    {
        private Collider2D[] _collider2Ds = new Collider2D[10];
        
        /// <summary>
        /// Tool definition에서 먼저 range, metric에 해당하는 셀일 경우 호출 된다
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override ValidationResult Validate(ToolActionContext ctx)
        {
            var vr = new ValidationResult();
            var user = ctx.user;
            var player = user.GetComponent<Player>();
            var colliderAttackRange = player.colliderAttackRange;
            var transform = user;
            
            foreach (var cell in ctx.targetCells)
            {
                
                bool blocked = ctx.registry.AnyTileAt(cell, ctx.tool.blockRoles);
                bool hasGround = ctx.registry.AnyTileAt(cell, ctx.tool.readRoles);

                if (!blocked && hasGround)
                {
                    Vector2 size = new Vector2(16, 16);
                    // Vector2 point = (Vector2)transform.position + colliderAttackRange.offset * transform.localScale;
                    Vector2 point = ctx.grid.GetCellCenterWorld(cell);

                    int found = 0;
#if UNITY_6000_0_OR_NEWER
                    int hitCount = Physics2D.OverlapCapsule(point, size, colliderAttackRange.direction, 0f, new ContactFilter2D().NoFilter(), _collider2Ds);
                    // GcLogger.Log($"hitCount {hitCount}");
                    for (int i = 0; i < hitCount; i++)
                    {
                        Collider2D hit = _collider2Ds[i];
#else
                    Physics2D.OverlapCapsuleNonAlloc(point, size, colliderCheckCharacter.direction, 0f, _collider2Ds);
                    foreach (var hit in _collider2Ds)
                    {
#endif
                        if (!hit || !hit.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Npc))) continue;
                        CharacterHitArea characterHitArea = hit.GetComponent<CharacterHitArea>();
                        if (characterHitArea == null) continue;
                    
                        var npc = characterHitArea.target as Npc;
                        if (npc == null || !npc.IsSubCategoryOre()) continue;
                        // GcLogger.Log($"npc {npc.name}");
                        found++;
                        break;
                    }

                    if (found > 0)
                    {
                        vr.ValidCells.Add(cell);
                    }
                    else
                    {
                        vr.InvalidCells.Add(cell);
                    }
                }
                else
                {
                    vr.InvalidCells.Add(cell);
                }
            }

            vr.IsValid = vr.ValidCells.Count > 0 && vr.InvalidCells.Count == 0;
            if (!vr.IsValid) vr.Reason = "Blocked or no ground.";
            return vr;
        }

        public override void Execute(ToolActionContext ctx)
        {
            var info = ctx.gridInformation;
            if (!info)
            {
                Debug.LogWarning("[WaterAction] GridInformation이 필요합니다.", ctx.grid);
                return;
            }
            var user = ctx.user;
            var player = user.GetComponent<Player>();
            var colliderAttackRange = player.colliderAttackRange;
            var transform = user;

            Npc npc = null;
            foreach (var cell in ctx.targetCells)
            {
                bool blocked = ctx.registry.AnyTileAt(cell, ctx.tool.blockRoles);
                bool hasGround = ctx.registry.AnyTileAt(cell, ctx.tool.readRoles);

                if (!blocked && hasGround)
                {
                    Vector2 size = new Vector2(16, 16);
                    // Vector2 point = (Vector2)transform.position + colliderAttackRange.offset * transform.localScale;
                    Vector2 point = ctx.grid.GetCellCenterWorld(cell);

#if UNITY_6000_0_OR_NEWER
                    int hitCount = Physics2D.OverlapCapsule(point, size, colliderAttackRange.direction, 0f, new ContactFilter2D().NoFilter(), _collider2Ds);
                    // GcLogger.Log($"hitCount {hitCount}");
                    for (int i = 0; i < hitCount; i++)
                    {
                        Collider2D hit = _collider2Ds[i];
#else
                    Physics2D.OverlapCapsuleNonAlloc(point, size, colliderCheckCharacter.direction, 0f, _collider2Ds);
                    foreach (var hit in _collider2Ds)
                    {
#endif
                        if (!hit || !hit.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Npc))) continue;
                        CharacterHitArea characterHitArea = hit.GetComponent<CharacterHitArea>();
                        if (characterHitArea == null) continue;
                    
                        npc = characterHitArea.target as Npc;
                        if (npc == null || !npc.IsSubCategoryOre()) continue;
                        // GcLogger.Log($"npc {npc.name}");
                        break;
                    }
                }
            }

            if (npc == null) return;
            MetadataDamage metadataDamage = new MetadataDamage
            {
                damageType = SkillConstants.DamageType.Physic,
                damage = 1,
                attacker = user.gameObject
            };
            npc.TakeDamage(metadataDamage);
        }
    }
}