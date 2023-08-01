using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ClipboardLinkSaver
{
    public class OrderedSet<T> : ICollection<T>
    {
        private readonly IDictionary<T, LinkedListNode<T>> m_Dictionary;
        private readonly LinkedList<T> m_LinkedList;

        public OrderedSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        public OrderedSet(IEqualityComparer<T> comparer)
        {
            m_Dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
            m_LinkedList = new LinkedList<T>();
        }

        public int Count
        {
            get { return m_Dictionary.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return m_Dictionary.IsReadOnly; }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            m_LinkedList.Clear();
            m_Dictionary.Clear();
        }

        public bool Remove(T item)
        {
            LinkedListNode<T> node;
            bool found = m_Dictionary.TryGetValue(item, out node);
            if (!found) return false;
            m_Dictionary.Remove(item);
            m_LinkedList.Remove(node);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_LinkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return m_Dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_LinkedList.CopyTo(array, arrayIndex);
        }

        public bool Add(T item)
        {
            if (m_Dictionary.ContainsKey(item)) return false;
            LinkedListNode<T> node = m_LinkedList.AddLast(item);
            m_Dictionary.Add(item, node);
            return true;
        }
    }

    public class OrderedSetExt<T> : OrderedSet<T>, ISet<T>
    {
        public OrderedSetExt()
        {
        }

        public OrderedSetExt(IEqualityComparer<T> comparer)
            : base(comparer)
        {
        }

        public OrderedSetExt(IEnumerable<T> collection)
            : this(collection, EqualityComparer<T>.Default)
        {
        }

        public OrderedSetExt(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }

        /// <summary>
        ///     Modifies the current set so that it contains all elements that are present in both the current set and in the
        ///     specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            foreach (T element in other)
            {
                Add(element);
            }
        }

        /// <summary>
        ///     Modifies the current set so that it contains only elements that are also in a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public void IntersectWith(IEnumerable<T> other)
        {
            foreach (T element in other)
            {
                if (Contains(element)) continue;
                Remove(element);
            }
        }

        /// <summary>
        ///     Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The collection of items to remove from the set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (T element in other)
            {
                Remove(element);
            }
        }

        /// <summary>
        ///     Modifies the current set so that it contains only elements that are present either in the current set or in the
        ///     specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            foreach (T element in other)
            {
                if (Contains(element))
                {
                    Remove(element);
                }
                else
                {
                    Add(element);
                }
            }
        }

        /// <summary>
        ///     Determines whether a set is a subset of a specified collection.
        /// </summary>
        /// <returns>
        ///     true if the current set is a subset of <paramref name="other" />; otherwise, false.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            var otherHashset = new HashSet<T>(other);
            return otherHashset.IsSupersetOf(this);
        }

        /// <summary>
        ///     Determines whether the current set is a superset of a specified collection.
        /// </summary>
        /// <returns>
        ///     true if the current set is a superset of <paramref name="other" />; otherwise, false.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return other.All(Contains);
        }

        /// <summary>
        ///     Determines whether the current set is a correct superset of a specified collection.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="T:System.Collections.Generic.ISet`1" /> object is a correct superset of
        ///     <paramref name="other" />; otherwise, false.
        /// </returns>
        /// <param name="other">The collection to compare to the current set. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            var otherHashset = new HashSet<T>(other);
            return otherHashset.IsProperSubsetOf(this);
        }

        /// <summary>
        ///     Determines whether the current set is a property (strict) subset of a specified collection.
        /// </summary>
        /// <returns>
        ///     true if the current set is a correct subset of <paramref name="other" />; otherwise, false.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            var otherHashset = new HashSet<T>(other);
            return otherHashset.IsProperSupersetOf(this);
        }

        /// <summary>
        ///     Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <returns>
        ///     true if the current set and <paramref name="other" /> share at least one common element; otherwise, false.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            if (Count == 0) return false;
            return other.Any(Contains);
        }

        /// <summary>
        ///     Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <returns>
        ///     true if the current set is equal to <paramref name="other" />; otherwise, false.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other" /> is null.</exception>
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException("other");
            var otherHashset = new HashSet<T>(other);
            return otherHashset.SetEquals(this);
        }
    }
}