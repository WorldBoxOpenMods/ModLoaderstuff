using System.Collections;

namespace NeoModLoader.utils.Lists;
/// <summary>
/// a simple wrapper for non-thread safe sets and lists
/// </summary>
public class ConcurrentWrapper<T> : ISet<T>, IList<T>
{
    private readonly object _lock = new();
    /// <summary>
    /// the wrapped Collection
    /// </summary>
    public readonly ICollection<T> Collection;
    private ISet<T> _set;
    private IList<T> _list;
    /// <summary>
    /// the wrapped List
    /// </summary>
    public IList<T> List => _list ?? throw new NotSupportedException("the concurrent collection is not a List!");
    /// <summary>
    /// the wrapped Set
    /// </summary>
    public ISet<T> Set => _set ?? throw new NotSupportedException("the concurrent collection is not a Set!");
    /// <summary>
    /// creates a new concurrent list
    /// </summary>
    public ConcurrentWrapper() : this(new List<T>()){}
    /// <summary>
    /// wraps a collection
    /// </summary>
    public ConcurrentWrapper(ICollection<T> collection)
    {
        Collection = collection;
        _set = Collection as ISet<T>;
        _list = Collection as IList<T>;
    }
    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        List<T> temp;
        lock (_lock)
        {
            temp = new List<T>(Collection);
        }
        return temp.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    /// <inheritdoc/>
    public void Add(T item)
    {
        lock (_lock)
        {
            Collection.Add(item);
        }
    }
    /// <inheritdoc/>
    public void UnionWith(IEnumerable<T> other)
    {
        lock (_lock)
        {
             Set.UnionWith(other);
        }
    }
    /// <inheritdoc/>
    public void IntersectWith(IEnumerable<T> other)
    {
        lock (_lock)
        {
             Set.IntersectWith(other);
        }
    }
    /// <inheritdoc/>
    public void ExceptWith(IEnumerable<T> other)
    {
        lock (_lock)
        {
             Set.ExceptWith(other);
        }
    }
    /// <inheritdoc/>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        lock (_lock)
        {
             Set.SymmetricExceptWith(other);
        }
    }
    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        lock (_lock)
        {
            return Set.IsSubsetOf(other);
        }
    }
    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        lock (_lock)
        {
            return Set.IsSupersetOf(other);
        }
    }
    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        lock (_lock)
        {
            return Set.IsProperSupersetOf(other);
        }
    }
    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        lock (_lock)
        {
            return Set.IsProperSubsetOf(other);
        }
    }
    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<T> other)
    {
        lock (_lock)
        {
            return Set.Overlaps(other);
        }
    }
    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<T> other)
    {
        lock (_lock)
        {
            return Set.SetEquals(other);
        }
    }

    bool ISet<T>.Add(T item)
    {
        lock (_lock)
        {
           return Set.Add(item);
        }
    }
    /// <inheritdoc/>
    public void Clear()
    {
        lock (_lock)
        {
            Collection.Clear();
        }
    }
    /// <inheritdoc/>
    public bool Contains(T item)
    {
        lock (_lock)
        {
            return Collection.Contains(item);
        }
    }
    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_lock)
        {
             Collection.CopyTo(array, arrayIndex);
        }
    }
    /// <inheritdoc/>
    public bool Remove(T item)
    {
        lock (_lock)
        {
            return Collection.Remove(item);
        }
    }
    /// <inheritdoc/>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return Collection.Count;
            }
        }
    }
    /// <inheritdoc/>
    public bool IsReadOnly
    {
        get
        {
            lock (_lock)
            {
                return Collection.IsReadOnly;
            }
        }
    }
    /// <inheritdoc/>
    public int IndexOf(T item)
    {
        lock (_lock)
        {
            return List.IndexOf(item);
        }
    }
    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        lock (_lock)
        {
             List.Insert(index, item);
        }
    }
    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        lock (_lock)
        {
             List.RemoveAt(index);
        }
    }
    /// <inheritdoc/>
    public T this[int index]
    {
        get
        {
            lock (_lock)
            {
                return List[index];
            }
        }
        set
        {
            lock (_lock)
            {
                 List[index] = value;
            }
        }
    }
}