using UnityEngine;

namespace LLL
{
    public class CharacterDataResolver
    {
        private readonly SOManager soManager;
        private readonly JsonLoadManager jsonLoadManager;

        public CharacterDataResolver(SOManager soManager, JsonLoadManager jsonLoadManager)
        {
            this.soManager = soManager;
            this.jsonLoadManager = jsonLoadManager;
        }

        public CharacterRuntimeData ResolvePlayer(int playerLibraryIndex, int level)
        {
            if (soManager == null)
            {
                Debug.LogError("SOManager is not assigned.");
                return null;
            }

            CharacterData characterData = soManager.GetPlayerCharacterData(playerLibraryIndex);
            return CreateRuntimeData(characterData, level);
        }

        public CharacterRuntimeData ResolvePlayer(string characterId, int level)
        {
            if (soManager == null)
            {
                Debug.LogError("SOManager is not assigned.");
                return null;
            }

            CharacterData characterData = soManager.GetPlayerCharacterData(characterId);
            return CreateRuntimeData(characterData, level);
        }

        public CharacterRuntimeData ResolveEnemy(EnemyDataLibrary stageEnemyDataLibrary, string characterId, int level)
        {
            CharacterData characterData = null;

            if (stageEnemyDataLibrary != null)
            {
                characterData = stageEnemyDataLibrary.GetCharacterData(characterId);
            }

            if (characterData == null && soManager != null)
            {
                characterData = soManager.GetMasterEnemyCharacterData(characterId);
            }

            return CreateRuntimeData(characterData, level);
        }

        public CharacterRuntimeData ResolveEnemy(EnemyDataLibrary stageEnemyDataLibrary, int index, bool boss, int level)
        {
            CharacterData characterData = null;

            if (stageEnemyDataLibrary != null)
            {
                characterData = boss ? stageEnemyDataLibrary.GetBossEnemyData(index) : stageEnemyDataLibrary.GetNormalEnemyData(index);
            }

            if (characterData == null && soManager != null)
            {
                characterData = soManager.GetMasterEnemyCharacterData(index, boss);
            }

            return CreateRuntimeData(characterData, level);
        }

        private CharacterRuntimeData CreateRuntimeData(CharacterData characterData, int level)
        {
            if (characterData == null)
            {
                Debug.LogError("CharacterData could not be resolved.");
                return null;
            }

            if (jsonLoadManager == null)
            {
                Debug.LogError("JsonLoadManager is not assigned.");
                return null;
            }

            if (!jsonLoadManager.TryGetCharacterLevelStats(characterData.CharacterId, level, out CharacterLevelStats levelStats))
            {
                Debug.LogError($"Character stats not found. characterId={characterData.CharacterId}, level={level}");
                return null;
            }

            return new CharacterRuntimeData(characterData, levelStats);
        }
    }
}
