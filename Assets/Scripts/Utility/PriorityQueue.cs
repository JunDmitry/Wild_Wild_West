using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Utility
{
    public sealed class PriorityQueue<TValue, TPriority> : IEnumerable<TValue>
        where TPriority : IComparable
    {
        private readonly object _lock = new();

        private List<TValue> _values;
        private List<TPriority> _priorities;
        private IComparer<TPriority> _comparer;

        private int _count;

        public PriorityQueue() : this(1)
        { }

        public PriorityQueue(int capacity) : this(capacity, Comparer<TPriority>.Default)
        {
        }

        public PriorityQueue(IComparer<TPriority> comparer) : this(1, comparer)
        {
        }

        public PriorityQueue(Comparison<TPriority> comparison)
            : this(1, comparison)
        {
        }

        public PriorityQueue(int capacity, Comparison<TPriority> comparison)
            : this(capacity, Comparer<TPriority>.Create(comparison))
        {
        }

        public PriorityQueue(int capacity, IComparer<TPriority> comparer)
        {
            _values = new(capacity);
            _priorities = new(capacity);
            _comparer = comparer;
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _count;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _count = value;
                }
            }
        }

        public int Capacity
        {
            get
            {
                lock (_lock)
                {
                    return _values.Capacity;
                }
            }
        }

        public void Enqueue(TValue value, TPriority priority)
        {
            lock (_lock)
            {
                _values.Add(value);
                _priorities.Add(priority);

                Count++;
                ShiftUp(Count);
            }
        }

        public TValue DequeueFirst()
        {
            lock (_lock)
            {
                if (Count == 0)
                    throw new IndexOutOfRangeException($"Count element in {nameof(PriorityQueue<TValue, TPriority>)} is zero!");

                TValue result = _values[0];

                Count--;
                _values[0] = _values[Count];
                _priorities[0] = _priorities[Count];
                _values.RemoveAt(Count);
                _priorities.RemoveAt(Count);

                ShiftDown(0);

                return result;
            }
        }

        public TValue DequeueLast()
        {
            lock (_lock)
            {
                if (Count == 0)
                    throw new IndexOutOfRangeException($"Count element in {nameof(PriorityQueue<TValue, TPriority>)} is zero!");

                Count--;

                TValue result = _values[Count];
                _values.RemoveAt(Count);
                _priorities.RemoveAt(Count);

                return result;
            }
        }

        public (TValue, TPriority) PeekFirstPair()
        {
            return (PeekFirst(), _priorities[0]);
        }

        public (TValue, TPriority) PeekLastPair()
        {
            return (PeekLast(), _priorities[Count - 1]);
        }

        public TValue PeekFirst()
        {
            lock (_lock)
            {
                if (Count == 0)
                    throw new IndexOutOfRangeException($"Count element in {nameof(PriorityQueue<TValue, TPriority>)} is zero!");

                return _values[0];
            }
        }

        public TValue PeekLast()
        {
            lock (_lock)
            {
                if (Count == 0)
                    throw new IndexOutOfRangeException($"Count element in {nameof(PriorityQueue<TValue, TPriority>)} is zero!");

                return _values[Count - 1];
            }
        }

        public bool Remove(TValue value)
        {
            lock (_lock)
            {
                int index = _values.IndexOf(value);

                if (index < 0 || index >= Count)
                    return false;

                return RemoveAt(index);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _values.Clear();
                _priorities.Clear();
                _comparer = default;
                Count = 0;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            TValue[] snapshot;

            lock (_lock)
            {
                snapshot = new TValue[_values.Count];
                PriorityQueue<TValue, TPriority> values = Clone();

                for (int i = 0; i < _values.Count; i++)
                    snapshot[i] = values.DequeueFirst();
            }

            return ((IEnumerable<TValue>)snapshot).GetEnumerator();
        }

        public PriorityQueue<TValue, TPriority> Clone()
        {
            PriorityQueue<TValue, TPriority> copy = new(_values.Count, _comparer);

            for (int i = 0; i < _values.Count; i++)
                copy.Enqueue(_values[i], _priorities[i]);

            return copy;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool RemoveAt(int index)
        {
            Count--;
            _values[index] = _values[Count];
            _priorities[index] = _priorities[Count];

            _values.RemoveAt(Count);
            _priorities.RemoveAt(Count);

            ShiftUp(index);
            ShiftDown(index);

            return true;
        }

        private void ShiftUp(int i)
        {
            int parentIndex;

            while (i < Count && i > 0 && _comparer.Compare(_priorities[Parent(i)], _priorities[i]) < 0)
            {
                parentIndex = Parent(i);
                Swap(i, parentIndex);

                i = parentIndex;
            }
        }

        private void ShiftDown(int i)
        {
            int maxIndex = i;

            int left = LeftChild(i);
            int right = RightChild(i);

            if (left < Count && _comparer.Compare(_priorities[left], _priorities[maxIndex]) > 0)
                maxIndex = left;

            if (right < Count && _comparer.Compare(_priorities[right], _priorities[maxIndex]) > 0)
                maxIndex = right;

            if (i != maxIndex)
            {
                Swap(maxIndex, i);
                ShiftDown(maxIndex);
            }
        }

        private void Swap(int i, int j)
        {
            TValue tempItem = _values[j];
            TPriority tempPriority = _priorities[j];

            _values[j] = _values[i];
            _priorities[j] = _priorities[i];

            _values[i] = tempItem;
            _priorities[i] = tempPriority;
        }

        private static int Parent(int i)
        {
            return (i - 1) / 2;
        }

        private static int LeftChild(int i)
        {
            return ((2 * i) + 1);
        }

        private static int RightChild(int i)
        {
            return ((2 * i) + 2);
        }
    }
}