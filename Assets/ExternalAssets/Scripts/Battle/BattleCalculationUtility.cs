using UnityEngine;

namespace LLL
{
    public static class BattleCalculationUtility
    {
        public static int CalculateDamage(
            BattleController.BattleCombatant attacker,
            BattleController.BattleCombatant target,
            SkillData.Damage damage,
            int stage)
        {
            if (attacker == null || target == null)
            {
                return 0;
            }

            float physicalBase = attacker.FinalStats.PhysicalPower * GetStageFloat(damage.PhysicsFactors, stage) + GetStageFloat(damage.PhysicsCs, stage);
            float magicBase = attacker.FinalStats.MagicPower * GetStageFloat(damage.MagicFactors, stage) + GetStageFloat(damage.MagicCs, stage);
            float rawDamage = Mathf.Max(0f, physicalBase + magicBase);
            float physicalWeight = rawDamage > 0f ? physicalBase / rawDamage : 0.5f;
            float magicWeight = rawDamage > 0f ? magicBase / rawDamage : 0.5f;
            float defense = target.FinalStats.PhysicalDefense * physicalWeight + target.FinalStats.MagicDefense * magicWeight;
            float defenseReduction = Mathf.Clamp(defense / (defense + 100f), 0f, 0.8f);
            float affinityMultiplier = GetAffinityDamageMultiplier(attacker.RuntimeData, target.RuntimeData);
            float synergyMultiplier = GetEquipmentEffectMultiplier(attacker.RuntimeData, target.RuntimeData, damage.Type, EquipmentData.EffectTrigger.OutgoingDamage)
                * GetEquipmentEffectMultiplier(target.RuntimeData, attacker.RuntimeData, damage.Type, EquipmentData.EffectTrigger.IncomingDamage);
            int flatBonus = GetEquipmentEffectFlatBonus(attacker.RuntimeData, target.RuntimeData, damage.Type, EquipmentData.EffectTrigger.OutgoingDamage)
                + GetEquipmentEffectFlatBonus(target.RuntimeData, attacker.RuntimeData, damage.Type, EquipmentData.EffectTrigger.IncomingDamage);

            float result = rawDamage * (1f - defenseReduction) * affinityMultiplier * synergyMultiplier + flatBonus;
            return Mathf.Max(1, Mathf.RoundToInt(result));
        }

        public static int CalculateHeal(BattleController.BattleCombatant caster, BattleController.BattleCombatant target, SkillData.Heal heal, int stage)
        {
            if (caster == null || target == null)
            {
                return 0;
            }

            float amount = caster.FinalStats.PhysicalPower * GetStageFloat(heal.PhysicsFactors, stage)
                + GetStageFloat(heal.PhysicsCs, stage)
                + caster.FinalStats.MagicPower * GetStageFloat(heal.MagicFactors, stage)
                + GetStageFloat(heal.MagicCs, stage);
            amount *= GetEquipmentEffectMultiplier(caster.RuntimeData, target.RuntimeData, SkillData.DamageType.None, EquipmentData.EffectTrigger.Healing);
            amount += GetEquipmentEffectFlatBonus(caster.RuntimeData, target.RuntimeData, SkillData.DamageType.None, EquipmentData.EffectTrigger.Healing);

            return Mathf.Max(1, Mathf.RoundToInt(amount));
        }

        public static int CalculateBlock(BattleController.BattleCombatant caster, SkillData.Block block, int stage)
        {
            if (caster == null)
            {
                return 0;
            }

            float amount = caster.FinalStats.PhysicalPower * GetStageFloat(block.PhysicsFactors, stage)
                + GetStageFloat(block.PhysicsCs, stage)
                + caster.FinalStats.MagicPower * GetStageFloat(block.MagicFactors, stage)
                + GetStageFloat(block.MagicCs, stage);
            amount *= GetEquipmentEffectMultiplier(caster.RuntimeData, caster.RuntimeData, SkillData.DamageType.None, EquipmentData.EffectTrigger.Blocking);
            amount += GetEquipmentEffectFlatBonus(caster.RuntimeData, caster.RuntimeData, SkillData.DamageType.None, EquipmentData.EffectTrigger.Blocking);

            return Mathf.Max(1, Mathf.RoundToInt(amount));
        }

