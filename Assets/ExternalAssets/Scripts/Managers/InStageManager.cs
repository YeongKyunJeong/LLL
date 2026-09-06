using UnityEngine;

namespace LLL
{
    public class InStageManager : MonoSingleton<InStageManager>
    {
        private GameManager gameManager;
        [field: SerializeField] private JewelManager jewelManager;
        [field: SerializeField] private SOManager sOManager;
        [field: SerializeField] private JsonLoadManager jsonLoadManager;
        [field: SerializeField] private BattleController battleController;
        [field: SerializeField] private EnemyDataLibrary stageEnemyDataLibrary;
        [field: SerializeField] private int defaultEnemyLevel = 1;

        private CharacterRuntimeData[] playerCharacters;
        private CharacterRuntimeData[] enemyCharacters;

        public CharacterRuntimeData[] PlayerCharacters => playerCharacters;
        public CharacterRuntimeData[] EnemyCharacters => enemyCharacters;
        public EnemyDataLibrary StageEnemyDataLibrary => stageEnemyDataLibrary;


        public void Initialize()
        {
            gameManager = GameManager.Instance;
            if (jsonLoadManager == null)
            {
                jsonLoadManager = gameManager != null ? gameManager.JsonLoadManager : FindFirstObjectByType<JsonLoadManager>();
            }

            sOManager.Initialize();
            CharacterDataResolver resolver = new CharacterDataResolver(sOManager, jsonLoadManager);
            playerCharacters = ResolvePlayerCharacters(resolver, gameManager != null ? gameManager.SelectedParty : PartySelectionData.CreateDefault(3));
            enemyCharacters = ResolveStageEnemyCharacters(resolver);
            int[] activeSkillIndices = CharacterSkillLoadout.BuildJewelSkillIndices(playerCharacters, JewelManager.JewelSkillCount);
            jewelManager.Initialize(sOManager, activeSkillIndices);
            EnsureBattleController();
            if (battleController == null) return;

            battleController.ConfigureRuntimeCombatants(playerCharacters, enemyCharacters);
            battleController.Initialize(sOManager, jewelManager);

        }

        private CharacterRuntimeData[] ResolvePlayerCharacters(CharacterDataResolver resolver, PartySelectionData partySelectionData)
        {
            PartyMemberSelection[] members = partySelectionData != null ? partySelectionData.Members : null;
            if (members == null || members.Length == 0)
            {
                members = PartySelectionData.CreateDefault(3).Members;
            }

            CharacterRuntimeData[] result = new CharacterRuntimeData[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                result[i] = resolver.ResolvePlayer(members[i].CharacterIndex, members[i].Level, members[i].EquipmentIndices);
            }

            return result;
        }

        private CharacterRuntimeData[] ResolveStageEnemyCharacters(CharacterDataResolver resolver)
        {
            CharacterData[] enemies = stageEnemyDataLibrary != null ? stageEnemyDataLibrary.NormalEnemies : null;
            if (enemies == null || enemies.Length == 0)
            {
                return new CharacterRuntimeData[0];
            }

            CharacterRuntimeData[] result = new CharacterRuntimeData[enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
            {
                result[i] = enemies[i] != null ? resolver.ResolveEnemy(stageEnemyDataLibrary, enemies[i].CharacterId, defaultEnemyLevel) : null;
            }

            return result;
        }

        private void EnsureBattleController()
        {
            if (battleController == null)
            {
                battleController = FindFirstObjectByType<BattleController>();
            }

            if (battleController != null) return;

            Debug.LogError("BattleController is not assigned. Place it in the scene or prefab and assign it from the inspector.");
        }


    }
}
