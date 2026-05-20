using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLL
{
    public class Jewel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerClickHandler
    {
        private static JewelManager jewelManager;
        private static SkillLibrary skillLibrary;
        private static Sprite[] fallbackIcons;

        public int ID { get => iD; }
        public Vector2 Pos { get => pos; }
        public int jewelType;
        public bool IsActive { get => isChosen; }
        public SkillData SkillData => skillLibrary != null ? skillLibrary.GetSkillData(jewelType) : null;

        [field: SerializeField] private int iD;
        [field: SerializeField] private Vector2 pos;
        [field: SerializeField] private bool isChosen;

        [field: SerializeField] private GameObject tempIcon;
        [field: SerializeField] private GameObject tempEffect;

        private bool isInitialize = false;


        public void Initialize(JewelManager _jewelManager, SkillLibrary _skillLibrary, int _iD)
        {
            if (jewelManager == null) jewelManager = _jewelManager;
            if (skillLibrary == null) skillLibrary = _skillLibrary;
            iD = _iD;
            ChangeJewelType();
            isInitialize = true;
            isChosen = true; // To Initialize;
            ActivateJewel(false);
        }

        public void ActivateJewel(bool _isChosen)
        {
            if (isChosen == _isChosen) return;

            isChosen = _isChosen;

            if (_isChosen)
            {
                if (tempEffect != null) tempEffect.SetActive(true);
                return;
            }

            if (tempEffect != null) tempEffect.SetActive(false);
            return;
        }

        public void Pop(int nextType = -1)
        {
            ChangeJewelType(nextType);
            ActivateJewel(false);
        }

        public void ChangeJewelType(int nextType = -1)
        {
            if (nextType == -1)
            {
                jewelType = Random.Range(0, JewelManager.JewelSkillCount);
            }
            else
            {
                jewelType = Mathf.Clamp(nextType, 0, JewelManager.JewelSkillCount - 1);
            }

            ApplyVisual();
        }

        public void SetNewPos(Vector2 newPos)
        {
            pos = newPos;
        }

        public void SetNewID(int newID)
        {
            iD = newID;
        }

        public void ApplyVisual()
        {
            if (jewelManager == null) return;

            SkillData skillData = SkillData;
            Color32 jewelColor = jewelManager.GetJewelColor(jewelType);

            if (tempIcon != null && tempIcon.TryGetComponent(out Image img))
            {
                img.color = jewelColor;
                img.sprite = skillData != null && skillData.Icon != null ? skillData.Icon : GetFallbackIcon(jewelType);
                img.preserveAspect = true;
            }

            if (tempEffect != null && tempEffect.TryGetComponent(out Image effectImage))
            {
                effectImage.color = new Color(jewelColor.r / 255f, jewelColor.g / 255f, jewelColor.b / 255f, 0.65f);
            }
        }

        private static Sprite GetFallbackIcon(int jewelType)
        {
            if (fallbackIcons == null || fallbackIcons.Length != JewelManager.JewelSkillCount)
            {
                fallbackIcons = new Sprite[JewelManager.JewelSkillCount];
            }

            int index = Mathf.Clamp(jewelType, 0, JewelManager.JewelSkillCount - 1);
            if (fallbackIcons[index] != null) return fallbackIcons[index];

            Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    bool isFilled = IsFallbackIconPixel(index, x, y, texture.width, texture.height);
                    texture.SetPixel(x, y, isFilled ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            fallbackIcons[index] = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
            fallbackIcons[index].hideFlags = HideFlags.HideAndDontSave;
            return fallbackIcons[index];
        }

        private static bool IsFallbackIconPixel(int type, int x, int y, int width, int height)
        {
            float cx = x - width * 0.5f;
            float cy = y - height * 0.5f;
            float ax = Mathf.Abs(cx);
            float ay = Mathf.Abs(cy);

            switch (type)
            {
                case 0:
                    return cx * cx + cy * cy < 110f;
                case 1:
                    return ax + ay < 13f;
                case 2:
                    return y > 7 && y < 25 && ax < (y - 6) * 0.55f;
                case 3:
                    return ax < 4f || ay < 4f;
                case 4:
                    return ax < 5f && ay < 13f;
                default:
                    return Mathf.Abs(ax - ay) < 3f || ay < 3f;
            }
        }



        #region Pointer Event

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInitialize) return;

            jewelManager.MouseEnterCall(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isInitialize) return;

            GameObject gO = eventData.pointerCurrentRaycast.gameObject;
            if (gO != null && gO.TryGetComponent(out Jewel nowJewel))
            {
                jewelManager.MouseUpCall(this);
                return;
            }

            jewelManager.MouseUpCall(null);

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInitialize) return;
            jewelManager.MouseDownCall(this);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            //throw new System.NotImplementedException();
        }

        #endregion
    }
}
