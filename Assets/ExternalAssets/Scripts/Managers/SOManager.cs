using System;
using UnityEngine;

namespace LLL
{
    public class SOManager : MonoBehaviour
    {
        [field: SerializeField] public SkillLibrary SkillLibrary { get; private set; }

        public void Initialize()
        {
            if (SkillLibrary == null)
            {
                throw new NotImplementedException("SkillLibrary Not Assigned");
            }
        }
    }
}
