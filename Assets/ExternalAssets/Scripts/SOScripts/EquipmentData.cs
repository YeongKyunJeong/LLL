using System;
using UnityEngine;

namespace LLL
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "SO/DataObject/EquipmentData")]
    public class EquipmentData : ScriptableObject
    {
        public enum EquipmentSlot
        {
            None,
            Weapon,
            Armor,
            Accessory1,
            Accessory2
        }

        public enum EffectTrigger
        {
            None,
            OutgoingDamage,
            IncomingDamage,
            Healing,
            Blocking
        }

        [field: Header("Identity")]
        [field: SerializeField] public string EquipmentId { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public EquipmentSlot Slot { get; private set; }

        [field: Header("Flat Stat Bonus")]
        [field: SerializeField] public CharacterStatBonus FlatBonus { get; private set; }

        [field: Header("Stat Multiplier")]
        [field: SerializeField] public CharacterStatMultiplier StatMultiplier { get; private set; } = CharacterStatMultiplier.Identity;

        [field: Header("Conditional Effects")]
        [field: SerializeField] public ConditionalEquipmentEffect[] ConditionalEffects { get; private set; }

        [Serializable]
        public struct CharacterStatBonus
        {
            [field: SerializeField] public int MaxHp { get; private set; }
            [field: SerializeField] public int PhysicalPower { get; private set; }
            [field: SerializeField] public int PhysicalDefense { get; private set; }
            [field: SerializeField] public int MagicPower { get; private set; }
            [field: SerializeField] public int MagicDefense { get; private set; }
        }

        [Serializable]
        public struct CharacterStatMultiplier
        {
            [field: SerializeField] public float MaxHp { get; private set; }
            [field: SerializeField] public float PhysicalPower { get; private set; }
            [field: SerializeField] public float PhysicalDefense { get; private set; }
            [field: SerializeField] public float MagicPower { get; private set; }
            [field: SerializeField] public float MagicDefense { get; private set; }

            public static CharacterStatMultiplier Identity => new CharacterStatMultiplier(1f, 1f, 1f, 1f, 1f);

            public CharacterStatMultiplier(float maxHp, float physicalPower, float physicalDefense, float magicPower, float magicDefense)
            {
                MaxHp = maxHp;
                PhysicalPower = physicalPower;
                PhysicalDefense = physicalDefense;
                MagicPower = magicPower;
                MagicDefense = magicDefense;
            }
        }

        [Serializable]
        public struct ConditionalEquipmentEffect
        {
            [field: SerializeField] public EffectTrigger Trigger { get; private set; }
            [field: SerializeField] public SkillData.DamageType DamageType { get; private set; }
            [field: SerializeField] public CharacterData.AffinityClass TargetAffinity { get; private set; }
            [field: SerializeField] public CharacterData.PlayerBaseClass TargetPlayerBaseClass { get; private set; }
            [field: SerializeField] public CharacterData.PlayerSubClass TargetPlayerSubClass { get; private set; }
            [field: SerializeField] public CharacterData.EnemyClass TargetEnemyClass { get; private set; }
            [field: SerializeField] public float MultiplierBonus { get; private set; }
            [field: SerializeField] public int FlatBonus { get; private set; }
        }
    }
}
