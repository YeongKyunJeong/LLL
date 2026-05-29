using UnityEngine;

namespace LLL
{
    [CreateAssetMenu(fileName = "EnemyDataLibrary", menuName = "SO/Library/EnemyDataLibrary")]
    public class EnemyDataLibrary : ScriptableObject
    {
        public enum LibraryType
        {
            Stage,
            Master
        }

        [field: Header("Library")]
        [field: SerializeField] public LibraryType Type { get; private set; } = LibraryType.Stage;

        [field: Header("Stage")]
        [field: SerializeField] public string StageId { get; private set; }

        [field: Header("Enemies")]
        [field: SerializeField] public CharacterData[] NormalEnemies { get; private set; }
        [field: SerializeField] public CharacterData[] BossEnemies { get; private set; }

        public CharacterData GetNormalEnemyData(int index)
        {
            return GetCharacterData(NormalEnemies, index);
        }

        public CharacterData GetBossEnemyData(int index)
        {
            return GetCharacterData(BossEnemies, index);
        }

        public CharacterData GetCharacterData(string characterId)
        {
            CharacterData character = FindCharacterData(NormalEnemies, characterId);
            if (character != null)
            {
                return character;
            }

            return FindCharacterData(BossEnemies, characterId);
        }

        private static CharacterData GetCharacterData(CharacterData[] characters, int index)
        {
            if (characters == null || index < 0 || index >= characters.Length)
            {
                return null;
            }

            return characters[index];
        }

        private static CharacterData FindCharacterData(CharacterData[] characters, string characterId)
        {
            if (characters == null || string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            for (int i = 0; i < characters.Length; i++)
            {
                CharacterData character = characters[i];
                if (character != null && character.CharacterId == characterId)
                {
                    return character;
                }
            }

            return null;
        }
    }
}
