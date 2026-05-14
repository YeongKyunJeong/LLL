using System;
using System.Linq;
using UnityEditor.Profiling;
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
        private SOManager sOManager;
        
        [field: SerializeField] private Jewel[] jewels;
        [field: SerializeField] private int[] ChosenJewels;
        [field: SerializeField] private int dragCount;
        
        public Color32[] tempColors;

        public bool IsDown { get => isDown; }

        private bool isDown;
        private Direction dir;

        public void Initialize(SOManager _sOManager)
        {
            sOManager = _sOManager;
            isDown = false;
            dir = Direction.None;
            dragCount = 0;
            ChosenJewels = new int[7];

            SkillLibrary skillLibrary = sOManager.SkillLibrary;
            int n = jewels.Length;
            for (int i = 0; i < n; i++)
            {
                jewels[i].Initialize(this, skillLibrary, i);
            }
        }

        public bool MouseDownCall(Jewel jewel)
        {
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
            else if ((int)jewel.Pos.y == 6) dir = Direction.Left;
            else return false; // Not Start Pos;

            jewels[ChosenJewels[0]].ActivateJewel(false);

            ChosenJewels[0] = jewel.ID;
            jewel.ActivateJewel(true);

            dragCount = 1;
            return true;
        }

        private void PopChosenJewels()
        {
            if (dragCount == 7)
            {
                Debug.Log("Pop!");
                foreach (int id in ChosenJewels)
                {
                    jewels[id].Pop();
                }
            }
            else
            {
                Debug.Log("Reset");
                foreach (int id in ChosenJewels)
                {
                    jewels[id].ActivateJewel(false);
                }
            }
            dir = Direction.None;
            dragCount = 0;
        }

    }
}