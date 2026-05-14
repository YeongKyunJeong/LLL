using UnityEngine;

namespace LLL
{
    public class TitleSceneInitializer : SceneInitializer
    {
        [field: SerializeField] private GameObject gameManagerPrefab { get; set; }
        [field: SerializeField] private GameObject titleSceneManagerPrefab { get; set; }

        [field: SerializeField] private TitleSceneManager titleSceneManager;

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

            if (titleSceneManager == null)
            {
                Debug.Log("Title Scene Manager Not Assigned");
                titleSceneManager = FindFirstObjectByType<TitleSceneManager>();

                if (titleSceneManager == null)
                {
                    Debug.LogWarning("TitleScene Not Exists");
                    titleSceneManager = Instantiate(titleSceneManagerPrefab).GetComponent<TitleSceneManager>();
                }
            }
#endif
            titleSceneManager.Initialize();

        }
    }
}
