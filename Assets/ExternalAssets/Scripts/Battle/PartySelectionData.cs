using System;
using UnityEngine;

namespace LLL
{
    [Serializable]
    public class PartySelectionData
    {
        [field: SerializeField] public PartyMemberSelection[] Members { get; private set; }

        public PartySelectionData(PartyMemberSelection[] members)
        {
            Members = members;
        }

        public static PartySelectionData CreateDefault(int memberCount)
        {
            int count = Mathf.Max(0, memberCount);
            PartyMemberSelection[] members = new PartyMemberSelection[count];

            for (int i = 0; i < count; i++)
            {
                members[i] = new PartyMemberSelection(0, 1);
            }

            return new PartySelectionData(members);
        }
    }

    [Serializable]
    public struct PartyMemberSelection
    {
        [field: SerializeField] public int CharacterIndex { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public int[] EquipmentIndices { get; private set; }
        [field: SerializeField] public int[] SkillEnhancementLevels { get; private set; }

        public PartyMemberSelection(int characterIndex, int level, int[] equipmentIndices = null, int[] skillEnhancementLevels = null)
        {
            CharacterIndex = Mathf.Max(0, characterIndex);
            Level = Mathf.Max(1, level);
            EquipmentIndices = equipmentIndices ?? new int[0];
            SkillEnhancementLevels = skillEnhancementLevels ?? new int[0];
        }
    }
}
