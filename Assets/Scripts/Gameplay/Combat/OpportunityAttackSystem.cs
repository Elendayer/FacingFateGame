using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public static class OpportunityAttackSystem
    {
        // Called after every NavMesh walk movement. Enqueues OA as priority reactions.
        public static void CheckAndFireOA(EntityScript movingEntity, Vector3 fromPos, Vector3 toPos)
        {
            if (movingEntity == null || movingEntity.isDead) return;
            if (movingEntity.HasModifier("Disengaged")) return;

            List<EntityScript> attackers = GetOAAttackers(movingEntity, fromPos, toPos);
            foreach (EntityScript attacker in attackers)
            {
                EntityScript capturedAttacker = attacker;
                EntityScript capturedTarget = movingEntity;

                ActionQueueUtility.EnqueuePriorityAction(() =>
                {
                    if (capturedAttacker == null || capturedAttacker.isDead) return;
                    if (capturedTarget == null || capturedTarget.isDead) return;

                    int damage = Mathf.RoundToInt(capturedAttacker.entityStats.CurrentStrength);

                    capturedTarget.entityStats.CurrentHealth -= damage;

                    DamageNumberSpawner.Instance?.SpawnDamage(
                        capturedTarget,
                        damage,
                        DamageNumberSpawner.NumberType.OpportunityAttack);

                    capturedTarget.entityStats.UpdateStats();
                    CombatUIController.Instance?.RefreshAll();

                }, 0f, capturedAttacker);
            }
        }

        // Returns all entities that would get an OA opportunity against movingEntity.
        public static List<EntityScript> GetOAAttackers(EntityScript movingEntity, Vector3 fromPos, Vector3 toPos)
        {
            var attackers = new List<EntityScript>();

            if (TurnManager.Instance == null) return attackers;

            foreach (EntityScript entity in TurnManager.Instance.TurnOrder)
            {
                if (entity == null || entity == movingEntity) continue;
                if (entity.isDead) continue;
                if (!AreOpponents(entity, movingEntity)) continue;
                if (entity.entityStats.IsStunned) continue;

                float range = entity.entityStats.opportunityAttackRange;
                bool wasInRange = Vector3.Distance(entity.transform.position, fromPos) <= range;
                bool isNowInRange = Vector3.Distance(entity.transform.position, toPos) <= range;

                if (wasInRange && !isNowInRange)
                    attackers.Add(entity);
            }

            return attackers;
        }

        // Returns total estimated OA damage for a hypothetical move. Used by AI.
        public static int EstimateOADamage(EntityScript movingEntity, Vector3 fromPos, Vector3 toPos)
        {
            if (movingEntity == null) return 0;
            if (movingEntity.HasModifier("Disengaged")) return 0;

            int total = 0;
            foreach (EntityScript attacker in GetOAAttackers(movingEntity, fromPos, toPos))
                total += Mathf.RoundToInt(attacker.entityStats.CurrentStrength);

            return total;
        }

        private static bool AreOpponents(EntityScript a, EntityScript b)
        {
            return (a.entityAffiliation == EntityAffiliation.Player && b.entityAffiliation == EntityAffiliation.Enemy)
                || (a.entityAffiliation == EntityAffiliation.Enemy && b.entityAffiliation == EntityAffiliation.Player);
        }
    }
}
