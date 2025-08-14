using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarCloudgamesLibrary
{
    public class ScrollViewGridController<TData, TItem> : ScrollViewController<TData, TItem> where TItem : ScrollViewItem<TData>
    {
        private List<LayoutElement> activatingSpaceItems;
        private ScrollViewPool<LayoutElement> spaceItemPool;
        private int constraintCount;

        public int spaceItemPoolCount;

        #region "Unity"

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ResetActivatingItems();
            spaceItemPool.Dispose();
        }

        #endregion

        #region "Initialize"

        public override void SetUp()
        {
            if(setup) return;

            base.SetUp();

            activatingSpaceItems = new List<LayoutElement>();
            spaceItemPool = new ScrollViewPool<LayoutElement>(CreateSpaceItem(scrollRect, elementSize), transform, spaceItemPoolCount); 

            if(scrollRect.content.TryGetComponent<GridLayoutGroup>(out var layout))
            {
                constraintCount = layout.constraintCount;
                layoutSpacing = scrollRect.vertical ? layout.spacing.y : layout.spacing.x;
                padding = layout.padding;
                elementSize += layoutSpacing;
            }
            else
            {
                DebugManager.DebugInGameWarningMessage($"Not assigned GridLayoutGroup");
            }
        }

        #endregion

        #region "Content"

        protected override void UpdateContent()
        {
            var totalElementsCount = ItemDataCount();
            var linesCount = totalElementsCount % constraintCount > 0 ? totalElementsCount / constraintCount + 1 : totalElementsCount / constraintCount;
            AdjustContentSize(elementSize * linesCount);

            var scrollAreaSize = GetScrollAreaSize(scrollRect.viewport);
            var elementsVisibleInScrollArea = Mathf.CeilToInt(scrollAreaSize / elementSize) * constraintCount + constraintCount;
            var elementsCulledAbove = Mathf.Clamp(Mathf.FloorToInt(GetScrollRectNormalizedPosition() * (linesCount * constraintCount - elementsVisibleInScrollArea)), 0,
                Mathf.Clamp(totalElementsCount - (elementsVisibleInScrollArea + constraintCount), 0, int.MaxValue));

            if(elementsCulledAbove != totalElementsCount - (elementsVisibleInScrollArea + constraintCount))
            {
                elementsCulledAbove -= elementsCulledAbove % constraintCount;
            }

            UpdateSpaceElement(elementsCulledAbove);

            var requiredElementsInList = Mathf.Min(elementsVisibleInScrollArea + constraintCount, totalElementsCount);

            if(activatingItems.Count != requiredElementsInList)
            {
                InitializeItems(requiredElementsInList, elementsCulledAbove);
            }
            else if(lastElementNumber != elementsCulledAbove)
            {
                UpdateItem(elementsCulledAbove > lastElementNumber ? ScrollDirection.TopToBottom : ScrollDirection.BottomToTop, elementsCulledAbove, false);
            }

            lastElementNumber = elementsCulledAbove;
        }

        #endregion

        #region "Item"

        protected override void UpdateItem(ScrollDirection direction, int itemNumber, bool updateAll)
        {
            if(activatingItems.Count == 0)
            {
                return;
            }

            var count = Mathf.Abs(itemNumber - lastElementNumber);

            if(direction == ScrollDirection.TopToBottom)
            {
                for(var i = 0; i < count; i++)
                {
                    var top = activatingItems[0];
                    activatingItems.RemoveAt(0);
                    activatingItems.Add(top);

                    if(activatingItems.Count >= 2)
                    {
                        top.transform.SetSiblingIndex(activatingItems[activatingItems.Count - 2].transform.GetSiblingIndex() + 1);
                    }

                    top.Data = itemDatas[itemNumber + (i + 1 - count) + activatingItems.Count - 1];
                }
            }
            else
            {
                for(var i = 0; i < count; i++)
                {
                    var bottom = activatingItems[activatingItems.Count - 1];
                    activatingItems.RemoveAt(activatingItems.Count - 1);
                    activatingItems.Insert(0, bottom);

                    bottom.transform.SetSiblingIndex(activatingItems[1].transform.GetSiblingIndex());
                    bottom.Data = itemDatas[itemNumber - (i + 1 - count)];
                }
            }
        }

        #endregion

        #region "SpaceItem"

        protected override void UpdateSpaceElement(float size)
        {
            int requiredSpaceElements = (int)size;

            if(activatingSpaceItems.Count == requiredSpaceElements)
            {
                return;
            }

            // 부족한 경우: 풀에서 꺼내어 추가
            while(activatingSpaceItems.Count < requiredSpaceElements)
            {
                var spaceElement = spaceItemPool.GetNextItem();
                spaceElement.transform.SetParent(scrollRect.content.transform, false);
                spaceElement.transform.SetSiblingIndex(0); // 항상 위쪽에 삽입
                activatingSpaceItems.Add(spaceElement);
            }

            // 초과한 경우: 뒤에서 제거 후 반환
            while(activatingSpaceItems.Count > requiredSpaceElements)
            {
                int lastIndex = activatingSpaceItems.Count - 1;
                spaceItemPool.Return(activatingSpaceItems[lastIndex]);
                activatingSpaceItems.RemoveAt(lastIndex);
            }
        }

        #endregion

        #region "ScrollView Method"

        public override void ResetActivatingItems()
        {
            base.ResetActivatingItems();

            activatingSpaceItems.ForEach(x => spaceItemPool.Return(x));
            activatingSpaceItems.Clear();
        }

        #endregion
    }
}