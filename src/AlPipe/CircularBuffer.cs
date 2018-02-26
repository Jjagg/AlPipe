using System;
using System.Collections;
using System.Collections.Generic;

namespace AlPipe
{
    /// <summary>
    /// Circular buffer implementation.
    /// </summary>
    /// <typeparam name="T">Type of the data.</typeparam>
    public class CircularBuffer<T> : IEnumerable<T> where T : struct
    {
        private readonly T[] _buffer;

        /// <summary>
        /// Index of the buffer end.
        /// </summary>
        public int End { get; private set; }
        /// <summary>
        /// Index of the buffer start.
        /// </summary>
        public int Start { get; private set; }

        /// <summary>
        /// Create a new <see cref="CircularBuffer{T}"/> with the given capacity.
        /// </summary>
        /// <param name="capacity">Capacity of the buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="capacity"/> is less than 0.</exception>
        public CircularBuffer(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative.");
            _buffer = new T[capacity];
            End = capacity - 1;
        }

        /// <summary>
        /// Number of items in the buffer.
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Capacity of the buffer.
        /// </summary>
        public int Capacity => _buffer.Length;
        /// <summary>
        /// Number of free spaces in the buffer. Equal to <code>Capacity - Count</code>.
        /// </summary>
        public int Available => Capacity - Count;

        /// <summary>
        /// Add a number of items to the buffer. Overrides oldest items when full.
        /// </summary>
        /// <param name="items">List of items from which to add.</param>
        /// <param name="start">Start index.</param>
        public void Enqueue(IList<T> items, int start = 0)
        {
            var count = items.Count - start;
            Enqueue(items, start, count);
        }

        /// <summary>
        /// Add a number of items to the buffer. Overrides oldest items when full.
        /// </summary>
        /// <param name="items">Items from which to add.</param>
        /// <param name="count">Number of items to add. If more than the number of items, all items are enqueued.</param>
        public void Enqueue(IEnumerable<T> items, int count)
        {
            var realCount = 0;
            foreach (var item in items)
            {
                if (realCount >= count)
                    break;
                End = (End + 1) % Capacity;
                _buffer[End] = item;
                realCount++;
            }

            var overwritten = realCount - Available;
            if (overwritten > 0)
            {
                Start = (Start + overwritten) % Capacity;
                Count = Capacity;
            }
            else
                Count += realCount;
        }

        /// <summary>
        /// Add a number of items to the buffer. Overrides oldest items when full.
        /// </summary>
        /// <param name="items">List of items from which to add.</param>
        /// <param name="start">Start index.</param>
        /// <param name="count">Number of items to add.</param>
        public void Enqueue(IList<T> items, int start, int count)
        {
            for (var i = 0; i < count; i++)
            {
                End = (End + 1) % Capacity;
                _buffer[End] = items[start + i];
            }

            var overwritten = count - Available;
            if (overwritten > 0)
            {
                Start = (Start + overwritten) % Capacity;
                Count = Capacity;
            }
            else
                Count += count;
        }

        /// <summary>
        /// Add an item to the buffer. Overrides oldest item when full.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Enqueue(T item)
        {
            End = (End + 1) % Capacity;
            _buffer[End] = item;
            if (Available == 0)
                Start = (Start + 1) % Capacity;
            else
                Count++;
        }

        /// <summary>
        /// Remove the oldest item from the buffer.
        /// </summary>
        /// <returns>The removed item.</returns>
        /// <exception cref="InvalidOperationException">If the buffer is empty.</exception>
        public T Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("queue exhausted");

            var dequeued = _buffer[Start];
            _buffer[Start] = default;
            Start = (Start + 1) % Capacity;
            --Count;
            return dequeued;
        }

        /// <summary>
        /// Clear the buffer.
        /// </summary>
        public void Clear()
        {
            End = Capacity - 1;
            Start = 0;
            Count = 0;
        }

        /// <summary>
        /// Get or set the item at index <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of the item to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   If the index is less than <code>0</code> or larger than <see cref="Count"/>.
        /// </exception>
        public T this[int index]
        {
            get
            {
#if DEBUG
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
#endif

                return _buffer[(Start + index) % Capacity];
            }
            set
            {
#if DEBUG
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
#endif

                _buffer[(Start + index) % Capacity] = value;
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            if (Count == 0 || Capacity == 0)
                yield break;

            for (var i = 0; i < Count; ++i)
                yield return this[i];
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
