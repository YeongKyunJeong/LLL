using UnityEngine;

namespace LLL
{
    public class InStageManager : MonoSingleton<InStageManager>
    {
        private GameManager gameManager;
        [field: SerializeField] private JewelManager jewelManager;
        [field: SerializeField] private SOManager sOManager;
        [field: SerializeField] private BattleController battleController;


        public void Initialize()
        {
            gameManager = GameManager.Instance;
            sOManager.Initialize();
            jewelManager.Initialize(sOManager);
            EnsureBattleController();
            battleController.Initialize(sOManager, jewelManager);

        }

        private void EnsureBattleController()
        {
            if (battleController == null)
            {
                battleController = FindFirstObjectByType<BattleController>();
            }

            if (battleController != null) return;

            GameObject battleControllerObject = new GameObject("BattleController");
            battleControllerObject.transform.SetParent(transform);
            battleController = battleControllerObject.AddComponent<BattleController>();
        }


    }
}
