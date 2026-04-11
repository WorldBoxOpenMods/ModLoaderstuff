using System.Collections;
namespace NeoModLoader.utils.Collections;
/// <summary>
/// a list of slots which can contain a variable
/// </summary>
/// <remarks>thread safe</remarks>
public sealed class SlotList<T> : IList<T>
{
    private readonly Dictionary<int, T>   _data     = new();
    private readonly Dictionary<int, int> _position = new(); 
    private readonly Stack<int>           _free     = new();

    private int[] _active = new int[8];  
    private int   _count  = 0;
    private int   _next   = 0;           

    private readonly object _lock = new();
    
    public T GetRandom(bool IncludeEmptySlots = false)
    {
        if (IncludeEmptySlots)
        {
            lock (_lock)
            {
                int candidate = Randy.randomInt(0, _next);
                return InternalGet(candidate);
            }
        }
        
        var snap = Volatile.Read(ref _active);
        int c    = Volatile.Read(ref _count);

        if (c == 0) throw new InvalidOperationException("Collection is empty.");
        
        lock (_lock)
        {
            int candidate = snap[Randy.randomInt(0, c)];
            if (_data.TryGetValue(candidate, out var value))
                return value;

            // candidate was removed in the race window — fall back to a safe read
            return _count == 0 ? throw new InvalidOperationException("Collection is empty.") : _data[_active[Randy.randomInt(0, _count)]];
        }
    }

    public T? Get(int index)
    {
        lock (_lock)
        {
            return InternalGet(index);
        }
    }
    T? InternalGet(int index)
    {
        if (_data.TryGetValue(index, out var value))
        {
            return value;
        }
        return default;
    }
    public int Add(T value)
    {
        lock (_lock)
        {
            int index = _free.Count > 0 ? _free.Pop() : _next++;
            InsertInternal(index, value);
            return index;
        }
    }
    public void Set(int index, T value)
    {
        lock (_lock)
        {
            InternalSet(index, value);
        }
    }
    void InternalSet(int index, T value)
    {
        if (_data.ContainsKey(index))
        {
            _data[index] = value;
        }
        else
        {
            if (index >= _next) _next = index + 1;

            InsertInternal(index, value);
        }
    }
    public bool Remove(int index)
    {
        lock (_lock)
        {
            if (!_data.ContainsKey(index)) return false;
            RemoveInternal(index);
            return true;
        }
    }
    private void InsertInternal(int index, T value)
    {
        _data[index] = value;
        EnsureCapacity(_count + 1);

        _active[_count]  = index;
        _position[index] = _count;
        _count++;
    }

    private void RemoveInternal(int index)
    {
        int pos       = _position[index];
        int lastIndex = _active[_count - 1];

        _active[pos]        = lastIndex;
        _position[lastIndex] = pos;
        _count--;

        _data.Remove(index);
        _position.Remove(index);
        _free.Push(index);
    }

    private void EnsureCapacity(int required)
    {
        if (required <= _active.Length) return;

        int newSize = _active.Length;
        while (newSize < required) newSize *= 2;

        var next = new int[newSize];
        Array.Copy(_active, next, _count);
        Volatile.Write(ref _active, next);
    }

    public void RemoveAt(int index)
    {
        Remove(index);
    }

    public T this[int key]
    {
        get => Get(key);
        set => Set(key, value);
    }
    /// <summary>
    /// removes the first instance of the item
    /// </summary>
    public bool Remove(T item)
    {
        lock (_lock)
        {
            foreach (var pair in _data)
            {
                if (Equals(pair.Value, item))
                {
                    RemoveInternal(pair.Key);
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// swaps two slots, if the second slot is empty, it just moves the first item
    /// </summary>
    /// <returns>true if the first slot is not empty</returns>
    public bool Swap(int index, int newindex)
    {
        lock (_lock)
        {
            if (!_data.TryGetValue(index, out var aVal))
            {
                return false;
            }
          
            bool newExists = _data.ContainsKey(newindex);
            _data.TryGetValue(newindex, out var bVal);

            if (!newExists)
            {
                RemoveInternal(index);
                InsertInternal(newindex, aVal!);
            }
            else
            {
                _data[index]    = bVal!;
                _data[newindex] = aVal!;
            }
            return true;
        }
    }
    /// <summary>
    /// inserts an item into the slot. if the slot is full, the previous item will be pushed to the next slot
    /// </summary>
    public void Insert(int index, T item)
    {
        lock (_lock)
        {
            if (_data.TryGetValue(index, out var existing))
            {
                int newIndex = _free.Count > 0 ? _free.Pop() : _next++;
                InsertInternal(newIndex, existing);
            }
            InternalSet(index, item); 
        }
    }
    public int IndexOf(T item)
    {
        lock (_lock)
        {
            foreach (var pair in _data)
            {
                if (Equals(pair.Value, item))
                {
                    return pair.Key;
                }
            }
        }
        return -1;
    }

    public int Count { get { lock (_lock) return _next; } }

    public bool IsReadOnly => false;

    public ICollection<int> Slots
    {
        get { lock (_lock) return _data.Keys.ToList(); }
    }

    public ICollection<T> Values
    {
        get { lock (_lock) return _data.Values.ToList(); }
    }
    
    public bool IsSlotFull(int key)
    {
        lock (_lock) return _data.ContainsKey(key);
    }

    public bool TryReadSlot(int key, out T value)
    {
        lock (_lock) return _data.TryGetValue(key, out value);
    }
    public IEnumerator<T> GetEnumerator()
    {
        int next;
        lock (_lock) next = _next;

        for (int i = 0; i < next; i++)
            yield return Get(i); 
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _data.Clear();
            _position.Clear();
            _free.Clear();
            _active = new int[8];
            _count  = 0;
            _next   = 0;
        }
    }

    public bool Contains(T item)
    {
        lock (_lock)
        {
            return _data.Values.Contains(item);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_lock)
            foreach (var kv in _data)
                array[arrayIndex++] = kv.Value;
    }
}