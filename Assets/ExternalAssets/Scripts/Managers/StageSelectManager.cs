using UnityEngine;

namespace LLL
{
    public class StageSelectManager : MonoSingleton<StageSelectManager>
    {
        private GameManager gameManager;
        private bool isLoading;

        public void Initialize()
        {
            isLoading = false;
            gameManager = GameManager.Instance;
        }

        private void StartStageCall(StageType stage, int stageNumber)
        {
            gameManager.SelectSceneStartCall(stage, stageNumber);
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
