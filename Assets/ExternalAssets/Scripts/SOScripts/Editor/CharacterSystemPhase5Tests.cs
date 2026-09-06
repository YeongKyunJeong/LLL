using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LLL.Editor.Tests
{
    public class CharacterSystemPhase5Tests
    {
        [Test]
        public void DefaultCharacterSystemData_IsValid()
        {
            CharacterSystemDataValidator.ValidationReport report = CharacterSystemDataValidator.ValidateDefaultAssets();

            Assert.That(report.Errors, Is.Empty, string.Join("\n", report.Errors));
        }

        [Test]
        public void FinalStats_ApplyEquipmentFlatBonus()
        {
            CharacterData characterData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/ExternalAssets/SO/CharacterData/SamplePlayerCharacter.asset");
            EquipmentData equipmentData = AssetDatabase.LoadAssetAtPath<EquipmentData>("Assets/ExternalAssets/SO/EquipmentData/SampleEquipment.asset");
            CharacterLevelStats levelStats = new CharacterLevelStats
            {
                level = 1,
                maxHp = 100,
                physicalPower = 10,
                physicalDefense = 5,
                magicPower = 8,
                magicDefense = 4
            };

            CharacterRuntimeData runtimeData = new CharacterRuntimeData(characterData, levelStats, new[] { equipmentData });

            Assert.That(runtimeData.FinalStats.PhysicalPower, Is.EqualTo(15));
        }

        [Test]
        public void DamageCalculation_UsesStatsAndAffinity()
        {
            CharacterData attackerData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/ExternalAssets/SO/CharacterData/SamplePlayerCharacter.asset");
            CharacterData targetData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/ExternalAssets/SO/CharacterData/SampleEnemyCharacter.asset");
            EquipmentData equipmentData = AssetDatabase.LoadAssetAtPath<EquipmentData>("Assets/ExternalAssets/SO/EquipmentData/SampleEquipment.asset");
            CharacterLevelStats attackerStats = new CharacterLevelStats
            {
                level = 1,
                maxHp = 100,
                physicalPower = 10,
                physicalDefense = 5,
                magicPower = 8,
                magicDefense = 4
            };
            CharacterLevelStats targetStats = new CharacterLevelStats
            {
                level = 1,
                maxHp = 80,
                physicalPower = 8,
                physicalDefense = 3,
                magicPower = 4,
                magicDefense = 2
            };

            CharacterRuntimeData attackerRuntime = new CharacterRuntimeData(attackerData, attackerStats, new[] { equipmentData });
            CharacterRuntimeData targetRuntime = new CharacterRuntimeData(targetData, targetStats);
            BattleController.BattleCombatant attacker = new BattleController.BattleCombatant();
            BattleController.BattleCombatant target = new BattleController.BattleCombatant();
            attacker.Initialize(attackerRuntime, "Attacker");
            target.Initialize(targetRuntime, "Target");
            SkillData.Damage damage = CreateDamage(1f, 0f, 0f, 0f, 1, 1);

            int result = BattleCalculationUtility.CalculateDamage(attacker, target, damage, 1);

            Assert.That(result, Is.GreaterThan(10));
        }

        private static SkillData.Damage CreateDamage(float physicalFactor, float physicalConstant, float magicFactor, float magicConstant, int targetCount, int hitCount)
        {
            DamageFactory factory = ScriptableObject.CreateInstance<DamageFactory>();
            SerializedObject serialized = new SerializedObject(factory);
            SerializedProperty property = serialized.FindProperty("damage");
            property.FindPropertyRelative("<PhysicsFactors>k__BackingField").arraySize = 1;
            property.FindPropertyRelative("<PhysicsFactors>k__BackingField").GetArrayElementAtIndex(0).floatValue = physicalFactor;
            property.FindPropertyRelative("<PhysicsCs>k__BackingField").arraySize = 1;
            property.FindPropertyRelative("<PhysicsCs>k__BackingField").GetArrayElementAtIndex(0).floatValue = physicalConstant;
            property.FindPropertyRelative("<MagicFactors>k__BackingField").arraySize = 1;
            property.FindPropertyRelative("<MagicFactors>k__BackingField").GetArrayElementAtIndex(0).floatValue = magicFactor;
            property.FindPropertyRelative("<MagicCs>k__BackingField").arraySize = 1;
            property.FindPropertyRelative("<MagicCs>k__BackingField").GetArrayElementAtIndex(0).floatValue = magicConstant;
            property.FindPropertyRelative("<TargetCount>k__BackingField").arraySize = 1;
            property.FindPropertyRelative("<TargetCount>k__BackingField").GetArrayElementAtIndex(0).intValue = targetCount;
            property.FindPropertyRelative("<HitCount>k__BackingField").arraySize = 1;
            property.FindPropertyRelative("<HitCount>k__BackingField").GetArrayElementAtIndex(0).intValue = hitCount;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            SkillData.Damage damage = factory.damage;
            Object.DestroyImmediate(factory);
            return damage;
        }

        private class DamageFactory : ScriptableObject
        {
            public SkillData.Damage damage;
        }
    }
}
