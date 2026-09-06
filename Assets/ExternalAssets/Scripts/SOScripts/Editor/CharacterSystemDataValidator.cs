using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LLL.Editor
{
    public static class CharacterSystemDataValidator
    {
        private const string PlayerLibraryPath = "Assets/ExternalAssets/SO/Library/Character/PlayerDataLibrary.asset";
        private const string MasterEnemyLibraryPath = "Assets/ExternalAssets/SO/Library/Character/MasterEnemyDataLibrary.asset";
        private const string StageEnemyLibrarySearchPath = "Assets/ExternalAssets/SO/Library/Character/StageEnemy";
        private const string CharacterStatsJsonPath = "Assets/ExternalAssets/Data/Character/CharacterStatsSample.json";
        private const string SkillLibraryPath = "Assets/ExternalAssets/SO/Library/SkillLibrary.asset";

        [MenuItem("LLL/Validation/Run Character System Validation")]
        public static void RunMenuValidation()
        {
            ValidationReport report = ValidateDefaultAssets();
            foreach (string warning in report.Warnings)
            {
                Debug.LogWarning(warning);
            }

            foreach (string error in report.Errors)
            {
                Debug.LogError(error);
            }

            if (report.IsValid)
            {
                Debug.Log($"Character system validation passed. warnings={report.Warnings.Count}");
            }
            else
            {
                Debug.LogError($"Character system validation failed. errors={report.Errors.Count}, warnings={report.Warnings.Count}");
            }
        }

        public static ValidationReport ValidateDefaultAssets()
        {
            PlayerDataLibrary playerLibrary = AssetDatabase.LoadAssetAtPath<PlayerDataLibrary>(PlayerLibraryPath);
            EnemyDataLibrary masterEnemyLibrary = AssetDatabase.LoadAssetAtPath<EnemyDataLibrary>(MasterEnemyLibraryPath);
            TextAsset characterStatsJson = AssetDatabase.LoadAssetAtPath<TextAsset>(CharacterStatsJsonPath);
            SkillLibrary skillLibrary = AssetDatabase.LoadAssetAtPath<SkillLibrary>(SkillLibraryPath);
            EnemyDataLibrary[] stageEnemyLibraries = LoadStageEnemyLibraries();

            return Validate(playerLibrary, masterEnemyLibrary, stageEnemyLibraries, characterStatsJson, skillLibrary);
        }

        public static ValidationReport Validate(
            PlayerDataLibrary playerLibrary,
            EnemyDataLibrary masterEnemyLibrary,
            IEnumerable<EnemyDataLibrary> stageEnemyLibraries,
            TextAsset characterStatsJson,
            SkillLibrary skillLibrary)
        {
            ValidationReport report = new ValidationReport();
            CharacterStatsJsonRepository statsRepository = new CharacterStatsJsonRepository();

            if (!statsRepository.Load(characterStatsJson))
            {
                report.AddError("Character stats JSON could not be loaded.");
            }

            HashSet<string> jsonIds = new HashSet<string>(statsRepository.StatsByCharacterId.Keys);
            ValidateStatsJson(statsRepository, report);
            ValidatePlayerLibrary(playerLibrary, jsonIds, skillLibrary, report);
            ValidateMasterEnemyLibrary(masterEnemyLibrary, jsonIds, skillLibrary, report);
            ValidateStageEnemyLibraries(stageEnemyLibraries, masterEnemyLibrary, report);

            return report;
        }

        private static EnemyDataLibrary[] LoadStageEnemyLibraries()
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyDataLibrary", new[] { StageEnemyLibrarySearchPath });
            EnemyDataLibrary[] result = new EnemyDataLibrary[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                result[i] = AssetDatabase.LoadAssetAtPath<EnemyDataLibrary>(AssetDatabase.GUIDToAssetPath(guids[i]));
            }

            return result.Where(library => library != null).ToArray();
        }

        private static void ValidateStatsJson(CharacterStatsJsonRepository statsRepository, ValidationReport report)
        {
            foreach (KeyValuePair<string, CharacterStatsEntry> pair in statsRepository.StatsByCharacterId)
            {
                CharacterStatsEntry entry = pair.Value;
                if (entry.levelStats == null || entry.levelStats.Length == 0)
                {
                    report.AddError($"JSON stats entry has no levelStats. characterId={pair.Key}");
                    continue;
                }

                HashSet<int> levels = new HashSet<int>();
                for (int i = 0; i < entry.levelStats.Length; i++)
                {
                    CharacterLevelStats levelStats = entry.levelStats[i];
                    if (!levels.Add(levelStats.level))
                    {
                        report.AddError($"JSON stats entry has duplicated level. characterId={pair.Key}, level={levelStats.level}");
                    }

                    if (levelStats.level < 1)
                    {
                        report.AddError($"JSON stats entry has invalid level. characterId={pair.Key}, level={levelStats.level}");
                    }

                    if (levelStats.maxHp <= 0)
                    {
                        report.AddError($"JSON stats entry has invalid maxHp. characterId={pair.Key}, level={levelStats.level}");
                    }
                }

                if (!levels.Contains(1))
                {
                    report.AddError($"JSON stats entry has no level 1 data. characterId={pair.Key}");
                }

                int maxLevel = levels.Count > 0 ? levels.Max() : 0;
                for (int level = 1; level <= maxLevel; level++)
                {
                    if (!levels.Contains(level))
                    {
                        report.AddWarning($"JSON stats entry has a level gap. characterId={pair.Key}, missingLevel={level}");
                    }
                }
            }
        }

        private static void ValidatePlayerLibrary(PlayerDataLibrary playerLibrary, HashSet<string> jsonIds, SkillLibrary skillLibrary, ValidationReport report)
        {
            if (playerLibrary == null)
            {
                report.AddError("PlayerDataLibrary is not assigned.");
                return;
            }

            ValidateCharacterArray("PlayerDataLibrary", playerLibrary.Characters, jsonIds, skillLibrary, report);
        }

        private static void ValidateMasterEnemyLibrary(EnemyDataLibrary masterEnemyLibrary, HashSet<string> jsonIds, SkillLibrary skillLibrary, ValidationReport report)
        {
            if (masterEnemyLibrary == null)
            {
                report.AddError("Master EnemyDataLibrary is not assigned.");
                return;
            }

            ValidateCharacterArray("MasterEnemyDataLibrary.NormalEnemies", masterEnemyLibrary.NormalEnemies, jsonIds, skillLibrary, report);
            ValidateCharacterArray("MasterEnemyDataLibrary.BossEnemies", masterEnemyLibrary.BossEnemies, jsonIds, skillLibrary, report);
        }

        private static void ValidateStageEnemyLibraries(IEnumerable<EnemyDataLibrary> stageEnemyLibraries, EnemyDataLibrary masterEnemyLibrary, ValidationReport report)
        {
            HashSet<string> masterIds = new HashSet<string>();
            AddCharacterIds(masterEnemyLibrary != null ? masterEnemyLibrary.NormalEnemies : null, masterIds);
            AddCharacterIds(masterEnemyLibrary != null ? masterEnemyLibrary.BossEnemies : null, masterIds);

            foreach (EnemyDataLibrary stageLibrary in stageEnemyLibraries ?? Enumerable.Empty<EnemyDataLibrary>())
            {
                ValidateStageCharacters(stageLibrary, stageLibrary.NormalEnemies, masterIds, report);
                ValidateStageCharacters(stageLibrary, stageLibrary.BossEnemies, masterIds, report);
            }
        }

        private static void ValidateCharacterArray(string context, CharacterData[] characters, HashSet<string> jsonIds, SkillLibrary skillLibrary, ValidationReport report)
        {
            HashSet<string> ids = new HashSet<string>();
            if (characters == null || characters.Length == 0)
            {
                report.AddWarning($"{context} has no characters.");
                return;
            }

            for (int i = 0; i < characters.Length; i++)
            {
                CharacterData character = characters[i];
                if (character == null)
                {
                    report.AddError($"{context} has a null character at index {i}.");
                    continue;
                }

                string id = character.CharacterId;
                if (string.IsNullOrWhiteSpace(id))
                {
                    report.AddError($"{context} has a character with empty id. index={i}, asset={character.name}");
                    continue;
                }

                if (!ids.Add(id))
                {
                    report.AddError($"{context} has duplicated character id. id={id}");
                }

                if (!jsonIds.Contains(id))
                {
                    report.AddError($"{context} character id has no JSON stats entry. id={id}");
                }

                ValidateCharacterResources(context, character, report);
                ValidateSkillIndices(context, character, skillLibrary, report);
            }
        }

        private static void ValidateCharacterResources(string context, CharacterData character, ValidationReport report)
        {
            if (character.Portrait == null)
            {
                report.AddWarning($"{context} character has no portrait. id={character.CharacterId}");
            }

            if (character.BattleSprite == null)
            {
                report.AddWarning($"{context} character has no battle sprite. id={character.CharacterId}");
            }

            if (character.Skill1Animation == null)
            {
                report.AddWarning($"{context} character has no Skill1Animation. id={character.CharacterId}");
            }

            if (character.Skill2Animation == null)
            {
                report.AddWarning($"{context} character has no Skill2Animation. id={character.CharacterId}");
            }
        }

        private static void ValidateSkillIndices(string context, CharacterData character, SkillLibrary skillLibrary, ValidationReport report)
        {
            if (character.SkillIndices == null || character.SkillIndices.Length == 0)
            {
                report.AddWarning($"{context} character has no skill indices. id={character.CharacterId}");
                return;
            }

            for (int i = 0; i < character.SkillIndices.Length; i++)
            {
                if (skillLibrary == null || skillLibrary.GetSkillData(character.SkillIndices[i]) == null)
                {
                    report.AddWarning($"{context} character has unresolved skill index. id={character.CharacterId}, skillIndex={character.SkillIndices[i]}");
                }
            }
        }

        private static void ValidateStageCharacters(EnemyDataLibrary stageLibrary, CharacterData[] characters, HashSet<string> masterIds, ValidationReport report)
        {
            if (stageLibrary == null || characters == null)
            {
                return;
            }

            for (int i = 0; i < characters.Length; i++)
            {
                CharacterData character = characters[i];
                if (character == null)
                {
                    report.AddError($"{stageLibrary.name} has a null stage enemy at index {i}.");
                    continue;
                }

                if (!masterIds.Contains(character.CharacterId))
                {
                    report.AddError($"{stageLibrary.name} contains an enemy not registered in MasterEnemyDataLibrary. id={character.CharacterId}");
                }
            }
        }

        private static void AddCharacterIds(CharacterData[] characters, HashSet<string> ids)
        {
            if (characters == null)
            {
                return;
            }

            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] != null && !string.IsNullOrWhiteSpace(characters[i].CharacterId))
                {
                    ids.Add(characters[i].CharacterId);
                }
            }
        }

        public class ValidationReport
        {
            private readonly List<string> errors = new List<string>();
            private readonly List<string> warnings = new List<string>();

            public IReadOnlyList<string> Errors => errors;
            public IReadOnlyList<string> Warnings => warnings;
            public bool IsValid => errors.Count == 0;

            public void AddError(string message)
            {
                errors.Add(message);
            }

            public void AddWarning(string message)
            {
                warnings.Add(message);
            }
        }
    }
}
