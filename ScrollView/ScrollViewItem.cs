using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    public abstract class ScrollViewItem<T> : MonoBehaviour
    {
        private RectTransform rectTransform;
        public RectTransform RectTransform 
        {
            get
            {
                if(rectTransform == null)
                {
                    rectTransform = GetComponent<RectTransform>();
                }

                return rectTransform ??= GetComponent<RectTransform>();
            }
        }

        public abstract T Data { get; set; }
        public abstract void SetUp(T data);
        public abstract void UpdateData();
    }
}