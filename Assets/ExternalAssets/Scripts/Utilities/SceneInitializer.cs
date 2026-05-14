using UnityEngine;

namespace LLL
{

    public abstract class SceneInitializer : MonoSingleton<SceneInitializer>
    {
        protected bool isInitialize = false;

        public abstract void Initialize();

        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }
    }

}
