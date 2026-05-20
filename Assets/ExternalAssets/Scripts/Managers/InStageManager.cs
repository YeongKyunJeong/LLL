using UnityEngine;

namespace LLL
{
    public class InStageManager : MonoSingleton<InStageManager>
    {
        private GameManager gameManager;
        [field: SerializeField] private JewelManager jewelManager;
        [field: SerializeField] private SOManager sOManager;


        public void Initialize()
        {
            gameManager = GameManager.Instance;
            sOManager.Initialize();
            jewelManager.Initialize(sOManager);

        }


    }
}
