using UnityEngine;

namespace LLL
{
    public class CharacterRuntimeData
    {
        public CharacterRuntimeData(CharacterData characterData, CharacterLevelStats levelStats, EquipmentData[] equippedItems = null)
        {
            CharacterData = characterData;
            LevelStats = levelStats;
            EquippedItems = equippedItems ?? new EquipmentData[0];
            CurrentHp = FinalStats.MaxHp;
        }

        public CharacterData CharacterData { get; }
        public CharacterLevelStats LevelStats { get; }
        public EquipmentData[] EquippedItems { get; }
        public int CurrentHp { get; private set; }
        public CharacterFinalStats FinalStats => CharacterStatCalculator.Calculate(this);

        public string CharacterId => CharacterData != null ? CharacterData.CharacterId : string.Empty;
        public string DisplayName => CharacterData != null ? CharacterData.DisplayName : string.Empty;
        public CharacterData.CharacterType Type => CharacterData != null ? CharacterData.Type : CharacterData.CharacterType.None;
        public CharacterData.EnemySize Size => CharacterData != null ? CharacterData.Size : CharacterData.EnemySize.Small;
        public int OccupiedSlots => Type == CharacterData.CharacterType.Player ? 1 : Mathf.Max(1, (int)Size);

        public void SetCurrentHp(int value)
        {
            CurrentHp = Mathf.Clamp(value, 0, FinalStats.MaxHp);
        }
    }
}
