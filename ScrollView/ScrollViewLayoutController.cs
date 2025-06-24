using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarCloudgamesLibrary
{
    public class ScrollViewLayoutController<TData, TItem> : ScrollViewController<TData, TItem> where TItem : ScrollViewItem<TData>
    {
        private LayoutElement spaceElement;
        private RectTransform spaceElementRectTranstorm;

        #region "Unity"

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Destroy(spaceElement.gameObject);
        }

        #endregion

        #region "Initialize"

        public override void SetUp()
        {
            if(setup) return;

            base.SetUp();

            if(scrollRect.content.TryGetComponent<HorizontalLayoutGroup>(out var layout))
            {
                layoutSpacing = layout.spacing;
                padding = layout.padding;
                elementSize += layoutSpacing;
            }
            else
            {
                DebugManager.DebugInGameWarningMessage($"Not Assigned..");
            }

            spaceElement = CreateSpaceItem(scrollRect, 0.0f);
            spaceElement.transform.SetParent(scrollRect.content.transform, false);
            spaceElementRectTranstorm = spaceElement.GetComponent<RectTransform>();
        }

        #endregion

        #region "Content"

        protected override void UpdateContent()
        {
            AdjustContentSize(elementSize * ItemDataCount());

            float scrollAreaSize = GetScrollAreaSize(scrollRect.viewport);

            int visibleCount = Mathf.CeilToInt(scrollAreaSize / elementSize);
            int maxSkippable = Mathf.Max(0, ItemDataCount() - visibleCount);
            int elementsCulledAbove = Mathf.FloorToInt(GetScrollRectNormalizedPosition() * (ItemDataCount() - visibleCount));
            elementsCulledAbove = Mathf.Clamp(elementsCulledAbove, 0, maxSkippable);

            UpdateSpaceElement(elementsCulledAbove * elementSize);

            int requiredCount = Mathf.Min(visibleCount + 1, ItemDataCount());

            if(activatingItems.Count != requiredCount)
            {
                InitializeItems(requiredCount, elementsCulledAbove);
            }
            else if(lastElementNumber != elementsCulledAbove)
            {
                var direction = elementsCulledAbove > lastElementNumber ? ScrollDirection.TopToBottom : ScrollDirection.BottomToTop;

                bool fullRefresh = Mathf.Abs(lastElementNumber - elementsCulledAbove) > 1;
                UpdateItem(direction, elementsCulledAbove, fullRefresh);
            }

            lastElementNumber = elementsCulledAbove;
        }

        #endregion

        #region "Item"

        protected override void UpdateItem(ScrollDirection direction, int itemNumber, bool updateAll)
        {
            if(activatingItems.Count == 0)
                return;

            if(direction == ScrollDirection.TopToBottom)
            {
                var top = activatingItems[0];
                activatingItems.RemoveAt(0);
                activatingItems.Add(top);

                if(activatingItems.Count >= 2)
                {
                    int newIndex = activatingItems[activatingItems.Count - 2].transform.GetSiblingIndex() + 1;
                    top.transform.SetSiblingIndex(newIndex);
                }

                int topDataIndex = lastElementNumber + activatingItems.Count - 1;
                if(topDataIndex < itemDatas.Count)
                {
                    top.Data = itemDatas[topDataIndex];
                }

                if(updateAll)
                {
                    for(int i = 0; i < activatingItems.Count; i++)
                    {
                        int dataIndex = lastElementNumber + i;
                        if(dataIndex < itemDatas.Count)
                        {
                            activatingItems[i].Data = itemDatas[dataIndex];
                        }
                    }
                }
            }
            else // ScrollDirection.BottomToTop
            {
                var bottom = activatingItems[activatingItems.Count - 1];
                activatingItems.RemoveAt(activatingItems.Count - 1);
                activatingItems.Insert(0, bottom);

                if(activatingItems.Count >= 2)
                {
                    int newIndex = activatingItems[1].transform.GetSiblingIndex();
                    bottom.transform.SetSiblingIndex(newIndex);
                }

                if(lastElementNumber < itemDatas.Count)
                {
                    bottom.Data = itemDatas[lastElementNumber];
                }

                if(updateAll)
                {
                    for(int i = 0; i < activatingItems.Count; i++)
                    {
                        int dataIndex = lastElementNumber + i;
                        if(dataIndex < itemDatas.Count)
                        {
                            activatingItems[i].Data = itemDatas[dataIndex];
                        }
                    }
                }
            }
        }

        #endregion

        #region "Space"

        protected override void UpdateSpaceElement(float size)
        {
            bool isEmpty = size <= 0f;
            spaceElement.ignoreLayout = isEmpty;

            if(!isEmpty)
            {
                size -= layoutSpacing; // spacing Àû¿ë
            }

            if(scrollRect.vertical)
            {
                spaceElement.minHeight = Mathf.Max(0f, size);
                spaceElementRectTranstorm.sizeDelta = new Vector2(Screen.width, size);
            }
            else
            {
                spaceElement.minWidth = Mathf.Max(0f, size);
                spaceElementRectTranstorm.sizeDelta = new Vector2(size, Screen.height);
            }

            spaceElement.transform.SetSiblingIndex(0);
        }

        #endregion
    }
}