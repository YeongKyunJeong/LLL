using UnityEngine;

namespace LLL
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "SO/DataObject/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        public enum CharacterType
        {
            None,
            Player,
            Enemy,
            Boss
        }

        public enum AffinityClass
        {
            None,
            A,
            B,
            C,
            D,
            E
        }

        public enum PlayerBaseClass
        {
            None,
            Warrior,
            Priest,
            Mage,
            Ranger,
            Rogue
        }

        public enum PlayerSubClass
        {
            None,
            Knight,
            Cleric,
            Wizard,
            Archer,
            Assassin
        }

        public enum EnemyClass
        {
            None,
            Beast,
            Undead,
            Golem,
            Demon,
            Humanoid
        }

        public enum EnemySize
        {
            Small = 1,
            Medium = 2,
            Large = 3
        }

        public enum EnemyActionPatternType
        {
            None,
            FixedTurn,
            ConditionalRandom
        }

        [field: Header("Identity")]
        [field: SerializeField] public string CharacterId { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public CharacterType Type { get; private set; }
        [field: SerializeField] public AffinityClass Affinity { get; private set; }

        [field: Header("Player Class")]
        [field: SerializeField] public PlayerBaseClass BaseClass { get; private set; }
        [field: SerializeField] public PlayerSubClass SubClass { get; private set; }

        [field: Header("Enemy Class")]
        [field: SerializeField] public EnemyClass EnemyCharacterClass { get; private set; }
        [field: SerializeField] public EnemySize Size { get; private set; } = EnemySize.Small;

        [field: Header("Battle")]
        [field: SerializeField] public int[] SkillIndices { get; private set; }
        [field: SerializeField] public EnemyActionPattern[] EnemyActionPatterns { get; private set; }

        [field: Header("Resources")]
        [field: SerializeField] public Sprite Portrait { get; private set; }
        [field: SerializeField] public Sprite BattleSprite { get; private set; }
        [field: SerializeField] public AnimationClip Skill1Animation { get; private set; }
        [field: SerializeField] public AnimationClip Skill2Animation { get; private set; }

        public bool HasCharacterId => !string.IsNullOrWhiteSpace(CharacterId);

        [System.Serializable]
        public struct EnemyActionPattern
        {
            [field: SerializeField] public EnemyActionPatternType PatternType { get; private set; }
            [field: SerializeField] public int Turn { get; private set; }
            [field: SerializeField] public int[] CandidateSkillIndices { get; private set; }
            [field: SerializeField] public string ConditionKey { get; private set; }
        }
    }
}
