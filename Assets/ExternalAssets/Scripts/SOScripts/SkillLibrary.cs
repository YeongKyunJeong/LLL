using UnityEngine;

namespace LLL
{
    [CreateAssetMenu(fileName = "SkillLibrary", menuName = "SO/Library/SkillLibrary")]
    public class SkillLibrary : ScriptableObject
    {
        [field: SerializeField] public SkillData[] SkillData { get; private set; }
    
    }
}
