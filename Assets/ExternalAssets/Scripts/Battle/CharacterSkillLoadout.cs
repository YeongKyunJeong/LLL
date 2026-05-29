using System.Collections.Generic;
using UnityEngine;

namespace LLL
{
    public class CharacterSkillLoadout : MonoBehaviour
    {
        [SerializeField] private int[] skillDataIndices = new int[2];
        [SerializeField] private float[] skillWeights = new float[2] { 1f, 1f };

        public int[] SkillDataIndices => skillDataIndices;
        public float[] SkillWeights => skillWeights;

        public static int[] BuildJewelSkillIndices(CharacterSkillLoadout[] loadouts, int requiredCount)
        {
            List<int> skillIndices = new List<int>();

            if (loadouts != null)
            {
                for (int i = 0; i < loadouts.Length; i++)
                {
                    if (loadouts[i] == null || loadouts[i].skillDataIndices == null) continue;

                    for (int skillIndex = 0; skillIndex < loadouts[i].skillDataIndices.Length; skillIndex++)
                    {
                        int value = loadouts[i].skillDataIndices[skillIndex];
                        if (value < 0) continue;

                        skillIndices.Add(value);
                    }
                }
            }

            if (skillIndices.Count == 0) return null;

            int count = Mathf.Max(0, requiredCount);
            int[] result = new int[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = skillIndices[i % skillIndices.Count];
            }

            return result;
        }

        public static int[] BuildJewelSkillIndices(CharacterRuntimeData[] characters, int requiredCount)
        {
            List<int> skillIndices = new List<int>();

            if (characters != null)
            {
                for (int i = 0; i < characters.Length; i++)
                {
                    CharacterData characterData = characters[i] != null ? characters[i].CharacterData : null;
                    if (characterData == null || characterData.SkillIndices == null) continue;

                    for (int skillIndex = 0; skillIndex < characterData.SkillIndices.Length; skillIndex++)
                    {
                        int value = characterData.SkillIndices[skillIndex];
                        if (value < 0) continue;

                        skillIndices.Add(value);
                    }
                }
            }

            if (skillIndices.Count == 0) return null;

            int count = Mathf.Max(0, requiredCount);
            int[] result = new int[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = skillIndices[i % skillIndices.Count];
            }

            return result;
        }

        public int GetRandomSkillIndex()
        {
            if (skillDataIndices == null || skillDataIndices.Length == 0) return -1;

            float totalWeight = 0f;
            for (int i = 0; i < skillDataIndices.Length; i++)
            {
                totalWeight += GetWeight(i);
            }

            if (totalWeight <= 0f) return skillDataIndices[0];

            float roll = UnityEngine.Random.value * totalWeight;
            for (int i = 0; i < skillDataIndices.Length; i++)
            {
                roll -= GetWeight(i);
                if (roll <= 0f) return skillDataIndices[i];
            }

            return skillDataIndices[skillDataIndices.Length - 1];
        }

        private float GetWeight(int index)
        {
            if (skillWeights == null || index < 0 || index >= skillWeights.Length) return 1f;

            return Mathf.Max(0f, skillWeights[index]);
        }
    }
}
