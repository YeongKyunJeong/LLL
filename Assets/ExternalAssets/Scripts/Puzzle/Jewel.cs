using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLL
{
    public class Jewel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private static JewelManager jewelManager;
        private static SkillLibrary skillLibrary;
        private static Sprite[] fallbackIcons;

        public int ID { get => iD; }
        public Vector2 Pos { get => pos; }
        public int jewelType;
        public bool IsActive { get => isChosen; }
        public SkillData SkillData => skillLibrary != null ? skillLibrary.GetSkillData(jewelType) : null;
        public Vector3 WorldPosition
        {
            get
            {
                CacheVisualReferences();
                return rectTransform != null ? rectTransform.position : transform.position;
            }
        }

        [field: SerializeField] private int iD;
        [field: SerializeField] private Vector2 pos;
        [field: SerializeField] private bool isChosen;

        [field: SerializeField] private GameObject tempIcon;
        [field: SerializeField] private GameObject tempEffect;

        private bool isInitialize = false;
        private bool isPreviewingMove;
        private RectTransform rectTransform;
        private RectTransform iconRect;
        private RectTransform effectRect;
        private Image iconImage;
        private Image effectImage;
        private Vector3 iconBaseScale = Vector3.one;
        private Vector3 effectBaseScale = Vector3.one;
        private Vector3 iconBaseLocalPosition;
        private Vector3 effectBaseLocalPosition;
        private Color baseJewelColor = Color.white;


        public void Initialize(JewelManager _jewelManager, SkillLibrary _skillLibrary, int _iD)
        {
            if (jewelManager == null) jewelManager = _jewelManager;
            if (skillLibrary == null) skillLibrary = _skillLibrary;
            iD = _iD;
            CacheVisualReferences();
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
                SetDragFeedback(1);
                return;
            }

            ClearFeedback();
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
            CacheVisualReferences();

            SkillData skillData = SkillData;
            Color32 jewelColor = jewelManager.GetJewelColor(jewelType);
            baseJewelColor = jewelColor;

            if (iconImage != null)
            {
                iconImage.color = jewelColor;
                iconImage.sprite = skillData != null && skillData.Icon != null ? skillData.Icon : GetFallbackIcon(jewelType);
                iconImage.preserveAspect = true;
            }

            if (effectImage != null)
            {
                effectImage.color = WithAlpha(baseJewelColor, 0f);
            }

            ResetAnimationState();
        }

        public void SetDragFeedback(int connectedCount)
        {
            CacheVisualReferences();

            float clampedCount = Mathf.Clamp(connectedCount, 1, JewelManager.DefaultLineLength);
            float progress = clampedCount / JewelManager.DefaultLineLength;
            float scale = 1f + Mathf.Lerp(0.08f, 0.22f, progress);
            float alpha = Mathf.Lerp(0.42f, 0.85f, progress);

            if (iconRect != null)
            {
                iconRect.localScale = iconBaseScale * scale;
            }

            if (effectRect != null)
            {
                effectRect.localScale = effectBaseScale * scale;
            }

            if (effectImage != null)
            {
                effectImage.gameObject.SetActive(true);
                effectImage.color = WithAlpha(baseJewelColor, alpha);
            }
        }

        public void SetMovePreview(bool enabled)
        {
            isPreviewingMove = enabled;

            if (!enabled && !isChosen)
            {
                ClearFeedback();
            }
        }

        public void ClearFeedback()
        {
            CacheVisualReferences();
            isPreviewingMove = false;
            ResetAnimationState();
        }

        public void ResetAnimationState()
        {
            CacheVisualReferences();

            if (iconRect != null)
            {
                iconRect.localScale = iconBaseScale;
                iconRect.localPosition = iconBaseLocalPosition;
            }

            if (effectRect != null)
            {
                effectRect.localScale = effectBaseScale;
                effectRect.localPosition = effectBaseLocalPosition;
            }

            if (iconImage != null)
            {
                iconImage.color = WithAlpha(baseJewelColor, 1f);
            }

            if (effectImage != null)
            {
                effectImage.color = WithAlpha(baseJewelColor, 0f);
                effectImage.gameObject.SetActive(false);
            }
        }

        public void SetSequenceVisual(float alpha, float scale, bool showEffect)
        {
            CacheVisualReferences();
            alpha = Mathf.Clamp01(alpha);
            scale = Mathf.Max(0f, scale);

            if (iconRect != null)
            {
                iconRect.localScale = iconBaseScale * scale;
            }

            if (effectRect != null)
            {
                effectRect.localScale = effectBaseScale * scale;
            }

            if (iconImage != null)
            {
                iconImage.color = WithAlpha(baseJewelColor, alpha);
            }

            if (effectImage != null)
            {
                effectImage.gameObject.SetActive(showEffect && alpha > 0f);
                effectImage.color = WithAlpha(baseJewelColor, showEffect ? alpha * 0.75f : 0f);
            }
        }

        public void SetVisualWorldOffset(Vector3 offset)
        {
            CacheVisualReferences();

            if (iconRect != null)
            {
                iconRect.localPosition = iconBaseLocalPosition + GetLocalOffset(iconRect, offset);
            }

            if (effectRect != null)
            {
                effectRect.localPosition = effectBaseLocalPosition + GetLocalOffset(effectRect, offset);
            }
        }

        private void Update()
        {
            if (!isPreviewingMove || isChosen) return;

            CacheVisualReferences();
            float pulse = (Mathf.Sin(Time.unscaledTime * 5.5f) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(0.18f, 0.58f, pulse);
            float scale = Mathf.Lerp(1.02f, 1.1f, pulse);

            if (iconRect != null)
            {
                iconRect.localScale = iconBaseScale * scale;
            }

            if (effectRect != null)
            {
                effectRect.localScale = effectBaseScale * scale;
            }

            if (effectImage != null)
            {
                effectImage.gameObject.SetActive(true);
                effectImage.color = WithAlpha(baseJewelColor, alpha);
            }
        }

        private void CacheVisualReferences()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (iconRect == null && tempIcon != null)
            {
                iconRect = tempIcon.GetComponent<RectTransform>();
                iconImage = tempIcon.GetComponent<Image>();
                if (iconRect != null)
                {
                    iconBaseScale = iconRect.localScale;
                    iconBaseLocalPosition = iconRect.localPosition;
                }
            }

            if (effectRect == null && tempEffect != null)
            {
                effectRect = tempEffect.GetComponent<RectTransform>();
                effectImage = tempEffect.GetComponent<Image>();
                if (effectRect != null)
                {
                    effectBaseScale = effectRect.localScale;
                    effectBaseLocalPosition = effectRect.localPosition;
                }
            }
        }

        private static Vector3 GetLocalOffset(RectTransform targetRect, Vector3 worldOffset)
        {
            if (targetRect == null || targetRect.parent == null) return worldOffset;

            return targetRect.parent.InverseTransformVector(worldOffset);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
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

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInitialize) return;

            jewelManager.MouseExitCall(this);
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