        private static float GetAffinityDamageMultiplier(CharacterRuntimeData attacker, CharacterRuntimeData target)
        {
            CharacterData.AffinityClass attackerAffinity = attacker != null && attacker.CharacterData != null ? attacker.CharacterData.Affinity : CharacterData.AffinityClass.None;
            CharacterData.AffinityClass targetAffinity = target != null && target.CharacterData != null ? target.CharacterData.Affinity : CharacterData.AffinityClass.None;

            if (attackerAffinity == CharacterData.AffinityClass.None || targetAffinity == CharacterData.AffinityClass.None)
            {
                return 1f;
            }

            if ((attackerAffinity == CharacterData.AffinityClass.A && targetAffinity == CharacterData.AffinityClass.B)
                || (attackerAffinity == CharacterData.AffinityClass.B && targetAffinity == CharacterData.AffinityClass.C)
                || (attackerAffinity == CharacterData.AffinityClass.C && targetAffinity == CharacterData.AffinityClass.A))
            {
                return 1.25f;
            }

            if ((attackerAffinity == CharacterData.AffinityClass.B && targetAffinity == CharacterData.AffinityClass.A)
                || (attackerAffinity == CharacterData.AffinityClass.C && targetAffinity == CharacterData.AffinityClass.B)
                || (attackerAffinity == CharacterData.AffinityClass.A && targetAffinity == CharacterData.AffinityClass.C))
            {
                return 0.85f;
            }

            if (attackerAffinity == CharacterData.AffinityClass.D && targetAffinity == CharacterData.AffinityClass.E)
            {
                return 1.5f;
            }

            if (attackerAffinity == CharacterData.AffinityClass.E
                && (targetAffinity == CharacterData.AffinityClass.A || targetAffinity == CharacterData.AffinityClass.B || targetAffinity == CharacterData.AffinityClass.C))
            {
                return 1.1f;
            }

            return 1f;
        }

        private static float GetEquipmentEffectMultiplier(CharacterRuntimeData owner, CharacterRuntimeData target, SkillData.DamageType damageType, EquipmentData.EffectTrigger trigger)
        {
            float multiplier = 1f;
            EquipmentData[] equipment = owner != null ? owner.EquippedItems : null;
            if (equipment == null)
            {
                return multiplier;
            }

            for (int i = 0; i < equipment.Length; i++)
            {
                EquipmentData.ConditionalEquipmentEffect[] effects = equipment[i] != null ? equipment[i].ConditionalEffects : null;
                if (effects == null) continue;

                for (int j = 0; j < effects.Length; j++)
                {
                    if (IsEffectMatch(effects[j], target, damageType, trigger))
                    {
                        multiplier *= Mathf.Max(0f, 1f + effects[j].MultiplierBonus);
                    }
                }
            }

            return multiplier;
        }

        private static int GetEquipmentEffectFlatBonus(CharacterRuntimeData owner, CharacterRuntimeData target, SkillData.DamageType damageType, EquipmentData.EffectTrigger trigger)
        {
            int flatBonus = 0;
            EquipmentData[] equipment = owner != null ? owner.EquippedItems : null;
            if (equipment == null)
            {
                return flatBonus;
            }

            for (int i = 0; i < equipment.Length; i++)
            {
                EquipmentData.ConditionalEquipmentEffect[] effects = equipment[i] != null ? equipment[i].ConditionalEffects : null;
                if (effects == null) continue;

                for (int j = 0; j < effects.Length; j++)
                {
                    if (IsEffectMatch(effects[j], target, damageType, trigger))
                    {
                        flatBonus += effects[j].FlatBonus;
                    }
                }
            }

            return flatBonus;
        }

        private static bool IsEffectMatch(EquipmentData.ConditionalEquipmentEffect effect, CharacterRuntimeData target, SkillData.DamageType damageType, EquipmentData.EffectTrigger trigger)
        {
            if (effect.Trigger != trigger)
            {
                return false;
            }

            if (effect.DamageType != SkillData.DamageType.None && effect.DamageType != damageType)
            {
                return false;
            }

            CharacterData targetData = target != null ? target.CharacterData : null;
            if (targetData == null)
            {
                return true;
            }

            if (effect.TargetAffinity != CharacterData.AffinityClass.None && effect.TargetAffinity != targetData.Affinity)
            {
                return false;
            }

            if (effect.TargetPlayerBaseClass != CharacterData.PlayerBaseClass.None && effect.TargetPlayerBaseClass != targetData.BaseClass)
            {
                return false;
            }

            if (effect.TargetPlayerSubClass != CharacterData.PlayerSubClass.None && effect.TargetPlayerSubClass != targetData.SubClass)
            {
                return false;
            }

            if (effect.TargetEnemyClass != CharacterData.EnemyClass.None && effect.TargetEnemyClass != targetData.EnemyCharacterClass)
            {
                return false;
            }

            return true;
        }

        private static float GetStageFloat(float[] values, int stage)
        {
            if (values == null || values.Length == 0)
            {
                return 0f;
            }

            int index = Mathf.Clamp(stage - 1, 0, values.Length - 1);
            return values[index];
        }
    }
}
