using UnityEngine;

namespace LLL
{
    public class TitleSceneManager : MonoSingleton<TitleSceneManager>
    {
        private GameManager gameManager;
        private bool isLoading;

        public void Initialize()
        {
            isLoading = false;
            gameManager = GameManager.Instance;
        }

        public void ContinueCall()
        {
            if (isLoading) return;

            isLoading = true;
            gameManager.TitleSceneContinueCall();
        }
        
        public void StartCall()
        {
            if (isLoading) return;

            isLoading = true;
            gameManager.TitleSceneStartCall();
        }
        
        public void ExitCall()
        {
            if (isLoading) return;

            isLoading = true;
            gameManager.TitleSceneExitCall();
        }

    }
}
