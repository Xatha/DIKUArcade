#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DIKUArcade.DataStructures;

/// <summary>
/// Double buffered list that sacrifices memory for speed and less GC pressure. Supports parallel iteration. It is not thread safe.
/// </summary>
/// <remarks>The list is implemented as a dynamic array. The memory usage is twice a normal list, since it keeps two equally sized buffers at all times.</remarks>
/// <typeparam name="T">The type of the elements in the list.</typeparam>
public class DoubleBufferedList<T> : ICollection<T>
{
    private const int GROWTH_FACTOR = 2;
    
    public int Count { get; private set; }
    public bool IsReadOnly { get; }
    public uint Capacity { get; private set; }

    private T[] primaryBuffer;
    private T[] secondaryBuffer;

    public DoubleBufferedList(uint size)
    {
        if (size == 0)
        {
            throw new ArgumentException("Size cannot be 0.");
        }
        
        Capacity = size;
        IsReadOnly = false;
        primaryBuffer = new T[Capacity];
        secondaryBuffer = new T[Capacity];
    }
    
    public T this[int i] => primaryBuffer[i];

    /// <summary>
    /// Method that maps T and returns a boolean value indicating if T should be deleted from the collection.
    /// </summary>
    public delegate bool MutableMapper(T item);
    
    /// <summary>
    /// Method that maps T, does not support deletion. 
    /// </summary>
    public delegate void ImmutableMapper(T item);

    /// <summary>
    /// Iterates through the collection in parallel and applies the given mapper to every item.
    /// It is very important that the mapper does not mutate the collection in destructive way, since this will cause undefined behavior.
    /// </summary>
    /// <remarks>The mapper is called in parallel, so the order of the items in the collection is not guaranteed and any closures used in the mapper needs to be thread-safe.
    /// It is not recommended to use this method if the count is low (&lt; 10000). Also beware that is method does allocate some memory (SOH) which incurs some GC pressure.</remarks>
    /// <param name="mapper">The delegate method which is called on every item in the list.</param>
    public void ParallelImmutableIterator(ImmutableMapper mapper)
    {
        Parallel.For(0, Count, (i) =>
            {
                mapper(primaryBuffer[i]);
            }
        );
    }

    /// <summary>
    /// Iterates through the collection sequentially and mutates every item with the given mapper.
    /// If the mapper returns true, the item is removed from the collection.
    /// </summary>
    /// <remarks>For a very large list (&gt; 10000 objects), consider using <see cref="DoubleBufferedList{T}.ParallelMutatingIterator"/>.</remarks>
    /// <param name="mapper">The delegate method which is called on every item in the list.</param>
    public void MutatingIterator(MutableMapper mapper)
    {
        int secondaryBufferCount = 0;
        for (int i = 0; i < Count; i++)
        {
            if (mapper(primaryBuffer[i]))
            {
                secondaryBuffer[secondaryBufferCount++] = primaryBuffer[i];
            }
        }
        Swap();
        Count = secondaryBufferCount;
    }
    
    private void Swap()
    {
        (primaryBuffer, secondaryBuffer) = (secondaryBuffer, primaryBuffer);
        Array.Clear(secondaryBuffer, 0, Count);
    }

    private void Resize(uint newCapacity)
    {
        if (newCapacity > Array.MaxLength)
        {
            newCapacity = (uint)Array.MaxLength;
        }
        
        Capacity = newCapacity;
        T[] newBuffer = new T[Capacity];
        Array.Copy(primaryBuffer, newBuffer, Count);
        //Array.Resize(ref primaryBuffer, (int)Capacity);
        
        primaryBuffer = newBuffer;
        
        T[] newSecondaryBuffer = new T[Capacity];
        secondaryBuffer = newSecondaryBuffer;
    }

    #region ICollection<T>
    
    public void Add(T item)
    {
        if (Count < Capacity)
        {
            primaryBuffer[Count++] = item;
        }
        else
        {
            int newCapacity = (int)(Capacity * GROWTH_FACTOR);
            
            if (newCapacity % 2 != 0)
            {
                newCapacity++;
            }

            Resize((uint)newCapacity);
            primaryBuffer[Count++] = item;
        }
    }
    
    public bool Remove(T item)
    {
        int index = Array.IndexOf(primaryBuffer, item, 0, Count);
        
        if (index < 0)
        {
            return false;
        }
        
        if ((uint)index >= (uint)Count)
        {
            return false;
        }
        Count--;
        
        if (index < Count)
        {
            Array.Copy(primaryBuffer, index + 1, primaryBuffer, index, Count - index);
        }
        // If T is a reference, we have to clear it, so it can be collected by the GC.
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            primaryBuffer[Count] = default!;
        }
        return true;
    }
    
    public void Clear()
    {
        Array.Clear(primaryBuffer, 0, Count);
    }

    public bool Contains(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (primaryBuffer[i]?.Equals(item) ?? false)
            {
                return true;
            }
        }

        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(primaryBuffer, 0, array, arrayIndex, Count);
    }

    public IEnumerator<T> GetEnumerator()
    {
        ArraySegment<T> slice = new ArraySegment<T>(primaryBuffer, 0, Count);

        return slice.GetEnumerator();
        
        /*for (var index = 0; index < slice.Count; index++)
        {
            yield return slice[index];
        }*/
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public struct DoubleBufferedListEnum : IEnumerator<T>, IEnumerator
    {
        private T[] buffer;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        private int position = -1;

        public DoubleBufferedListEnum(T[] array)
        {
            buffer = array;
        }

        public bool MoveNext()
        {
            position++;
            
            if (position >= buffer.Length || buffer[position] == null)
            {
                return false;
            }
            
            return true;
        }

        public void Reset()
        {
            position = -1;
        }

        public T Current => buffer[position];
        
        object? IEnumerator.Current => Current;

        public void Dispose()
        {
            
        }
    }
    
    #endregion
}