using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarCloudgamesLibrary
{
    public class ScrollViewPool<T> : IDisposable where T : Component
    {
        private T itemPrefab;
        private Transform parentObject;
        private Transform poolRoot;
        private Queue<T> itemQueue;

        public int ItemCount() => itemQueue.Count;

        public ScrollViewPool(T _itemPrefab, Transform _parentObject, int poolCount)
        {
            itemPrefab = _itemPrefab;
            parentObject = _parentObject;
            itemQueue = new Queue<T>(poolCount);

            poolRoot = new GameObject($"{itemPrefab.GetType().Name} Pool").transform;
            poolRoot.SetParent(parentObject, false);

            for(int i = 0; i < poolCount; i++)
            {
                var newItem = Object.Instantiate(itemPrefab, poolRoot);
                newItem.gameObject.SetActive(false);

                itemQueue.Enqueue(newItem);
            }
        }

        public T GetNextItem()
        {
            if(itemQueue.Count > 0)
            {
                var dequeuedItem = itemQueue.Dequeue();
                dequeuedItem.transform.SetParent(parentObject, false);
                dequeuedItem.gameObject.SetActive(true);

                return dequeuedItem;
            }

            return Object.Instantiate(itemPrefab, parentObject);
        }

        public void Return(T item)
        {
            item.gameObject.SetActive(false);
            item.transform.position = Vector3.zero;
            item.transform.localEulerAngles = Vector3.zero;
            item.transform.SetParent(poolRoot, false);

            itemQueue.Enqueue(item);
        }

        public void Dispose()
        {
            foreach(var item in itemQueue)
            {
                Object.Destroy(item.gameObject);
            }

            Object.Destroy(poolRoot);

            itemPrefab = null;
            poolRoot = null;
            parentObject = null;
            itemQueue = null;
        }
    }
}