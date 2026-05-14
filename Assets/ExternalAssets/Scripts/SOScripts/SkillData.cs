using System;
using UnityEngine;

namespace LLL
{

    [CreateAssetMenu(fileName = "SkillData", menuName = "SO/DataObject/SkillData")]
    [Serializable]
    public class SkillData : ScriptableObject
    {
        public enum SkillType
        {
            None,
            Attack,
            Guard,
            Support,
            Disturbance
        }

        [Flags]
        public enum EffectType
        {
            None = 0,
            Dealing = 1,
            Blocking = 2,
            Healing = 4,
            Buff = 8,
            Debuff = 16,
            CrowdControl = 32
        }

        public enum DamageType
        {
            None,
            Slashing,
            Piercing,
            Bludgeoning,
            Fire,
            Cold,
            Thunder,
            Poison,
            Holy,
            Necro,
        }

        public enum TargetType
        {
            None,
            Front,
            Back,
            Vulnerable,
            Controlled,
            Random
        }

        [field: Header("Skill Data")]
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public SkillType Type { get; private set; }
        [field: SerializeField] public EffectType EffectTags { get; private set; }
        [field: SerializeField] public Damage[] Damages { get; private set; }
        [field: SerializeField] public Heal[] Heals { get; private set; }
        [field: SerializeField] public Block[] Blocks { get; private set; }

        [Serializable]
        public struct Damage
        {
            [field: SerializeField] public DamageType Type { get; private set; }
            [field: SerializeField] public float[] PhysicsFactors { get; private set; }
            [field: SerializeField] public float[] PhysicsCs { get; private set; }
            [field: SerializeField] public float[] MagicFactors { get; private set; }
            [field: SerializeField] public float[] MagicCs { get; private set; }
            [field: SerializeField] public TargetType Target { get; private set; }
            [field: SerializeField] public int[] TargetCount { get; private set; }
            [field: SerializeField] public int[] HitCount { get; private set; }
        }

        [Serializable]
        public struct Block
        {
            [field: SerializeField] public float[] PhysicsFactors { get; private set; }
            [field: SerializeField] public float[] PhysicsCs { get; private set; }
            [field: SerializeField] public float[] MagicFactors { get; private set; }
            [field: SerializeField] public float[] MagicCs { get; private set; }
        }


        [Serializable]
        public struct Heal
        {
            [field: SerializeField] public float[] PhysicsFactors { get; private set; }
            [field: SerializeField] public float[] PhysicsCs { get; private set; }
            [field: SerializeField] public float[] MagicFactors { get; private set; }
            [field: SerializeField] public float[] MagicCs { get; private set; }
            [field: SerializeField] public TargetType Target { get; private set; }
            [field: SerializeField] public int[] TargetCount { get; private set; }
        }


        // To Do : Add After Adding Statistics, and CCs
        [Serializable]
        public struct Buff
        {

        }

        [Serializable]
        public struct Debuff
        {

        }

        [Serializable]
        public struct CrowdControl
        {

        }
    }




}
