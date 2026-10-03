using System;

namespace CCLBStudio.EventBus
{
    /// <summary>
    /// Array-backed, ordered collection that can be safely modified while being iterated.
    /// Removals during an iteration only null the slot; compaction is deferred until the outermost iteration ends.
    /// </summary>
    internal sealed class ListenerCollection<TItem> where TItem : class
    {
        private const int DefaultCapacity = 4;

        private TItem[] _items = Array.Empty<TItem>();
        private int _count;
        private int _iterationDepth;
        private bool _requireCompaction;

        internal TItem[] Items => _items;
        internal int Count => _count;

        internal bool Add(TItem item)
        {
            if (item == null || IndexOf(item) >= 0)
            {
                return false;
            }

            if (_count == _items.Length)
            {
                Array.Resize(ref _items, _items.Length == 0 ? DefaultCapacity : _items.Length * 2);
            }

            _items[_count++] = item;
            return true;
        }

        internal bool Remove(TItem item)
        {
            if (item == null)
            {
                return false;
            }

            int index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        internal void RemoveAt(int index)
        {
            if (_iterationDepth > 0)
            {
                _items[index] = null;
                _requireCompaction = true;
                return;
            }

            _count--;
            if (index < _count)
            {
                Array.Copy(_items, index + 1, _items, index, _count - index);
            }

            _items[_count] = null;
        }

        internal bool Contains(TItem item)
        {
            return item != null && IndexOf(item) >= 0;
        }

        /// <summary>
        /// Starts an iteration and returns the number of items to iterate.
        /// Items added during the iteration are not visited by it.
        /// </summary>
        internal int BeginIteration()
        {
            _iterationDepth++;
            return _count;
        }

        internal void EndIteration()
        {
            if (--_iterationDepth > 0 || !_requireCompaction)
            {
                return;
            }

            Compact();
        }

        internal void Clear()
        {
            Array.Clear(_items, 0, _count);
            _count = 0;
            _requireCompaction = false;
        }

        private int IndexOf(TItem item)
        {
            for (int i = 0; i < _count; i++)
            {
                TItem current = _items[i];
                if (ReferenceEquals(current, item) || (current != null && current.Equals(item)))
                {
                    return i;
                }
            }

            return -1;
        }

        private void Compact()
        {
            int write = 0;
            for (int read = 0; read < _count; read++)
            {
                if (_items[read] != null)
                {
                    _items[write++] = _items[read];
                }
            }

            Array.Clear(_items, write, _count - write);
            _count = write;
            _requireCompaction = false;
        }
    }
}
