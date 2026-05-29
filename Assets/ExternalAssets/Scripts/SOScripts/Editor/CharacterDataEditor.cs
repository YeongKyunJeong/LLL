using UnityEditor;
using UnityEngine;

namespace LLL.Editor
{
    [CustomEditor(typeof(CharacterData))]
    [CanEditMultipleObjects]
    public class CharacterDataEditor : UnityEditor.Editor
    {
        private SerializedProperty characterId;
        private SerializedProperty displayName;
        private SerializedProperty type;
        private SerializedProperty affinity;
        private SerializedProperty baseClass;
        private SerializedProperty subClass;
        private SerializedProperty enemyCharacterClass;
        private SerializedProperty size;
        private SerializedProperty skillIndices;
        private SerializedProperty enemyActionPatterns;
        private SerializedProperty portrait;
        private SerializedProperty battleSprite;
        private SerializedProperty skill1Animation;
        private SerializedProperty skill2Animation;

        private void OnEnable()
        {
            characterId = FindBackingField(nameof(CharacterData.CharacterId));
            displayName = FindBackingField(nameof(CharacterData.DisplayName));
            type = FindBackingField(nameof(CharacterData.Type));
            affinity = FindBackingField(nameof(CharacterData.Affinity));
            baseClass = FindBackingField(nameof(CharacterData.BaseClass));
            subClass = FindBackingField(nameof(CharacterData.SubClass));
            enemyCharacterClass = FindBackingField(nameof(CharacterData.EnemyCharacterClass));
            size = FindBackingField(nameof(CharacterData.Size));
            skillIndices = FindBackingField(nameof(CharacterData.SkillIndices));
            enemyActionPatterns = FindBackingField(nameof(CharacterData.EnemyActionPatterns));
            portrait = FindBackingField(nameof(CharacterData.Portrait));
            battleSprite = FindBackingField(nameof(CharacterData.BattleSprite));
            skill1Animation = FindBackingField(nameof(CharacterData.Skill1Animation));
            skill2Animation = FindBackingField(nameof(CharacterData.Skill2Animation));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSection("Identity");
            DrawProperty(characterId);
            DrawProperty(displayName);
            DrawProperty(type);
            DrawProperty(affinity);

            DrawSection("Class");
            DrawProperty(baseClass);
            DrawProperty(subClass);
            DrawProperty(enemyCharacterClass);

            DrawSection("Battle");
            DrawProperty(skillIndices, true);

            if (ShouldShowEnemyOnlyFields())
            {
                DrawProperty(size);
                DrawProperty(enemyActionPatterns, true);
            }
            else
            {
                EditorGUILayout.HelpBox("Player characters use a fixed size of 1 and do not expose enemy action patterns.", MessageType.Info);
            }

            DrawSection("Resources");
            DrawProperty(portrait);
            DrawProperty(battleSprite);
            DrawProperty(skill1Animation);
            DrawProperty(skill2Animation);

            serializedObject.ApplyModifiedProperties();
        }

        private bool ShouldShowEnemyOnlyFields()
        {
            if (type == null || type.hasMultipleDifferentValues)
            {
                return true;
            }

            CharacterData.CharacterType selectedType = (CharacterData.CharacterType)type.enumValueIndex;
            return selectedType == CharacterData.CharacterType.Enemy || selectedType == CharacterData.CharacterType.Boss;
        }

        private SerializedProperty FindBackingField(string propertyName)
        {
            return serializedObject.FindProperty($"<{propertyName}>k__BackingField");
        }

        private static void DrawSection(string label)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private static void DrawProperty(SerializedProperty property, bool includeChildren = false)
        {
            if (property == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(property, includeChildren);
        }
    }
}
