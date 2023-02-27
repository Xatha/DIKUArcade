#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DIKUArcade.Entities;

public class DoubleBufferedCollection<T> : ICollection<T>
{
    public int Count { get; private set; }
    public bool IsReadOnly { get; }
    public uint Capacity { get; private set; }    

    private T[] primaryBuffer;
    private T[] secondaryBuffer;
    
    public T this[int i] => primaryBuffer[i];

    public DoubleBufferedCollection(uint size)
    {
        Capacity = size;
        primaryBuffer = new T[Capacity];
        secondaryBuffer = new T[Capacity];
    }

    public void Add(T item)
    {
        if (Count < Capacity)
        {
            primaryBuffer[Count++] = item;
        }
        else
        {
            int newCapacity = Count * 2;
            Resize((uint)newCapacity);
            primaryBuffer[Count++] = item;
        }
    }
    
    
    public delegate bool Mapper(T item);

    /// <summary>
    /// Maps T from the primary buffer to the secondary buffer - then swaps the buffers and clears any dangling data.
    /// </summary>
    public void MapToBuffer(Mapper mapper)
    {
        // Use type parameter to make subtotal a long, not an int
        Parallel.For<int>(0, Count, () => 0, (i, loop, secondaryBufferCount) =>
            {
                var entity = primaryBuffer[i];
                if (entity is null) return secondaryBufferCount;
            
                if (mapper(entity))
                {
                    secondaryBuffer[secondaryBufferCount] = entity;
                    secondaryBufferCount++;
                }

                return secondaryBufferCount;
            },
            (_) => { }
        );

            /*
    Parallel.For(0, Count, i =>
    {
        var entity = primaryBuffer[i];
        if (entity is null) return;
        
        if (mapper(entity))
        {
            secondaryBuffer[secondaryBufferCount] = entity;
            Interlocked.Increment(ref secondaryBufferCount);
        }
    });
    */
        
        /*
        for (int i = 0; i < Count; i++)
        {
            var shouldBeMapped = mapper(primaryBuffer[i]);
            if (shouldBeMapped)
            {
                secondaryBuffer[secondaryBufferCount++] = primaryBuffer[i];
            }
        }*/

        Swap();
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
        
        primaryBuffer = newBuffer;
        
        T[] newSecondaryBuffer = new T[Capacity];
        secondaryBuffer = newSecondaryBuffer;
    }

    public void Clear()
    {
        throw new System.NotImplementedException();
    }

    public bool Contains(T item)
    {
        throw new System.NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new System.NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}