using UnityEngine;

namespace LLL
{
    public class SelectSceneInitializer : SceneInitializer
    {
        [field: SerializeField] private GameObject gameManagerPrefab { get; set; }
        [field: SerializeField] private GameObject selectSceneManagerPrefab { get; set; }

        [field: SerializeField] private StageSelectManager stageSelectManager;

        public override void Initialize()
        {
            if (isInitialize) return;

            isInitialize = true;

#if UNITY_EDITOR
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.Log("Game Manager Not Exists");
                Instantiate(gameManagerPrefab);
            }

            if (stageSelectManager == null)
            {
                Debug.Log("Title Scene Manager Not Assigned");
                stageSelectManager = FindFirstObjectByType<StageSelectManager>();

                if (stageSelectManager == null)
                {
                    Debug.LogWarning("TitleScene Not Exists");
                    stageSelectManager = Instantiate(selectSceneManagerPrefab).GetComponent<StageSelectManager>();
                }
            }
#endif
            stageSelectManager.Initialize();

        }
    }
}
