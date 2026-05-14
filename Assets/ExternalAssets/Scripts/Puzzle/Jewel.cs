using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLL
{
    public class Jewel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerClickHandler
    {
        private static JewelManager jewelManager;
        private static SkillLibrary skillLibrary;

        public int ID { get => iD; }
        public Vector2 Pos { get => pos; }
        public int jewelType;
        public bool IsActive { get => isChosen; }

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
                // TO DO: Add Jewel Activate Logic;
                tempEffect.SetActive(true);
                return;
            }

            // TO DO: Add Jewel Deactivate Logic;
            tempEffect.SetActive(false);
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
                jewelType = Random.Range(0, 6);
            }
            else
            {
                jewelType = nextType;
            }

            if (tempIcon.TryGetComponent(out Image img))
            {
                //img.sprite = skillLibrary.SkillData[nextType].Icon;
                Debug.Log($"{skillLibrary.SkillData[jewelType].ToSafeString() }");
                img.color = jewelManager.tempColors[jewelType];
            }

        }

        private void ChangeJewelIcon(int target)
        {
            // TO DO: Add Jewel Icon Chage Logic;
        }

        //public void SetNewPos(Vector2 newPos)
        //{
        //    pos.x = newPos.x;
        //    pos.y = newPos.y;
        //}

        public void SetNewID(int newID)
        {
            iD = newID;
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
