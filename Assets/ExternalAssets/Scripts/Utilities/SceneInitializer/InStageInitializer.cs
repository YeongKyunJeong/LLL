using UnityEngine;

namespace LLL
{
    public class InStageInitializer : SceneInitializer
    {
        [field: SerializeField] private GameObject gameManagerPrefab { get; set; }
        [field: SerializeField] private GameObject inStageManagerPrefab { get; set; }

        [field: SerializeField] private InStageManager inStageManager;

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

            if (inStageManager == null)
            {
                Debug.Log("Title Scene Manager Not Assigned");
                inStageManager = FindFirstObjectByType<InStageManager>();

                if (inStageManager == null)
                {
                    Debug.LogWarning("TitleScene Not Exists");
                    inStageManager = Instantiate(inStageManagerPrefab).GetComponent<InStageManager>();
                }
            }
#endif
            inStageManager.Initialize();

        }
    }
}
