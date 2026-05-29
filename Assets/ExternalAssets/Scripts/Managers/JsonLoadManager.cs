using System;
using UnityEngine;

namespace LLL
{
    public class JsonLoadManager : MonoSingleton<JsonLoadManager>
    {
        [field: SerializeField] public TextAsset CharacterStatsJson { get; private set; }

        private readonly CharacterStatsJsonRepository characterStatsRepository = new CharacterStatsJsonRepository();
        private bool isInitialized;

        protected override void Awake()
        {
            base.Awake();

            if (!isInitialized && CharacterStatsJson != null)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (CharacterStatsJson == null)
            {
                throw new NotImplementedException("CharacterStatsJson Not Assigned");
            }

            isInitialized = characterStatsRepository.Load(CharacterStatsJson);
        }

        public bool TryGetCharacterStats(string characterId, out CharacterStatsEntry stats)
        {
            EnsureInitialized();
            return characterStatsRepository.TryGetStats(characterId, out stats);
        }

        public bool TryGetCharacterLevelStats(string characterId, int level, out CharacterLevelStats levelStats)
        {
            EnsureInitialized();
            return characterStatsRepository.TryGetLevelStats(characterId, level, out levelStats);
        }

        private void EnsureInitialized()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }
    }
}
