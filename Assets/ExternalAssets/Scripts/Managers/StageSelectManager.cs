using UnityEngine;

namespace LLL
{
    public class StageSelectManager : MonoSingleton<StageSelectManager>
    {
        private GameManager gameManager;
        private bool isLoading;
        [SerializeField] private int[] selectedCharacterIndices = new int[3] { 0, 0, 0 };
        [SerializeField] private int[] selectedCharacterLevels = new int[3] { 1, 1, 1 };

        public void Initialize()
        {
            isLoading = false;
            gameManager = GameManager.Instance;
        }

        private void StartStageCall(StageType stage, int stageNumber)
        {
            gameManager.SetSelectedParty(BuildSelectedPartyData());
            gameManager.SelectSceneStartCall(stage, stageNumber);
        }

        public void SetPartyCharacterIndex(int slotIndex, int characterIndex)
        {
            if (!IsValidPartySlot(slotIndex)) return;

            selectedCharacterIndices[slotIndex] = Mathf.Max(0, characterIndex);
        }

        public void SetPartyCharacterLevel(int slotIndex, int level)
        {
            if (!IsValidPartySlot(slotIndex)) return;

            selectedCharacterLevels[slotIndex] = Mathf.Max(1, level);
        }

        public void SetPartySelection(int[] characterIndices, int[] levels = null)
        {
            for (int i = 0; i < selectedCharacterIndices.Length; i++)
            {
                if (characterIndices != null && i < characterIndices.Length)
                {
                    selectedCharacterIndices[i] = Mathf.Max(0, characterIndices[i]);
                }

                if (levels != null && i < levels.Length)
                {
                    selectedCharacterLevels[i] = Mathf.Max(1, levels[i]);
                }
            }
        }

        public CharacterData[] GetAvailablePlayerCharacters()
        {
            SOManager soManager = SOManager.Instance;
            return soManager != null && soManager.PlayerDataLibrary != null ? soManager.PlayerDataLibrary.Characters : null;
        }

        private PartySelectionData BuildSelectedPartyData()
        {
            int count = Mathf.Min(selectedCharacterIndices.Length, selectedCharacterLevels.Length);
            PartyMemberSelection[] members = new PartyMemberSelection[count];

            for (int i = 0; i < count; i++)
            {
                members[i] = new PartyMemberSelection(selectedCharacterIndices[i], selectedCharacterLevels[i]);
            }

            return new PartySelectionData(members);
        }

        private bool IsValidPartySlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < selectedCharacterIndices.Length && slotIndex < selectedCharacterLevels.Length;
        }

        public void TutorialStageCall(int stageNumber)
        {
            if (isLoading) return;

            isLoading = true;
            StartStageCall(StageType.Tutorial, stageNumber);
        }

        public void CaveStageCall(int stageNumber)
        {
            if (isLoading) return;

            isLoading = true;
            StartStageCall(StageType.Cave, stageNumber);
        }


        public void ToTitleCall()
        {
            if (isLoading) return;

            // To Do : Add Save Logic
            isLoading = true;
            gameManager.ToTitleCall();
        }
    }
}
