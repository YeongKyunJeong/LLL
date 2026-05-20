using System.Linq;
using UnityEngine;

namespace LLL
{
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public class JewelManager : MonoSingleton<JewelManager>
    {
        public const int JewelSkillCount = 6;
        private const int DefaultLineLength = 7;

        private SOManager sOManager;
        
        [SerializeField] private Jewel[] jewels;
        [SerializeField] private int[] ChosenJewels;
        [SerializeField] private int dragCount;
        
        public Color32[] tempColors = new Color32[JewelSkillCount]
        {
            new Color32(225, 72, 76, 255),
            new Color32(238, 118, 70, 255),
            new Color32(72, 176, 103, 255),
            new Color32(145, 203, 75, 255),
            new Color32(72, 128, 224, 255),
            new Color32(85, 189, 231, 255),
        };

        public bool IsDown { get => isDown; }
        public Jewel[] Jewels => jewels;

        private bool isDown;
        private Direction dir;
        private SkillLibrary skillLibrary;

        public void Initialize(SOManager _sOManager)
        {
            sOManager = _sOManager;
            skillLibrary = sOManager != null ? sOManager.SkillLibrary : null;

            isDown = false;
            dir = Direction.None;
            dragCount = 0;
            ChosenJewels = new int[DefaultLineLength];

            EnsureJewelReferences();
            EnsurePalette();

            int n = jewels.Length;
            for (int i = 0; i < n; i++)
            {
                if (jewels[i] == null) continue;

                jewels[i].Initialize(this, skillLibrary, i);
            }
        }

        public bool MouseDownCall(Jewel jewel)
        {
            if (jewel == null) return false;
            if (isDown) return false;

            isDown = true;

            MouseEnterCall(jewel);
            return true;
        }

        public bool MouseUpCall(Jewel jewel)
        {
            if (!isDown) return false;

            isDown = false;

            if (jewel == null)
            {

            }


            PopChosenJewels();

            return true;
        }

        public bool MouseEnterCall(Jewel jewel)
        {
            if (jewel == null) return false;
            if (!isDown) return false;

            // Add Condition to Drag
            if (dragCount == 0)
            {
                if (SetFirstChosenJewel(jewel)) return true;

                // Add Jewel Activation Logic
            }
            else
            {
                int dx = (int)jewel.Pos.x - (int)jewels[ChosenJewels[dragCount - 1]].Pos.x;
                int dy = (int)jewel.Pos.y - (int)jewels[ChosenJewels[dragCount - 1]].Pos.y;

                switch (dir)
                {
                    case Direction.Up:
                        {
                            if (dy == 1 && Mathf.Abs(dx) <= 1)
                            {
                                DragAhead(jewel);
                            }
                            else if (dy == 0)
                            {
                                ChangeLastDrag(jewel);
                            }
                            else if (dy == -1)
                            {
                                RollBackAndChangeDrag(jewel);
                            }
                            break;
                        }
                    case Direction.Down:
                        {
                            if (dy == -1 && Mathf.Abs(dx) <= 1)
                            {
                                DragAhead(jewel);
                            }
                            else if (dy == 0)
                            {
                                ChangeLastDrag(jewel);
                            }
                            else if (dy == 1)
                            {
                                RollBackAndChangeDrag(jewel);
                            }
                            break;
                        }

                    case Direction.Left:
                        {
                            if (dx == -1 && Mathf.Abs(dy) <= 1)
                            {
                                DragAhead(jewel);
                            }
                            else if (dx == 0)
                            {
                                ChangeLastDrag(jewel);
                            }
                            else if (dx == 1)
                            {
                                RollBackAndChangeDrag(jewel);
                            }
                            break;
                        }
                    case Direction.Right:
                        {
                            if (dx == 1 && Mathf.Abs(dy) <= 1)
                            {
                                DragAhead(jewel);
                            }
                            else if (dx == 0)
                            {
                                ChangeLastDrag(jewel);
                            }
                            else if (dx == -1)
                            {
                                RollBackAndChangeDrag(jewel);
                            }
                            break;
                        }
                }

            }


            return true;
        }

        private void DragAhead(Jewel jewel)
        {
            if (dragCount >= ChosenJewels.Length) return;

            ChosenJewels[dragCount++] = jewel.ID;
            jewel.ActivateJewel(true);
        }

        private void ChangeLastDrag(Jewel jewel)
        {
            if (dragCount == 1)
            {
                SetFirstChosenJewel(jewel);
                return;
            }

            int d;
            if (dir == Direction.Up || dir == Direction.Down)
            {
                d = (int)jewel.Pos.x - (int)jewels[ChosenJewels[dragCount - 2]].Pos.x;
            }
            else // if(direction == Direction.Left || direction == Direction.Right)
            {
                d = (int)jewel.Pos.y - (int)jewels[ChosenJewels[dragCount - 2]].Pos.y;
            }

            if (Mathf.Abs(d) <= 1)
            {
                jewels[ChosenJewels[dragCount - 1]].ActivateJewel(false);

                ChosenJewels[dragCount - 1] = jewel.ID;
                jewel.ActivateJewel(true);
            }
        }

        private void RollBackAndChangeDrag(Jewel jewel)
        {
            if (dragCount < 2) return;

            if (dragCount == 2)
            {
                jewels[ChosenJewels[1]].ActivateJewel(false);
                SetFirstChosenJewel(jewel);
                return;
            }

            int d;
            if (dir == Direction.Up || dir == Direction.Down)
            {
                d = (int)jewel.Pos.x - (int)jewels[ChosenJewels[dragCount - 3]].Pos.x;
            }
            else // if(direction == Direction.Left || direction == Direction.Right)
            {
                d = (int)jewel.Pos.y - (int)jewels[ChosenJewels[dragCount - 3]].Pos.y;
            }

            if (Mathf.Abs(d) <= 1)
            {
                jewels[ChosenJewels[dragCount - 1]].ActivateJewel(false);
                jewels[ChosenJewels[dragCount - 2]].ActivateJewel(false);

                ChosenJewels[dragCount - 2] = jewel.ID;
                dragCount--;
                jewel.ActivateJewel(true);
            }
        }

        private bool SetFirstChosenJewel(Jewel jewel)
        {
            if ((int)jewel.Pos.y == 0) dir = Direction.Up;
            else if ((int)jewel.Pos.y == 6) dir = Direction.Down;
            else if ((int)jewel.Pos.x == 0) dir = Direction.Right;
            else if ((int)jewel.Pos.x == 6) dir = Direction.Left;
            else return false; // Not Start Pos;

            if (dragCount > 0)
            {
                jewels[ChosenJewels[0]].ActivateJewel(false);
            }

            ChosenJewels[0] = jewel.ID;
            jewel.ActivateJewel(true);

            dragCount = 1;
            return true;
        }

        private void PopChosenJewels()
        {
            if (dragCount == ChosenJewels.Length)
            {
                Debug.Log("Pop!");
                for (int i = 0; i < dragCount; i++)
                {
                    int id = ChosenJewels[i];
                    jewels[id].Pop();
                }
            }
            else
            {
                Debug.Log("Reset");
                for (int i = 0; i < dragCount; i++)
                {
                    int id = ChosenJewels[i];
                    jewels[id].ActivateJewel(false);
                }
            }
            dir = Direction.None;
            dragCount = 0;
        }

        public Color32 GetJewelColor(int jewelType)
        {
            EnsurePalette();

            int index = Mathf.Clamp(jewelType, 0, tempColors.Length - 1);
            return tempColors[index];
        }

        public SkillData GetSkillData(int jewelType)
        {
            return skillLibrary != null ? skillLibrary.GetSkillData(jewelType) : null;
        }

        private void EnsureJewelReferences()
        {
            if (jewels != null && jewels.Length > 0) return;

            jewels = FindObjectsByType<Jewel>(FindObjectsSortMode.None)
                .OrderBy(jewel => jewel.ID)
                .ToArray();
        }

        private void EnsurePalette()
        {
            if (tempColors != null && tempColors.Length >= JewelSkillCount) return;

            tempColors = new Color32[JewelSkillCount]
            {
                new Color32(225, 72, 76, 255),
                new Color32(238, 118, 70, 255),
                new Color32(72, 176, 103, 255),
                new Color32(145, 203, 75, 255),
                new Color32(72, 128, 224, 255),
                new Color32(85, 189, 231, 255),
            };
        }

    }
}
