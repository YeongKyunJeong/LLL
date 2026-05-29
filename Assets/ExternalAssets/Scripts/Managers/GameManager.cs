using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace LLL
{
    public enum SceneType
    {
        Null,
        TitleScene,
        StageSelectScene,
        InStageScene
    }

    public enum StageType
    {
        Tutorial,
        Cave
    }

    public class GameManager : MonoSingleton<GameManager>
    {

        public const string TITLE_SCENE_NAME_STR = "TitleScene";
        public const string STAGE_SELECT_SCENE_NAME_STR = "StageSelectScene";
        public const string IN_STAGE_SCENE_STR = "InStage";

        public static readonly string[] IN_STAGE_NAME_STRS = new string[2]
        {
            "Tutorial",
            "Cave"
        };

        //private bool isSceneLoading;
        private SceneType currentSceneType;
        [SerializeField] private JsonLoadManager jsonLoadManager;
        [SerializeField] private PartySelectionData selectedParty = PartySelectionData.CreateDefault(3);

        public SceneType CurrentSceneType
        {
            get
            {
                if (currentSceneType == SceneType.Null) GetCurrentSceneType();
                return currentSceneType;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (Instance == null) { } // Always False by MonoSingleton;

            //isSceneLoading = false;
            InitializeGlobalManagers();

        }

        private void InitializeGlobalManagers()
        {
            DontDestroyOnLoad(gameObject);

            if (JsonLoadManager != null)
            {
                JsonLoadManager.Initialize();
            }
        }

        private void InitializeScene()
        {
#if UNITY_EDITOR
            SceneInitializer sceneInitializer = FindFirstObjectByType<SceneInitializer>();
#endif
            if (sceneInitializer != null)
            {
                sceneInitializer.Initialize();
            }
            else
            {
                Debug.LogWarning("No Scene Initializer Found in This Scene");
            }

        }

        private SceneType GetCurrentSceneType()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName.Contains(IN_STAGE_SCENE_STR))
            {
                return currentSceneType = SceneType.InStageScene;
            }
            else if (sceneName.Contains(STAGE_SELECT_SCENE_NAME_STR))
            {
                return currentSceneType = SceneType.StageSelectScene;
            }
            else if (sceneName.Contains(TITLE_SCENE_NAME_STR))
            {
                return currentSceneType = SceneType.TitleScene;
            }

            return currentSceneType = SceneType.Null;
        }

        /// <param name="sceneName">�ε��� �� �̸�</param>
        //private void LoadScene(string sceneName, int stageNumber = 0)
        //{
        //    if (sceneName == IN_STAGE_SCENE_STR) sceneName = $"{IN_STAGE_SCENE_STR}_{stageNumber:00}";

        //    StartCoroutine(LoadSceneAsync(sceneName));
        //    // TO DO : Add Loading Logic;
        //}

        private void LoadScene(SceneType sceneType, int stageTypeIndex = 0, int stageNumber = 0)
        {
            //if (isSceneLoading)
            //{
            //    Debug.Log("Scene is Already Loading");
            //    return;
            //}

            string sceneName = string.Empty;
            switch (sceneType)
            {
                case SceneType.InStageScene:
                    {
                        sceneName = $"{IN_STAGE_SCENE_STR}_{IN_STAGE_NAME_STRS[stageTypeIndex]}_{stageNumber:00}";
                        break;
                    }
                case SceneType.StageSelectScene:
                    {
                        sceneName = STAGE_SELECT_SCENE_NAME_STR;
                        break;
                    }
                case SceneType.TitleScene:
                    {
                        sceneName = TITLE_SCENE_NAME_STR;
                        break;
                    }
                default:
                    {
                        Debug.LogError($"Scene Named {sceneType} Not Exists");
                        return;
                    }
            }

            StartCoroutine(LoadSceneAsync(sceneName));
            // TO DO : Add Loading Logic;
        }

        private IEnumerator LoadSceneAsync(string sceneName, int stageNumber = 0)
        {
            if (sceneName == IN_STAGE_SCENE_STR) sceneName = $"{sceneName}_{stageNumber:00}";
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            GetCurrentSceneType();
            //isSceneLoading = false;

            yield return null;
        }

        private void LoadData(int userID, int saveNumber)
        {

        }

        private void SaveData()
        {

        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #region External Call

        #region TitleScene
        public void TitleSceneStartCall()
        {
            LoadScene(SceneType.StageSelectScene);
            // LoadScene(SceneType.InStageScene);
        }

        public void TitleSceneContinueCall()
        {

        }

        public void TitleSceneExitCall()
        {
            QuitGame();
        }

        #endregion

        #region Stage Select Scene
        public void SelectSceneStartCall(StageType stageType, int stageNumber)
        {
            LoadScene(SceneType.InStageScene, (int)stageType, stageNumber);

            //LoadScene(SceneType.InStageScene, );
        }
        #endregion

        #region In Stage Scene
        //
        #endregion

        #region Global
        public void ToTitleCall()
        {
            LoadScene(SceneType.TitleScene);
        }
        #endregion

        #endregion

        public JsonLoadManager JsonLoadManager
        {
            get
            {
                if (jsonLoadManager == null)
                {
                    jsonLoadManager = GetComponentInChildren<JsonLoadManager>();
                }

                return jsonLoadManager;
            }
        }

        public PartySelectionData SelectedParty
        {
            get
            {
                if (selectedParty == null || selectedParty.Members == null || selectedParty.Members.Length == 0)
                {
                    selectedParty = PartySelectionData.CreateDefault(3);
                }

                return selectedParty;
            }
        }

        public void SetSelectedParty(PartySelectionData partySelectionData)
        {
            selectedParty = partySelectionData ?? PartySelectionData.CreateDefault(3);
        }
    }

}