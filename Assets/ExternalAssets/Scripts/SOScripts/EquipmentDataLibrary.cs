using UnityEngine;

namespace LLL
{
    [CreateAssetMenu(fileName = "EquipmentDataLibrary", menuName = "SO/Library/EquipmentDataLibrary")]
    public class EquipmentDataLibrary : ScriptableObject
    {
        [field: SerializeField] public EquipmentData[] EquipmentData { get; private set; }

        public EquipmentData GetEquipmentData(int index)
        {
            if (EquipmentData == null || index < 0 || index >= EquipmentData.Length)
            {
                return null;
            }

            return EquipmentData[index];
        }

        public EquipmentData GetEquipmentData(string equipmentId)
        {
            if (EquipmentData == null || string.IsNullOrWhiteSpace(equipmentId))
            {
                return null;
            }

            for (int i = 0; i < EquipmentData.Length; i++)
            {
                if (EquipmentData[i] != null && EquipmentData[i].EquipmentId == equipmentId)
                {
                    return EquipmentData[i];
                }
            }

            return null;
        }
    }
}
