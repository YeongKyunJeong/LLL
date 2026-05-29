using UnityEngine;

namespace LLL
{
    [CreateAssetMenu(fileName = "PlayerDataLibrary", menuName = "SO/Library/PlayerDataLibrary")]
    public class PlayerDataLibrary : ScriptableObject
    {
        [field: SerializeField] public CharacterData[] Characters { get; private set; }

        public CharacterData GetCharacterData(int index)
        {
            if (Characters == null || index < 0 || index >= Characters.Length)
            {
                return null;
            }

            return Characters[index];
        }

        public CharacterData GetCharacterData(string characterId)
        {
            if (Characters == null || string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            for (int i = 0; i < Characters.Length; i++)
            {
                CharacterData character = Characters[i];
                if (character != null && character.CharacterId == characterId)
                {
                    return character;
                }
            }

            return null;
        }
    }
}
