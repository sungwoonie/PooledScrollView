using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

            if(scrollRect.content.TryGetComponent<HorizontalOrVerticalLayoutGroup>(out var layout))
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
            var elementsCulledAbove = Mathf.Clamp(Mathf.FloorToInt(GetScrollRectNormalizedPosition() * (ItemDataCount() - visibleCount)), 0, Mathf.Clamp(ItemDataCount() - (visibleCount + 1), 0, int.MaxValue));
            
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
            {
                return;
            }

            if(direction == ScrollDirection.TopToBottom)
            {
                var top = activatingItems[0];
                activatingItems.RemoveAt(0);
                activatingItems.Add(top);

                if(activatingItems.Count >= 2)
                {
                    top.transform.SetSiblingIndex(activatingItems[activatingItems.Count - 2].transform.GetSiblingIndex() + 1);
                }

                top.Data = itemDatas[itemNumber + activatingItems.Count - 1];

                if(updateAll)
                {
                    for(int i = 0; i < activatingItems.Count; i++)
                    {
                        activatingItems[i].Data = itemDatas[itemNumber + i];
                    }
                }
            }
            else
            {
                var bottom = activatingItems[activatingItems.Count - 1];
                activatingItems.RemoveAt(activatingItems.Count - 1);
                activatingItems.Insert(0, bottom);

                bottom.transform.SetSiblingIndex(activatingItems[1].transform.GetSiblingIndex());
                bottom.Data = itemDatas[itemNumber];

                if(updateAll)
                {
                    for(int i = 0; i < activatingItems.Count; i++)
                    {
                        activatingItems[i].Data = itemDatas[itemNumber + i];
                    }
                }
            }
        }

        #endregion

        #region "Space"

        protected override void UpdateSpaceElement(float size)
        {
            if(size <= 0)
            {
                spaceElement.ignoreLayout = true;
            }
            else
            {
                spaceElement.ignoreLayout = false;
                size -= layoutSpacing;
            }

            if(scrollRect.vertical)
            {
                spaceElement.minHeight = size;
                spaceElementRectTranstorm.sizeDelta = new Vector2(Screen.width, size);
            }
            else
            {
                spaceElement.minWidth = size;
                spaceElementRectTranstorm.sizeDelta = new Vector2(size, Screen.height);
            }

            spaceElement.transform.SetSiblingIndex(0);
        }

        #endregion
    }
}