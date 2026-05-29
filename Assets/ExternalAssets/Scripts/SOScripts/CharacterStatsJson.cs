using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLL
{
    [Serializable]
    public class CharacterStatsJson
    {
        public CharacterStatsEntry[] characters;
    }

    [Serializable]
    public class CharacterStatsEntry
    {
        public string characterId;
        public CharacterLevelStats[] levelStats;
    }

    [Serializable]
    public struct CharacterLevelStats
    {
        public int level;
        public int requiredExp;
        public int maxHp;
        public int physicalPower;
        public int physicalDefense;
        public int magicPower;
        public int magicDefense;
    }

    public class CharacterStatsJsonRepository
    {
        private readonly Dictionary<string, CharacterStatsEntry> statsByCharacterId = new Dictionary<string, CharacterStatsEntry>();

        public IReadOnlyDictionary<string, CharacterStatsEntry> StatsByCharacterId => statsByCharacterId;

        public bool Load(TextAsset jsonTextAsset)
        {
            if (jsonTextAsset == null)
            {
                Debug.LogError("Character stats JSON TextAsset is not assigned.");
                return false;
            }

            return Load(jsonTextAsset.text);
        }

        public bool Load(string json)
        {
            statsByCharacterId.Clear();

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError("Character stats JSON is empty.");
                return false;
            }

            CharacterStatsJson parsed = JsonUtility.FromJson<CharacterStatsJson>(json);
            if (parsed == null || parsed.characters == null)
            {
                Debug.LogError("Character stats JSON could not be parsed.");
                return false;
            }

            bool result = true;
            for (int i = 0; i < parsed.characters.Length; i++)
            {
                CharacterStatsEntry entry = parsed.characters[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.characterId))
                {
                    Debug.LogWarning($"Character stats entry at index {i} has no characterId.");
                    result = false;
                    continue;
                }

                if (statsByCharacterId.ContainsKey(entry.characterId))
                {
                    Debug.LogWarning($"Duplicated character stats id: {entry.characterId}");
                    result = false;
                    continue;
                }

                statsByCharacterId.Add(entry.characterId, entry);
            }

            return result;
        }

        public bool TryGetStats(string characterId, out CharacterStatsEntry stats)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                stats = null;
                return false;
            }

            return statsByCharacterId.TryGetValue(characterId, out stats);
        }

        public bool TryGetLevelStats(string characterId, int level, out CharacterLevelStats levelStats)
        {
            levelStats = default;

            if (!TryGetStats(characterId, out CharacterStatsEntry stats) || stats.levelStats == null)
            {
                return false;
            }

            for (int i = 0; i < stats.levelStats.Length; i++)
            {
                if (stats.levelStats[i].level == level)
                {
                    levelStats = stats.levelStats[i];
                    return true;
                }
            }

            return false;
        }
    }
}
