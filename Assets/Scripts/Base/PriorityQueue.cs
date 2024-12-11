using System;
using System.Collections.Generic;

// 优先队列(堆)的实现
namespace NaviPath
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        public List<T> list = null;
        public int Count { get => list.Count; }
        public PriorityQueue(int capacity = 4)
        {
            list = new List<T>(capacity);
        }

        /// <summary>
        /// 入队列
        /// </summary>
        public void Enqueue(T item)
        {
            list.Add(item);

            HeapfiyUp(list.Count - 1);
        }
        /// <summary>
        /// 出队列
        /// </summary>
        public T Dequeue()
        {
            if (list.Count == 0)
            {
                return default;
            }
            T item = list[0];
            int endIndex = list.Count - 1;
            list[0] = list[endIndex];
            list.RemoveAt(endIndex);
            --endIndex;
            HeapfiyDown(0, endIndex);

            return item;
        }

        public T Peek()
        {
            return list.Count > 0 ? list[0] : default;
        }
        public int IndexOf(T t)
        {
            return list.IndexOf(t);
        }
        public T RemoveAt(int rmvIndex)
        {
            if (list.Count <= rmvIndex)
            {
                return default;
            }
            T item = list[rmvIndex];
            int endIndex = list.Count - 1;
            list[rmvIndex] = list[endIndex];
            list.RemoveAt(endIndex);
            --endIndex;

            if (rmvIndex < endIndex)
            {
                int parentIndex = (rmvIndex - 1) / 2;
                if (parentIndex > 0 && list[rmvIndex].CompareTo(list[parentIndex]) < 0)
                {
                    HeapfiyUp(rmvIndex);
                }
                else
                {
                    HeapfiyDown(rmvIndex, endIndex);
                }
            }

            return item;
        }
        public T RemoveItem(T t)
        {
            int index = IndexOf(t);
            return index != -1 ? RemoveAt(index) : default;
        }

        public void Clear()
        {
            list.Clear();
        }
        public bool Contains(T t)
        {
            return list.Contains(t);
        }
        public bool IsEmpty()
        {
            return list.Count == 0;
        }
        public List<T> ToList()
        {
            return list;
        }
        public T[] ToArray()
        {
            return list.ToArray();
        }

        void HeapfiyUp(int childIndex)
        {
            int parentIndex = (childIndex - 1) / 2;
            while (childIndex > 0 && list[childIndex].CompareTo(list[parentIndex]) < 0)
            {
                Swap(childIndex, parentIndex);
                childIndex = parentIndex;
                parentIndex = (childIndex - 1) / 2;
            }
        }
        void HeapfiyDown(int topIndex, int endIndex)
        {
            while (true)
            {
                int minIndex = topIndex;
                int childIndex = topIndex * 2 + 1;
                if (childIndex <= endIndex && list[childIndex].CompareTo(list[topIndex]) < 0)
                    minIndex = childIndex;
                childIndex = topIndex * 2 + 2;
                if (childIndex <= endIndex && list[childIndex].CompareTo(list[minIndex]) < 0)
                    minIndex = childIndex;
                if (topIndex == minIndex) break;
                Swap(topIndex, minIndex);
                topIndex = minIndex;
            }
        }

        void Swap(int a, int b)
        {
            T temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }
    }
}
