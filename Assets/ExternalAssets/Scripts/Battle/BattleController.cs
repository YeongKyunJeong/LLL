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
        private class BattleCombatant
        {
            [field: SerializeField] public string DisplayName { get; private set; }
            [field: SerializeField] public int MaxHp { get; private set; } = 100;
            [field: SerializeField] public int CurrentHp { get; private set; } = 100;
            [field: SerializeField] public int Block { get; private set; }

            public bool IsAlive => CurrentHp > 0;
            public float HpRatio => MaxHp > 0 ? Mathf.Clamp01((float)CurrentHp / MaxHp) : 0f;

            public void Initialize(string displayName, int maxHp)
            {
                DisplayName = displayName;
                MaxHp = Mathf.Max(1, maxHp);
                CurrentHp = MaxHp;
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
                return appliedDamage;
            }

            public int Heal(int amount)
            {
                if (!IsAlive) return 0;

                int previousHp = CurrentHp;
                CurrentHp = Mathf.Min(MaxHp, CurrentHp + Mathf.Max(0, amount));
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
        [SerializeField] private int defaultPlayerPower = 10;
        [SerializeField] private int enemyAttackDamage = 8;
        [SerializeField] private bool autoBindHpBars = true;
        [SerializeField] private Image[] allyHpBars;
        [SerializeField] private Image[] enemyHpBars;

        private JewelManager jewelManager;
        private int currentTurn = 1;
        private bool isResolvingTurn;

        public void ConfigureRuntimeCombatants(CharacterRuntimeData[] allyRuntimeData, CharacterRuntimeData[] enemyRuntimeData)
        {
            allies = BuildCombatants(allyRuntimeData, "Ally");
            enemies = BuildCombatants(enemyRuntimeData, "Enemy");
        }

        public void Initialize(SOManager soManager, JewelManager sourceJewelManager)
        {
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
                return null;
            }

            BattleCombatant[] result = new BattleCombatant[runtimeData.Length];
            for (int i = 0; i < runtimeData.Length; i++)
            {
                result[i] = new BattleCombatant();

                if (runtimeData[i] != null)
                {
                    string displayName = string.IsNullOrWhiteSpace(runtimeData[i].DisplayName) ? $"{fallbackPrefix} {i + 1}" : runtimeData[i].DisplayName;
                    result[i].Initialize(displayName, runtimeData[i].LevelStats.maxHp);
                }
                else
                {
                    result[i].Initialize($"{fallbackPrefix} {i + 1}", 100);
                }
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
                if (skill == null) continue;

                ResolveDamageSkill(skill, activation.Stage);
                ResolveHealSkill(skill, activation.Stage);
                ResolveBlockSkill(skill, activation.Stage);
            }
        }

        private void ResolveDamageSkill(SkillData skill, int stage)
        {
            if (skill.Damages == null) return;

            for (int i = 0; i < skill.Damages.Length; i++)
            {
                SkillData.Damage damage = skill.Damages[i];
                int targetCount = Mathf.Max(1, GetStageInt(damage.TargetCount, stage, 1));
                int hitCount = Mathf.Max(1, GetStageInt(damage.HitCount, stage, 1));
                int amount = Mathf.Max(1, Mathf.RoundToInt(GetStageValue(damage.PhysicsFactors, damage.PhysicsCs, stage) + GetStageValue(damage.MagicFactors, damage.MagicCs, stage)));
                List<BattleCombatant> targets = GetTargets(enemies, damage.Target, targetCount);

                for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                {
                    for (int hit = 0; hit < hitCount; hit++)
                    {
                        int appliedDamage = targets[targetIndex].TakeDamage(amount);
                        Debug.Log($"{skill.name} Lv.{stage} dealt {appliedDamage} to {targets[targetIndex].DisplayName}");
                    }
                }
            }
        }

        private void ResolveHealSkill(SkillData skill, int stage)
        {
            if (skill.Heals == null) return;

            for (int i = 0; i < skill.Heals.Length; i++)
            {
                SkillData.Heal heal = skill.Heals[i];
                int targetCount = Mathf.Max(1, GetStageInt(heal.TargetCount, stage, 1));
                int amount = Mathf.Max(1, Mathf.RoundToInt(GetStageValue(heal.PhysicsFactors, heal.PhysicsCs, stage) + GetStageValue(heal.MagicFactors, heal.MagicCs, stage)));
                List<BattleCombatant> targets = GetTargets(allies, heal.Target, targetCount);

                for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                {
                    int appliedHeal = targets[targetIndex].Heal(amount);
                    Debug.Log($"{skill.name} Lv.{stage} healed {appliedHeal} on {targets[targetIndex].DisplayName}");
                }
            }
        }

        private void ResolveBlockSkill(SkillData skill, int stage)
        {
            if (skill.Blocks == null) return;

            for (int i = 0; i < skill.Blocks.Length; i++)
            {
                SkillData.Block block = skill.Blocks[i];
                int amount = Mathf.Max(1, Mathf.RoundToInt(GetStageValue(block.PhysicsFactors, block.PhysicsCs, stage) + GetStageValue(block.MagicFactors, block.MagicCs, stage)));
                BattleCombatant target = GetFirstAlive(allies);
                if (target == null) continue;

                target.AddBlock(amount);
                Debug.Log($"{skill.name} Lv.{stage} added {amount} block to {target.DisplayName}");
            }
        }

        private void ResolveEnemyPatterns()
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null || !enemies[i].IsAlive) continue;

                BattleCombatant target = GetFirstAlive(allies);
                if (target == null) break;

                int appliedDamage = target.TakeDamage(enemyAttackDamage);
                Debug.Log($"{enemies[i].DisplayName} attacked {target.DisplayName} for {appliedDamage}");
            }
        }

        private List<BattleCombatant> GetTargets(BattleCombatant[] candidates, SkillData.TargetType targetType, int targetCount)
        {
            List<BattleCombatant> aliveCandidates = candidates
                .Where(candidate => candidate != null && candidate.IsAlive)
                .ToList();

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

            return aliveCandidates.Take(Mathf.Max(1, targetCount)).ToList();
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

        private float GetStageValue(float[] factors, float[] constants, int stage)
        {
            int index = Mathf.Max(0, stage - 1);
            float factor = factors != null && factors.Length > 0 ? factors[Mathf.Min(index, factors.Length - 1)] : 0f;
            float constant = constants != null && constants.Length > 0 ? constants[Mathf.Min(index, constants.Length - 1)] : 0f;
            return defaultPlayerPower * factor + constant;
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

            if (enemies == null || enemies.Length == 0)
            {
                enemies = new BattleCombatant[3];
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
