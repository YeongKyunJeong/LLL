using UnityEngine;

namespace LLL
{
    [CreateAssetMenu(fileName = "SkillLibrary", menuName = "SO/Library/SkillLibrary")]
    public class SkillLibrary : ScriptableObject
    {
        [field: SerializeField] public SkillData[] SkillData { get; private set; }

        public SkillData GetSkillData(int index)
        {
            if (SkillData == null || index < 0 || index >= SkillData.Length)
            {
                return null;
            }

            return SkillData[index];
        }
    }
}
