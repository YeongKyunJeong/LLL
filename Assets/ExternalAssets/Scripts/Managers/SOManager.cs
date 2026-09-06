using System;
using UnityEngine;

namespace LLL
{
    public class SOManager : MonoSingleton<SOManager>
    {
        [field: SerializeField] public SkillLibrary SkillLibrary { get; private set; }
        [field: SerializeField] public PlayerDataLibrary PlayerDataLibrary { get; private set; }
        [field: SerializeField] public EnemyDataLibrary MasterEnemyDataLibrary { get; private set; }
        [field: SerializeField] public EquipmentDataLibrary EquipmentDataLibrary { get; private set; }

        [SerializeField] private int[] defaultJewelSkillIndices = new int[6] { 0, 1, 2, 3, 4, 5 };

        public void Initialize()
        {
            if (SkillLibrary == null)
            {
                throw new NotImplementedException("SkillLibrary Not Assigned");
            }
        }

        public SkillData GetSkillData(int index)
        {
            if (SkillLibrary == null)
            {
                Debug.LogError("SkillLibrary Not Assigned");
                return null;
            }

            return SkillLibrary.GetSkillData(index);
        }

        public CharacterData GetPlayerCharacterData(int index)
        {
            if (PlayerDataLibrary == null)
            {
                Debug.LogError("PlayerDataLibrary Not Assigned");
                return null;
            }

            return PlayerDataLibrary.GetCharacterData(index);
        }

        public CharacterData GetPlayerCharacterData(string characterId)
        {
            if (PlayerDataLibrary == null)
            {
                Debug.LogError("PlayerDataLibrary Not Assigned");
                return null;
            }

            return PlayerDataLibrary.GetCharacterData(characterId);
        }

        public CharacterData GetMasterEnemyCharacterData(string characterId)
        {
            if (MasterEnemyDataLibrary == null)
            {
                Debug.LogError("MasterEnemyDataLibrary Not Assigned");
                return null;
            }

            return MasterEnemyDataLibrary.GetCharacterData(characterId);
        }

        public CharacterData GetMasterEnemyCharacterData(int index, bool boss)
        {
            if (MasterEnemyDataLibrary == null)
            {
                Debug.LogError("MasterEnemyDataLibrary Not Assigned");
                return null;
            }

            return boss ? MasterEnemyDataLibrary.GetBossEnemyData(index) : MasterEnemyDataLibrary.GetNormalEnemyData(index);
        }

        public EquipmentData GetEquipmentData(int index)
        {
            if (EquipmentDataLibrary == null)
            {
                return null;
            }

            return EquipmentDataLibrary.GetEquipmentData(index);
        }

        public EquipmentData GetEquipmentData(string equipmentId)
        {
            if (EquipmentDataLibrary == null)
            {
                return null;
            }

            return EquipmentDataLibrary.GetEquipmentData(equipmentId);
        }

        public int[] GetDefaultJewelSkillIndices(int requiredCount)
        {
            int count = Mathf.Max(0, requiredCount);
            int[] result = new int[count];

            for (int i = 0; i < count; i++)
            {
                if (defaultJewelSkillIndices != null && i < defaultJewelSkillIndices.Length)
                {
                    result[i] = Mathf.Max(0, defaultJewelSkillIndices[i]);
                }
                else
                {
                    result[i] = i;
                }
            }

            return result;
        }
    }
}
