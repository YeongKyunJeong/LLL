using UnityEngine;

namespace LLL
{
    public struct CharacterFinalStats
    {
        public int MaxHp { get; private set; }
        public int PhysicalPower { get; private set; }
        public int PhysicalDefense { get; private set; }
        public int MagicPower { get; private set; }
        public int MagicDefense { get; private set; }

        public CharacterFinalStats(int maxHp, int physicalPower, int physicalDefense, int magicPower, int magicDefense)
        {
            MaxHp = Mathf.Max(1, maxHp);
            PhysicalPower = Mathf.Max(0, physicalPower);
            PhysicalDefense = Mathf.Max(0, physicalDefense);
            MagicPower = Mathf.Max(0, magicPower);
            MagicDefense = Mathf.Max(0, magicDefense);
        }
    }

    public static class CharacterStatCalculator
    {
        public static CharacterFinalStats Calculate(CharacterRuntimeData runtimeData)
        {
            if (runtimeData == null)
            {
                return new CharacterFinalStats(1, 0, 0, 0, 0);
            }

            CharacterLevelStats baseStats = runtimeData.LevelStats;
            int maxHp = baseStats.maxHp;
            int physicalPower = baseStats.physicalPower;
            int physicalDefense = baseStats.physicalDefense;
            int magicPower = baseStats.magicPower;
            int magicDefense = baseStats.magicDefense;

            float maxHpMultiplier = 1f;
            float physicalPowerMultiplier = 1f;
            float physicalDefenseMultiplier = 1f;
            float magicPowerMultiplier = 1f;
            float magicDefenseMultiplier = 1f;

            EquipmentData[] equipment = runtimeData.EquippedItems;
            if (equipment != null)
            {
                for (int i = 0; i < equipment.Length; i++)
                {
                    if (equipment[i] == null) continue;

                    EquipmentData.CharacterStatBonus flatBonus = equipment[i].FlatBonus;
                    maxHp += flatBonus.MaxHp;
                    physicalPower += flatBonus.PhysicalPower;
                    physicalDefense += flatBonus.PhysicalDefense;
                    magicPower += flatBonus.MagicPower;
                    magicDefense += flatBonus.MagicDefense;

                    EquipmentData.CharacterStatMultiplier multiplier = equipment[i].StatMultiplier;
                    maxHpMultiplier *= NormalizeMultiplier(multiplier.MaxHp);
                    physicalPowerMultiplier *= NormalizeMultiplier(multiplier.PhysicalPower);
                    physicalDefenseMultiplier *= NormalizeMultiplier(multiplier.PhysicalDefense);
                    magicPowerMultiplier *= NormalizeMultiplier(multiplier.MagicPower);
                    magicDefenseMultiplier *= NormalizeMultiplier(multiplier.MagicDefense);
                }
            }

            return new CharacterFinalStats(
                Mathf.RoundToInt(maxHp * maxHpMultiplier),
                Mathf.RoundToInt(physicalPower * physicalPowerMultiplier),
                Mathf.RoundToInt(physicalDefense * physicalDefenseMultiplier),
                Mathf.RoundToInt(magicPower * magicPowerMultiplier),
                Mathf.RoundToInt(magicDefense * magicDefenseMultiplier));
        }

        private static float NormalizeMultiplier(float value)
        {
            return value > 0f ? value : 1f;
        }
    }
}
