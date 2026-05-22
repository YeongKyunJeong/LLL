using System;
using System.Collections;
using System.Collections.Generic;
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
        public const int DefaultLineLength = 7;

        private SOManager sOManager;
        
        [SerializeField] private Jewel[] jewels;
        [SerializeField] private int[] ChosenJewels;
        [SerializeField] private int dragCount;
        [SerializeField] private float popAnimationDuration = 0.18f;
        [SerializeField] private float moveAnimationDuration = 0.22f;
        [SerializeField] private float spawnAnimationDuration = 0.18f;
        
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
        public event Action<List<JewelSkillActivation>> PopResolved;

        private bool isDown;
        private bool isInputBlocked;
        private Direction dir;
        private SkillLibrary skillLibrary;
        private int hoveredJewelId = -1;

        private struct MoveSwap
        {
            public Jewel Source;
            public Jewel Target;

            public MoveSwap(Jewel source, Jewel target)
            {
                Source = source;
                Target = target;
            }
        }

        public struct JewelSkillActivation
        {
            public int JewelType { get; }
            public int Stage { get; }
            public int PopCount { get; }
            public int SequenceIndex { get; }
            public SkillData SkillData { get; }

            public JewelSkillActivation(int jewelType, int stage, int popCount, int sequenceIndex, SkillData skillData)
            {
                JewelType = jewelType;
                Stage = stage;
                PopCount = popCount;
                SequenceIndex = sequenceIndex;
                SkillData = skillData;
            }
        }

        public void Initialize(SOManager _sOManager)
        {
            sOManager = _sOManager;
            skillLibrary = sOManager != null ? sOManager.SkillLibrary : null;

            isDown = false;
            isInputBlocked = false;
            dir = Direction.None;
            hoveredJewelId = -1;
            dragCount = 0;
            ChosenJewels = new int[DefaultLineLength];

            EnsureJewelReferences();
            EnsurePalette();
            ClearMovePreview();

            int n = jewels.Length;
            for (int i = 0; i < n; i++)
            {
                if (jewels[i] == null) continue;

                jewels[i].Initialize(this, skillLibrary, i);
            }
        }

        public bool MouseDownCall(Jewel jewel)
        {
            if (isInputBlocked) return false;
            if (jewel == null) return false;
            if (isDown) return false;

            isDown = true;

            MouseEnterCall(jewel);
            return true;
        }

        public bool MouseUpCall(Jewel jewel)
        {
            if (isInputBlocked) return false;
            if (!isDown) return false;

            isDown = false;
            hoveredJewelId = -1;

            if (jewel == null)
            {

            }


            PopChosenJewels();

            return true;
        }

        public bool MouseEnterCall(Jewel jewel)
        {
            if (isInputBlocked) return false;
            if (jewel == null) return false;
            if (!isDown) return false;

            hoveredJewelId = jewel.ID;

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

            RefreshDragFeedback();
            RefreshMovePreview();

            return true;
        }

        public bool MouseExitCall(Jewel jewel)
        {
            if (isInputBlocked) return false;
            if (jewel == null) return false;
            if (hoveredJewelId != jewel.ID) return false;

            hoveredJewelId = -1;
            RefreshMovePreview();
            return true;
        }

        private void DragAhead(Jewel jewel)
        {
            if (dragCount >= ChosenJewels.Length) return;

            ChosenJewels[dragCount++] = jewel.ID;
            jewel.ActivateJewel(true);
            RefreshDragFeedback();
            RefreshMovePreview();
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
                RefreshDragFeedback();
                RefreshMovePreview();
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
                RefreshDragFeedback();
                RefreshMovePreview();
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
            RefreshDragFeedback();
            RefreshMovePreview();
            return true;
        }

        private void PopChosenJewels()
        {
            ClearMovePreview();
            int currentDragCount = dragCount;
            int[] popJewelIds = new int[currentDragCount];
            for (int i = 0; i < currentDragCount; i++)
            {
                popJewelIds[i] = ChosenJewels[i];
            }

            if (currentDragCount == ChosenJewels.Length)
            {
                Debug.Log("Pop!");
                Direction moveDirection = GetResolvedMoveDirection();
                List<JewelSkillActivation> skillActivations = BuildSkillActivations(popJewelIds);
                ResetDragState();
                StartCoroutine(ResolvePopSequence(popJewelIds, moveDirection, skillActivations));
            }
            else
            {
                Debug.Log("Reset");
                for (int i = 0; i < currentDragCount; i++)
                {
                    int id = popJewelIds[i];
                    jewels[id].ActivateJewel(false);
                }
                ResetDragState();
            }
        }

        private void ResetDragState()
        {
            dir = Direction.None;
            hoveredJewelId = -1;
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

        private void RefreshDragFeedback()
        {
            for (int i = 0; i < dragCount; i++)
            {
                Jewel jewel = GetJewelById(ChosenJewels[i]);
                if (jewel == null) continue;

                jewel.SetDragFeedback(dragCount);
            }
        }

        private void RefreshMovePreview()
        {
            ClearMovePreview();

            if (!isDown || dragCount != DefaultLineLength) return;

            Direction firstDirection;
            Direction secondDirection;
            GetMovePreviewDirections(out firstDirection, out secondDirection);

            ApplyMovePreview(firstDirection);
            ApplyMovePreview(secondDirection);
        }

        private void GetMovePreviewDirections(out Direction firstDirection, out Direction secondDirection)
        {
            firstDirection = Direction.None;
            secondDirection = Direction.None;

            if (dir == Direction.Up || dir == Direction.Down)
            {
                int leftCount;
                int rightCount;
                CountHorizontalAreasByChosenPath(out leftCount, out rightCount);

                if (leftCount < rightCount) firstDirection = Direction.Right;
                else if (rightCount < leftCount) firstDirection = Direction.Left;
                else
                {
                    firstDirection = Direction.Left;
                    secondDirection = Direction.Right;
                }

                return;
            }

            if (dir == Direction.Left || dir == Direction.Right)
            {
                int downCount;
                int upCount;
                CountVerticalAreasByChosenPath(out downCount, out upCount);

                if (downCount < upCount) firstDirection = Direction.Up;
                else if (upCount < downCount) firstDirection = Direction.Down;
                else
                {
                    firstDirection = Direction.Down;
                    secondDirection = Direction.Up;
                }
            }
        }

        private Direction GetResolvedMoveDirection()
        {
            GetMovePreviewDirections(out Direction firstDirection, out Direction secondDirection);

            if (secondDirection == Direction.None) return firstDirection;
            return UnityEngine.Random.value < 0.5f ? firstDirection : secondDirection;
        }

        private IEnumerator ResolvePopSequence(int[] popJewelIds, Direction moveDirection, List<JewelSkillActivation> skillActivations)
        {
            isInputBlocked = true;

            List<Jewel> poppedJewels = GetJewelList(popJewelIds);
            List<MoveSwap> moveSwaps = BuildMoveSwaps(popJewelIds, moveDirection);
            List<Jewel> spawnJewels = GetSpawnJewels(popJewelIds, moveDirection, moveSwaps);

            yield return PlayPopAnimation(poppedJewels);
            yield return PlayMoveAnimation(moveSwaps);

            ApplySwapResult(moveSwaps, spawnJewels);
            yield return PlaySpawnAnimation(spawnJewels);

            ClearAllBoardSelection();
            PopResolved?.Invoke(skillActivations);
            isInputBlocked = false;
        }

        private IEnumerator PlayPopAnimation(List<Jewel> poppedJewels)
        {
            float duration = Mathf.Max(0.01f, popAnimationDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float alpha = 1f - progress;
                float scale = Mathf.Lerp(1.2f, 0.1f, progress);

                for (int i = 0; i < poppedJewels.Count; i++)
                {
                    poppedJewels[i].SetSequenceVisual(alpha, scale, showEffect: true);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < poppedJewels.Count; i++)
            {
                poppedJewels[i].SetSequenceVisual(0f, 0f, showEffect: false);
            }
        }

        private IEnumerator PlayMoveAnimation(List<MoveSwap> moveSwaps)
        {
            float duration = Mathf.Max(0.01f, moveAnimationDuration);
            float elapsed = 0f;
            HashSet<int> sourceJewelIds = new HashSet<int>();

            for (int i = 0; i < moveSwaps.Count; i++)
            {
                sourceJewelIds.Add(moveSwaps[i].Source.ID);
            }

            for (int i = 0; i < moveSwaps.Count; i++)
            {
                if (sourceJewelIds.Contains(moveSwaps[i].Target.ID)) continue;

                moveSwaps[i].Target.SetSequenceVisual(0f, 1f, showEffect: false);
            }

            while (elapsed < duration)
            {
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                for (int i = 0; i < moveSwaps.Count; i++)
                {
                    MoveSwap swap = moveSwaps[i];
                    Vector3 offset = (swap.Target.WorldPosition - swap.Source.WorldPosition) * progress;
                    swap.Source.SetVisualWorldOffset(offset);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < moveSwaps.Count; i++)
            {
                MoveSwap swap = moveSwaps[i];
                swap.Source.SetVisualWorldOffset(swap.Target.WorldPosition - swap.Source.WorldPosition);
            }
        }

        private IEnumerator PlaySpawnAnimation(List<Jewel> spawnJewels)
        {
            float duration = Mathf.Max(0.01f, spawnAnimationDuration);
            float elapsed = 0f;

            for (int i = 0; i < spawnJewels.Count; i++)
            {
                spawnJewels[i].SetSequenceVisual(0f, 0.2f, showEffect: true);
            }

            while (elapsed < duration)
            {
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                for (int i = 0; i < spawnJewels.Count; i++)
                {
                    spawnJewels[i].SetSequenceVisual(progress, Mathf.Lerp(0.2f, 1f, progress), showEffect: true);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < spawnJewels.Count; i++)
            {
                spawnJewels[i].SetSequenceVisual(1f, 1f, showEffect: false);
            }
        }

        private void ApplySwapResult(List<MoveSwap> moveSwaps, List<Jewel> spawnJewels)
        {
            Dictionary<int, int> nextTypesByJewelId = new Dictionary<int, int>();

            for (int i = 0; i < moveSwaps.Count; i++)
            {
                MoveSwap swap = moveSwaps[i];
                nextTypesByJewelId[swap.Target.ID] = swap.Source.jewelType;
            }

            for (int i = 0; i < spawnJewels.Count; i++)
            {
                nextTypesByJewelId[spawnJewels[i].ID] = UnityEngine.Random.Range(0, JewelSkillCount);
            }

            foreach (KeyValuePair<int, int> pair in nextTypesByJewelId)
            {
                Jewel jewel = GetJewelById(pair.Key);
                if (jewel == null) continue;

                jewel.ChangeJewelType(pair.Value);
                jewel.ResetAnimationState();
            }

            for (int i = 0; i < moveSwaps.Count; i++)
            {
                moveSwaps[i].Source.ResetAnimationState();
            }
        }

        private List<MoveSwap> BuildMoveSwaps(int[] popJewelIds, Direction moveDirection)
        {
            List<MoveSwap> swaps = new List<MoveSwap>();
            Dictionary<Vector2Int, Jewel> jewelsByPosition = BuildJewelPositionMap();
            HashSet<int> popSet = BuildIdSet(popJewelIds);

            for (int i = 0; i < jewels.Length; i++)
            {
                Jewel source = jewels[i];
                if (!IsUsableBoardJewel(source) || popSet.Contains(source.ID)) continue;
                if (!TryGetMoveTargetPosition(source.Pos, moveDirection, popJewelIds, out Vector2Int targetPosition)) continue;
                if (!jewelsByPosition.TryGetValue(targetPosition, out Jewel target)) continue;

                swaps.Add(new MoveSwap(source, target));
            }

            return swaps;
        }

        private List<JewelSkillActivation> BuildSkillActivations(int[] popJewelIds)
        {
            List<JewelSkillActivation> activations = new List<JewelSkillActivation>();
            int[] popCountsByType = new int[JewelSkillCount];

            for (int i = 0; i < popJewelIds.Length; i++)
            {
                Jewel jewel = GetJewelById(popJewelIds[i]);
                if (jewel == null) continue;

                int jewelType = Mathf.Clamp(jewel.jewelType, 0, JewelSkillCount - 1);
                popCountsByType[jewelType]++;
            }

            int sequenceIndex = 0;
            for (int jewelType = 0; jewelType < popCountsByType.Length; jewelType++)
            {
                int remainingCount = popCountsByType[jewelType];
                while (remainingCount > 0)
                {
                    int stage = Mathf.Min(3, remainingCount);
                    activations.Add(new JewelSkillActivation(jewelType, stage, popCountsByType[jewelType], sequenceIndex++, GetSkillData(jewelType)));
                    remainingCount -= stage;
                }
            }

            return activations;
        }

        private List<Jewel> GetSpawnJewels(int[] popJewelIds, Direction moveDirection, List<MoveSwap> moveSwaps)
        {
            List<Jewel> spawnJewels = new List<Jewel>();
            HashSet<int> targetJewelIds = new HashSet<int>();

            for (int i = 0; i < moveSwaps.Count; i++)
            {
                targetJewelIds.Add(moveSwaps[i].Target.ID);
            }

            for (int i = 0; i < jewels.Length; i++)
            {
                Jewel spawnJewel = jewels[i];
                if (!IsUsableBoardJewel(spawnJewel)) continue;
                if (targetJewelIds.Contains(spawnJewel.ID)) continue;
                if (!IsSpawnSideJewel(spawnJewel.Pos, moveDirection, popJewelIds)) continue;

                spawnJewels.Add(spawnJewel);
            }

            return spawnJewels;
        }

        private bool TryGetMoveTargetPosition(Vector2 sourcePosition, Direction moveDirection, int[] popJewelIds, out Vector2Int targetPosition)
        {
            Vector2Int source = ToGridPosition(sourcePosition);
            targetPosition = source;

            switch (moveDirection)
            {
                case Direction.Right:
                    if (!TryGetPopPathCoordinate(source.y, useX: true, popJewelIds, out int lineXForRight)) return false;
                    if (source.x >= lineXForRight) return false;
                    targetPosition = new Vector2Int(source.x + 1, source.y);
                    return true;
                case Direction.Left:
                    if (!TryGetPopPathCoordinate(source.y, useX: true, popJewelIds, out int lineXForLeft)) return false;
                    if (source.x <= lineXForLeft) return false;
                    targetPosition = new Vector2Int(source.x - 1, source.y);
                    return true;
                case Direction.Up:
                    if (!TryGetPopPathCoordinate(source.x, useX: false, popJewelIds, out int lineYForUp)) return false;
                    if (source.y >= lineYForUp) return false;
                    targetPosition = new Vector2Int(source.x, source.y + 1);
                    return true;
                case Direction.Down:
                    if (!TryGetPopPathCoordinate(source.x, useX: false, popJewelIds, out int lineYForDown)) return false;
                    if (source.y <= lineYForDown) return false;
                    targetPosition = new Vector2Int(source.x, source.y - 1);
                    return true;
                default:
                    return false;
            }
        }

        private bool IsSpawnSideJewel(Vector2 jewelPosition, Direction moveDirection, int[] popJewelIds)
        {
            Vector2Int position = ToGridPosition(jewelPosition);

            switch (moveDirection)
            {
                case Direction.Right:
                    return TryGetPopPathCoordinate(position.y, useX: true, popJewelIds, out int lineXForRight) && position.x <= lineXForRight;
                case Direction.Left:
                    return TryGetPopPathCoordinate(position.y, useX: true, popJewelIds, out int lineXForLeft) && position.x >= lineXForLeft;
                case Direction.Up:
                    return TryGetPopPathCoordinate(position.x, useX: false, popJewelIds, out int lineYForUp) && position.y <= lineYForUp;
                case Direction.Down:
                    return TryGetPopPathCoordinate(position.x, useX: false, popJewelIds, out int lineYForDown) && position.y >= lineYForDown;
                default:
                    return false;
            }
        }

        private bool TryGetPopPathCoordinate(int fixedAxisCoordinate, bool useX, int[] popJewelIds, out int pathCoordinate)
        {
            for (int i = 0; i < popJewelIds.Length; i++)
            {
                Jewel jewel = GetJewelById(popJewelIds[i]);
                if (jewel == null) continue;

                int fixedAxis = useX ? Mathf.RoundToInt(jewel.Pos.y) : Mathf.RoundToInt(jewel.Pos.x);
                if (fixedAxis != fixedAxisCoordinate) continue;

                pathCoordinate = useX ? Mathf.RoundToInt(jewel.Pos.x) : Mathf.RoundToInt(jewel.Pos.y);
                return true;
            }

            pathCoordinate = 0;
            return false;
        }

        private Dictionary<Vector2Int, Jewel> BuildJewelPositionMap()
        {
            Dictionary<Vector2Int, Jewel> jewelsByPosition = new Dictionary<Vector2Int, Jewel>();

            for (int i = 0; i < jewels.Length; i++)
            {
                Jewel jewel = jewels[i];
                if (!IsUsableBoardJewel(jewel)) continue;

                jewelsByPosition[ToGridPosition(jewel.Pos)] = jewel;
            }

            return jewelsByPosition;
        }

        private List<Jewel> GetJewelList(int[] jewelIds)
        {
            List<Jewel> result = new List<Jewel>();

            for (int i = 0; i < jewelIds.Length; i++)
            {
                Jewel jewel = GetJewelById(jewelIds[i]);
                if (!IsUsableBoardJewel(jewel)) continue;

                result.Add(jewel);
            }

            return result;
        }

        private HashSet<int> BuildIdSet(int[] jewelIds)
        {
            HashSet<int> result = new HashSet<int>();

            for (int i = 0; i < jewelIds.Length; i++)
            {
                result.Add(jewelIds[i]);
            }

            return result;
        }

        private bool IsUsableBoardJewel(Jewel jewel)
        {
            return jewel != null && jewel.gameObject.activeInHierarchy;
        }

        private Vector2Int ToGridPosition(Vector2 position)
        {
            return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        }

        private void ResetAllBoardVisuals()
        {
            if (jewels == null) return;

            for (int i = 0; i < jewels.Length; i++)
            {
                if (jewels[i] == null) continue;

                jewels[i].ResetAnimationState();
            }
        }

        private void ClearAllBoardSelection()
        {
            if (jewels == null) return;

            for (int i = 0; i < jewels.Length; i++)
            {
                if (jewels[i] == null) continue;

                jewels[i].ActivateJewel(false);
                jewels[i].ResetAnimationState();
            }
        }

        private void CountHorizontalAreasByChosenPath(out int leftCount, out int rightCount)
        {
            leftCount = 0;
            rightCount = 0;

            int fallbackX = GetAverageChosenCoordinate(useX: true);

            for (int i = 0; i < jewels.Length; i++)
            {
                Jewel jewel = jewels[i];
                if (jewel == null || IsChosen(jewel.ID)) continue;

                int y = Mathf.RoundToInt(jewel.Pos.y);
                int lineX = TryGetChosenPathCoordinate(y, useX: true, out int chosenX) ? chosenX : fallbackX;
                int x = Mathf.RoundToInt(jewel.Pos.x);

                if (x < lineX) leftCount++;
                else if (x > lineX) rightCount++;
            }
        }

        private void CountVerticalAreasByChosenPath(out int downCount, out int upCount)
        {
            downCount = 0;
            upCount = 0;

            int fallbackY = GetAverageChosenCoordinate(useX: false);

            for (int i = 0; i < jewels.Length; i++)
            {
                Jewel jewel = jewels[i];
                if (jewel == null || IsChosen(jewel.ID)) continue;

                int x = Mathf.RoundToInt(jewel.Pos.x);
                int lineY = TryGetChosenPathCoordinate(x, useX: false, out int chosenY) ? chosenY : fallbackY;
                int y = Mathf.RoundToInt(jewel.Pos.y);

                if (y < lineY) downCount++;
                else if (y > lineY) upCount++;
            }
        }

        private bool TryGetChosenPathCoordinate(int fixedAxisCoordinate, bool useX, out int pathCoordinate)
        {
            for (int i = 0; i < dragCount; i++)
            {
                Jewel jewel = GetJewelById(ChosenJewels[i]);
                if (jewel == null) continue;

                int fixedAxis = useX ? Mathf.RoundToInt(jewel.Pos.y) : Mathf.RoundToInt(jewel.Pos.x);
                if (fixedAxis != fixedAxisCoordinate) continue;

                pathCoordinate = useX ? Mathf.RoundToInt(jewel.Pos.x) : Mathf.RoundToInt(jewel.Pos.y);
                return true;
            }

            pathCoordinate = 0;
            return false;
        }

        private int GetAverageChosenCoordinate(bool useX)
        {
            if (dragCount <= 0) return 0;

            int total = 0;
            int count = 0;

            for (int i = 0; i < dragCount; i++)
            {
                Jewel jewel = GetJewelById(ChosenJewels[i]);
                if (jewel == null) continue;

                total += useX ? Mathf.RoundToInt(jewel.Pos.x) : Mathf.RoundToInt(jewel.Pos.y);
                count++;
            }

            return count > 0 ? Mathf.RoundToInt((float)total / count) : 0;
        }

        private void ApplyMovePreview(Direction moveDirection)
        {
            if (moveDirection == Direction.None) return;

            for (int i = 0; i < jewels.Length; i++)
            {
                Jewel jewel = jewels[i];
                if (jewel == null || IsChosen(jewel.ID)) continue;

                Vector2 position = jewel.Pos;
                bool shouldPreview = false;

                switch (moveDirection)
                {
                    case Direction.Up:
                        shouldPreview = Mathf.RoundToInt(position.y) == 0;
                        break;
                    case Direction.Down:
                        shouldPreview = Mathf.RoundToInt(position.y) == DefaultLineLength - 1;
                        break;
                    case Direction.Left:
                        shouldPreview = Mathf.RoundToInt(position.x) == DefaultLineLength - 1;
                        break;
                    case Direction.Right:
                        shouldPreview = Mathf.RoundToInt(position.x) == 0;
                        break;
                }

                if (shouldPreview)
                {
                    jewel.SetMovePreview(true);
                }
            }
        }

        private void ClearMovePreview()
        {
            if (jewels == null) return;

            for (int i = 0; i < jewels.Length; i++)
            {
                if (jewels[i] == null) continue;
                if (IsChosen(jewels[i].ID)) continue;

                jewels[i].SetMovePreview(false);
            }
        }

        private bool IsChosen(int jewelId)
        {
            for (int i = 0; i < dragCount; i++)
            {
                if (ChosenJewels[i] == jewelId) return true;
            }

            return false;
        }

        private Jewel GetJewelById(int jewelId)
        {
            if (jewels == null || jewelId < 0 || jewelId >= jewels.Length) return null;
            return jewels[jewelId];
        }

    }
}
