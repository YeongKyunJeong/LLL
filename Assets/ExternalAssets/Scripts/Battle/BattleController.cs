using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LLL
{
    public class BattleController : MonoBehaviour
    {
        [Serializable]
        public class BattleCombatant
        {
            [field: SerializeField] public string DisplayName { get; private set; }
            [field: SerializeField] public int MaxHp { get; private set; } = 100;
            [field: SerializeField] public int CurrentHp { get; private set; } = 100;
            [field: SerializeField] public int Block { get; private set; }

            public CharacterRuntimeData RuntimeData { get; private set; }
            public CharacterFinalStats FinalStats { get; private set; }
            public bool IsAlive => CurrentHp > 0;
            public float HpRatio => MaxHp > 0 ? Mathf.Clamp01((float)CurrentHp / MaxHp) : 0f;
            public int OccupiedSlots => RuntimeData != null ? RuntimeData.OccupiedSlots : 1;

            public void Initialize(string displayName, int maxHp)
            {
                DisplayName = displayName;
                RuntimeData = null;
                FinalStats = new CharacterFinalStats(maxHp, 0, 0, 0, 0);
                MaxHp = FinalStats.MaxHp;
                CurrentHp = MaxHp;
                Block = 0;
            }

            public void Initialize(CharacterRuntimeData runtimeData, string fallbackName)
            {
                RuntimeData = runtimeData;
                FinalStats = CharacterStatCalculator.Calculate(runtimeData);
                DisplayName = runtimeData != null && !string.IsNullOrWhiteSpace(runtimeData.DisplayName) ? runtimeData.DisplayName : fallbackName;
                MaxHp = FinalStats.MaxHp;
                CurrentHp = Mathf.Clamp(runtimeData != null ? runtimeData.CurrentHp : MaxHp, 0, MaxHp);
                Block = 0;
            }

            public int TakeDamage(int amount)
            {
                if (!IsAlive) return 0;

                int remainingDamage = Mathf.Max(0, amount);
                int blockedDamage = Mathf.Min(Block, remainingDamage);
                Block -= blockedDamage;
                remainingDamage -= blockedDamage;

                int appliedDamage = Mathf.Min(CurrentHp, remainingDamage);
                CurrentHp -= appliedDamage;
                RuntimeData?.SetCurrentHp(CurrentHp);
                return appliedDamage;
            }

            public int Heal(int amount)
            {
                if (!IsAlive) return 0;

                int previousHp = CurrentHp;
                CurrentHp = Mathf.Min(MaxHp, CurrentHp + Mathf.Max(0, amount));
                RuntimeData?.SetCurrentHp(CurrentHp);
                return CurrentHp - previousHp;
            }

            public void AddBlock(int amount)
            {
                if (!IsAlive) return;

                Block += Mathf.Max(0, amount);
            }
        }

        [SerializeField] private BattleCombatant[] allies;
        [SerializeField] private BattleCombatant[] enemies;
        [SerializeField] private int enemyAttackDamage = 8;
        [SerializeField] private bool autoBindHpBars;
        [SerializeField] private Image[] allyHpBars;
        [SerializeField] private Image[] enemyHpBars;

        private SOManager soManager;
        private JewelManager jewelManager;
        private int currentTurn = 1;
        private bool isResolvingTurn;

        public void ConfigureRuntimeCombatants(CharacterRuntimeData[] allyRuntimeData, CharacterRuntimeData[] enemyRuntimeData)
        {
            allies = BuildCombatants(allyRuntimeData, "Ally");
            enemies = BuildCombatants(enemyRuntimeData, "Enemy");
        }

        public void Initialize(SOManager sourceSoManager, JewelManager sourceJewelManager)
        {
            soManager = sourceSoManager;
            if (jewelManager != null)
            {
                jewelManager.PopResolved -= ResolveTurn;
            }

            jewelManager = sourceJewelManager;
            if (jewelManager != null)
            {
                jewelManager.PopResolved += ResolveTurn;
            }

            EnsureDefaultCombatants();
            if (autoBindHpBars)
            {
                AutoBindHpBars();
            }

            RefreshHpBars();
        }

        private BattleCombatant[] BuildCombatants(CharacterRuntimeData[] runtimeData, string fallbackPrefix)
        {
            if (runtimeData == null || runtimeData.Length == 0)
            {
                return Array.Empty<BattleCombatant>();
            }

            BattleCombatant[] result = new BattleCombatant[runtimeData.Length];
            for (int i = 0; i < runtimeData.Length; i++)
            {
                result[i] = new BattleCombatant();
                result[i].Initialize(runtimeData[i], $"{fallbackPrefix} {i + 1}");
            }

            return result;
        }

        private void OnDestroy()
        {
            if (jewelManager != null)
            {
                jewelManager.PopResolved -= ResolveTurn;
            }
        }

        private void ResolveTurn(List<JewelManager.JewelSkillActivation> activations)
        {
            if (isResolvingTurn) return;

            isResolvingTurn = true;
            Debug.Log($"Battle Turn {currentTurn} Start");

            ResolvePlayerSkills(activations);
            ResolveEnemyPatterns();
            RefreshHpBars();

            Debug.Log($"Battle Turn {currentTurn} End");
            currentTurn++;
            isResolvingTurn = false;
        }

        private void ResolvePlayerSkills(List<JewelManager.JewelSkillActivation> activations)
        {
            if (activations == null || activations.Count == 0) return;

            for (int i = 0; i < activations.Count; i++)
            {
                JewelManager.JewelSkillActivation activation = activations[i];
                SkillData skill = activation.SkillData;
                BattleCombatant caster = GetCasterForActivation(activation.SequenceIndex);
                if (skill == null || caster == null) continue;

                ResolveDamageSkill(skill, activation.Stage, caster, enemies);
                ResolveHealSkill(skill, activation.Stage, caster, allies);
                ResolveBlockSkill(skill, activation.Stage, caster);
            }
        }

        private void ResolveDamageSkill(SkillData skill, int stage, BattleCombatant caster, BattleCombatant[] targetGroup)
        {
            if (skill.Damages == null) return;

            for (int i = 0; i < skill.Damages.Length; i++)
            {
                SkillData.Damage damage = skill.Damages[i];
                int targetCount = Mathf.Max(1, GetStageInt(damage.TargetCount, stage, 1));
                int hitCount = Mathf.Max(1, GetStageInt(damage.HitCount, stage, 1));
                List<BattleCombatant> targets = GetTargets(targetGroup, damage.Target, targetCount);

                for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                {
                    for (int hit = 0; hit < hitCount; hit++)
                    {
                        int amount = BattleCalculationUtility.CalculateDamage(caster, targets[targetIndex], damage, stage);
                        int appliedDamage = targets[targetIndex].TakeDamage(amount);
                        Debug.Log($"{caster.DisplayName} used {skill.name} Lv.{stage} and dealt {appliedDamage} to {targets[targetIndex].DisplayName}");
                    }
                }
            }
        }

        private void ResolveHealSkill(SkillData skill, int stage, BattleCombatant caster, BattleCombatant[] targetGroup)
        {
            if (skill.Heals == null) return;

            for (int i = 0; i < skill.Heals.Length; i++)
            {
                SkillData.Heal heal = skill.Heals[i];
                int targetCount = Mathf.Max(1, GetStageInt(heal.TargetCount, stage, 1));
                List<BattleCombatant> targets = GetTargets(targetGroup, heal.Target, targetCount);

                for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                {
                    int amount = BattleCalculationUtility.CalculateHeal(caster, targets[targetIndex], heal, stage);
                    int appliedHeal = targets[targetIndex].Heal(amount);
                    Debug.Log($"{caster.DisplayName} used {skill.name} Lv.{stage} and healed {appliedHeal} on {targets[targetIndex].DisplayName}");
                }
            }
        }

        private void ResolveBlockSkill(SkillData skill, int stage, BattleCombatant caster)
        {
            if (skill.Blocks == null) return;

            for (int i = 0; i < skill.Blocks.Length; i++)
            {
                int amount = BattleCalculationUtility.CalculateBlock(caster, skill.Blocks[i], stage);
                caster.AddBlock(amount);
                Debug.Log($"{caster.DisplayName} used {skill.name} Lv.{stage} and gained {amount} block");
            }
        }

        private void ResolveEnemyPatterns()
        {
            if (enemies == null) return;

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null || !enemies[i].IsAlive) continue;

                SkillData skill = SelectEnemySkill(enemies[i]);
                if (skill != null)
                {
                    ResolveDamageSkill(skill, 1, enemies[i], allies);
                    ResolveHealSkill(skill, 1, enemies[i], enemies);
                    ResolveBlockSkill(skill, 1, enemies[i]);
                    continue;
                }

                BattleCombatant target = GetFirstAlive(allies);
                if (target == null) break;

                int fallbackDamage = Mathf.Max(1, enemies[i].FinalStats.PhysicalPower > 0 ? enemies[i].FinalStats.PhysicalPower : enemyAttackDamage);
                int appliedDamage = target.TakeDamage(fallbackDamage);
                Debug.Log($"{enemies[i].DisplayName} attacked {target.DisplayName} for {appliedDamage}");
            }
        }

        private SkillData SelectEnemySkill(BattleCombatant enemy)
        {
            CharacterData characterData = enemy != null && enemy.RuntimeData != null ? enemy.RuntimeData.CharacterData : null;
            if (characterData == null)
            {
                return null;
            }

            CharacterData.EnemyActionPattern[] patterns = characterData.EnemyActionPatterns;
            if (patterns != null)
            {
                for (int i = 0; i < patterns.Length; i++)
                {
                    if (patterns[i].PatternType == CharacterData.EnemyActionPatternType.FixedTurn && patterns[i].Turn == currentTurn)
                    {
                        return GetRandomCandidateSkill(patterns[i].CandidateSkillIndices);
                    }
                }

                List<int> conditionalCandidates = new List<int>();
                for (int i = 0; i < patterns.Length; i++)
                {
                    if (patterns[i].PatternType != CharacterData.EnemyActionPatternType.ConditionalRandom) continue;
                    if (!IsEnemyPatternConditionMet(enemy, patterns[i].ConditionKey)) continue;
                    if (patterns[i].CandidateSkillIndices != null)
                    {
                        conditionalCandidates.AddRange(patterns[i].CandidateSkillIndices);
                    }
                }

                SkillData conditionalSkill = GetRandomCandidateSkill(conditionalCandidates.ToArray());
                if (conditionalSkill != null)
                {
                    return conditionalSkill;
                }
            }

            return GetRandomCandidateSkill(characterData.SkillIndices);
        }

        private bool IsEnemyPatternConditionMet(BattleCombatant enemy, string conditionKey)
        {
            if (string.IsNullOrWhiteSpace(conditionKey))
            {
                return true;
            }

            switch (conditionKey.Trim().ToLowerInvariant())
            {
                case "lowhp":
                case "hp30":
                    return enemy != null && enemy.HpRatio <= 0.3f;
                case "allylowhp":
                    return enemies != null && enemies.Any(candidate => candidate != null && candidate.IsAlive && candidate.HpRatio <= 0.3f);
                case "playerblock":
                    return allies != null && allies.Any(candidate => candidate != null && candidate.IsAlive && candidate.Block > 0);
                default:
                    return true;
            }
        }

        private SkillData GetRandomCandidateSkill(int[] candidateSkillIndices)
        {
            if (candidateSkillIndices == null || candidateSkillIndices.Length == 0 || soManager == null)
            {
                return null;
            }

            int startIndex = UnityEngine.Random.Range(0, candidateSkillIndices.Length);
            for (int i = 0; i < candidateSkillIndices.Length; i++)
            {
                int index = candidateSkillIndices[(startIndex + i) % candidateSkillIndices.Length];
                SkillData skill = soManager.GetSkillData(index);
                if (skill != null)
                {
                    return skill;
                }
            }

            return null;
        }

        private BattleCombatant GetCasterForActivation(int sequenceIndex)
        {
            List<BattleCombatant> aliveAllies = allies != null ? allies.Where(candidate => candidate != null && candidate.IsAlive).ToList() : null;
            if (aliveAllies == null || aliveAllies.Count == 0)
            {
                return null;
            }

            return aliveAllies[Mathf.Abs(sequenceIndex) % aliveAllies.Count];
        }

        private List<BattleCombatant> GetTargets(BattleCombatant[] candidates, SkillData.TargetType targetType, int targetCount)
        {
            List<BattleCombatant> aliveCandidates = candidates != null
                ? candidates.Where(candidate => candidate != null && candidate.IsAlive).ToList()
                : new List<BattleCombatant>();

            if (aliveCandidates.Count == 0) return aliveCandidates;

            switch (targetType)
            {
                case SkillData.TargetType.Back:
                    aliveCandidates.Reverse();
                    break;
                case SkillData.TargetType.Vulnerable:
                    aliveCandidates = aliveCandidates
                        .OrderBy(candidate => candidate.HpRatio)
                        .ToList();
                    break;
                case SkillData.TargetType.Random:
                    aliveCandidates = aliveCandidates
                        .OrderBy(_ => UnityEngine.Random.value)
                        .ToList();
                    break;
            }

            List<BattleCombatant> selectedTargets = new List<BattleCombatant>();
            int occupiedSlots = 0;
            int requiredSlots = Mathf.Max(1, targetCount);
            for (int i = 0; i < aliveCandidates.Count && occupiedSlots < requiredSlots; i++)
            {
                selectedTargets.Add(aliveCandidates[i]);
                occupiedSlots += Mathf.Max(1, aliveCandidates[i].OccupiedSlots);
            }

            return selectedTargets;
        }

        private BattleCombatant GetFirstAlive(BattleCombatant[] candidates)
        {
            if (candidates == null) return null;

            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i] != null && candidates[i].IsAlive) return candidates[i];
            }

            return null;
        }

        private int GetStageInt(int[] values, int stage, int fallback)
        {
            if (values == null || values.Length == 0) return fallback;

            int index = Mathf.Clamp(stage - 1, 0, values.Length - 1);
            return values[index];
        }

        private void EnsureDefaultCombatants()
        {
            if (allies == null || allies.Length == 0)
            {
                allies = new BattleCombatant[3];
            }

            if (enemies == null)
            {
                enemies = Array.Empty<BattleCombatant>();
            }

            InitializeEmptyCombatants(allies, "Ally");
            InitializeEmptyCombatants(enemies, "Enemy");
        }

        private void InitializeEmptyCombatants(BattleCombatant[] combatants, string prefix)
        {
            for (int i = 0; i < combatants.Length; i++)
            {
                if (combatants[i] != null) continue;

                combatants[i] = new BattleCombatant();
                combatants[i].Initialize($"{prefix} {i + 1}", 100);
            }
        }

        private void AutoBindHpBars()
        {
            Image[] hpBars = FindObjectsByType<Image>(FindObjectsSortMode.None)
                .Where(image => image != null && image.gameObject.name == "HpBar")
                .OrderBy(image => image.transform.position.x)
                .ToArray();

            if (hpBars.Length < allies.Length + enemies.Length) return;

            allyHpBars = hpBars.Take(allies.Length).ToArray();
            enemyHpBars = hpBars.Skip(hpBars.Length - enemies.Length).Take(enemies.Length).ToArray();
        }

        private void RefreshHpBars()
        {
            RefreshHpBars(allyHpBars, allies);
            RefreshHpBars(enemyHpBars, enemies);
        }

        private void RefreshHpBars(Image[] hpBars, BattleCombatant[] combatants)
        {
            if (hpBars == null || combatants == null) return;

            int count = Mathf.Min(hpBars.Length, combatants.Length);
            for (int i = 0; i < count; i++)
            {
                if (hpBars[i] == null || combatants[i] == null) continue;

                hpBars[i].type = Image.Type.Filled;
                hpBars[i].fillMethod = Image.FillMethod.Horizontal;
                hpBars[i].fillOrigin = 0;
                hpBars[i].fillAmount = combatants[i].HpRatio;
            }
        }
    }
}
