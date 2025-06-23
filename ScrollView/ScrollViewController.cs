using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarCloudgamesLibrary
{
    public abstract class ScrollViewController<TItemData, TItem> : MonoBehaviour where TItem : ScrollViewItem<TItemData>
    {
        public TItem template;
        public int itemPoolCount;
        public List<TItemData> itemDatas;

        protected List<TItem> activatingItems;
        protected ScrollRect scrollRect;
        protected float layoutSpacing;
        protected RectOffset padding;
        protected float elementSize;
        protected int lastElementNumber;

        private ScrollViewPool<TItem> itemPool;
        private bool initialized;

        protected int ItemDataCount() => itemDatas.Count;

        #region "Unity"

        protected virtual void Awake()
        {
            SetUp();
        }

        protected virtual void Start()
        {
            if(!initialized)
            {
                InitializeScrollView();
            }
        }

        protected virtual void OnDestroy()
        {
            scrollRect.onValueChanged.RemoveListener(ScrollMoved);

            ResetActivatingItems();
            itemPool.Dispose();
        }

        #endregion

        #region "Initialize"

        public virtual void SetUp()
        {
            activatingItems = new List<TItem>();

            itemPool = new ScrollViewPool<TItem>(template, transform, itemPoolCount);

            scrollRect = GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener(ScrollMoved);

            if(scrollRect.content.TryGetComponent<GridLayoutGroup>(out var layout))
            {
                elementSize = scrollRect.vertical ? layout.cellSize.y : layout.cellSize.x;
            }
            else
            {
                elementSize = scrollRect.vertical ? template.RectTransform.rect.height : template.RectTransform.rect.width;
            }
        }

        public void InitializeScrollView()
        {
            initialized = true;
            lastElementNumber = -1;

            ResetPosition();
            UpdateContent();
            UpdateActiveItems();
        }

        public virtual void Initialize(List<TItemData> datas)
        {
            datas ??= new List<TItemData>();
            itemDatas = datas;

            InitializeScrollView();
        }

        #endregion

        #region "abstract Method"

        protected abstract void UpdateContent();
        protected abstract void UpdateSpaceElement(float size);
        protected abstract void UpdateItem(ScrollDirection direction, int itemNumber, bool updateAll);

        #endregion

        #region "Virtual Method"

        public virtual void UpdateActiveItems()
        {
            for(int i = 0; i < activatingItems.Count; i++)
            {
                var currentItem = activatingItems[i];
                var currentData = itemDatas[lastElementNumber + i];
                currentItem.Data = currentData;
            }
        }

        public virtual void ResetActivatingItems()
        {
            activatingItems.ForEach(item => itemPool.Return(item));
            activatingItems.Clear();
        }

        protected virtual TItem CreateNewItem(int itemNumber)
        {
            var newItem = itemPool.GetNextItem();
            newItem.transform.SetParent(scrollRect.content, false);
            newItem.transform.SetSiblingIndex(itemNumber);
            newItem.Data = itemDatas[itemNumber];

            return newItem;
        }

        protected virtual void ResetPosition()
        {
            if(scrollRect == null)
            {
                Awake();
            }

            if(scrollRect.vertical)
            {
                scrollRect.verticalNormalizedPosition = 1.0f;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = 0.0f;
            }
        }

        #endregion

        #region "Protected Method"

        protected void InitializeItems(int showCount, int itemNumber)
        {
            ResetActivatingItems();

            for(int i = 0; i < showCount && i + itemNumber < ItemDataCount(); i++)
            {
                activatingItems.Add(CreateNewItem(i + itemNumber));
            }
        }

        protected void AdjustContentSize(float size)
        {
            var currentSize = scrollRect.content.sizeDelta;
            size -= layoutSpacing;

            if(padding != null)
            {
                size += scrollRect.vertical ? (padding.top + padding.bottom) : (padding.left + padding.right);
            }

            if(scrollRect.vertical)
            {
                currentSize.y = size;
            }
            else
            {
                currentSize.x = size;
            }

            scrollRect.content.sizeDelta = currentSize;
        }

        #endregion

        #region "Get Value"

        protected float GetScrollAreaSize(RectTransform viewPort) => scrollRect.vertical ? viewPort.rect.height : viewPort.rect.width;

        protected float GetScrollRectNormalizedPosition() => Mathf.Clamp01(scrollRect.vertical ? 1 - scrollRect.verticalNormalizedPosition : scrollRect.horizontalNormalizedPosition);

        #endregion

        #region "ScrollView Method"

        protected static LayoutElement CreateSpaceItem(ScrollRect scrollRect, float itemSize)
        {
            var spaceItem = new GameObject("SpaceItem").AddComponent<LayoutElement>();

            if(scrollRect.vertical)
            {
                spaceItem.minHeight = itemSize;
            }
            else
            {
                spaceItem.minWidth = itemSize;
            }

            return spaceItem;
        }

        private void ScrollMoved(Vector2 delta)
        {
            UpdateContent();
        }

        #endregion
    }

    public enum ScrollDirection
    {
        TopToBottom,
        BottomToTop
    }
}