using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DIKUArcade.DataStructures;
using NUnit.Framework;

namespace DIKUArcadeUnitTests.DataStructureTests;

[TestFixture]
public class DoubleBufferedListTest
{
    [Test]
    public void TestConstructionWhenTIsValueType()
    {
        DoubleBufferedList<int> list = new DoubleBufferedList<int>(2);
        List<int> expected = new List<int> { 1, 2, 5, 6 };
        
        Assert.AreEqual(0, list.Count);
        Assert.AreEqual(2, list.Capacity);
        
        list.Add(1);
        list.Add(2);
        
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual(2, list.Capacity);
        
        list.Add(5);
        list.Add(6);
        
        Assert.AreEqual(4, list.Count);
        Assert.AreEqual(4, list.Capacity);
        
        //Test if the list is correctly ordered.
        Assert.That(list, Is.EqualTo(expected));
    }
    
    [Test]
    public void TestConstructionWhenTIsReferenceType()
    {
        var obj1 = new object();
        var obj2 = new object();
        var obj3 = new object();
        var obj4 = new object();

        DoubleBufferedList<object> list = new DoubleBufferedList<object>(2);
        List<object> expected = new List<object> { obj1, obj2, obj3, obj4 };
        
        Assert.AreEqual(0, list.Count);
        Assert.AreEqual(2, list.Capacity);
        
        list.Add(obj1);
        list.Add(obj2);
        
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual(2, list.Capacity);
        
        list.Add(obj3);
        list.Add(obj4);
        
        Assert.AreEqual(4, list.Count);
        Assert.AreEqual(4, list.Capacity);
        
        //Test if the list is correctly ordered.
        Assert.That(list, Is.EqualTo(expected));
    }

    [Test]
    public void TestRemove()
    {
        var obj1 = new object();
        var obj2 = new object();
        
        DoubleBufferedList<object> list = new DoubleBufferedList<object>(2);
        
        list.Add(obj1);
        list.Add(obj2);
        
        Assert.AreEqual(obj1, list[0]);
        
        list.Remove(obj1);
        
        Assert.AreEqual(1, list.Count);
        
        // obj2 is moved to index 0
        Assert.AreEqual(obj2, list[0]);
        Assert.That(list, Is.EqualTo(new List<object> { obj2 }));
    }

    [Test]
    [TestCase(0)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(100_000)]
    public void TestForEach(int size)
    {
        var list = new DoubleBufferedList<int>(100);
        for (int i = 0; i < size; i++)
        {
            list.Add(i);
        }
        
        Assert.AreEqual(size, list.Count);

        foreach (var num in list)
        {
            Assert.AreEqual(num, list[num]);
        }
    }
    
    [Test]
    [TestCase(0)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(100_000)]
    public void TestIndexing(int size)
    {
        var list = new DoubleBufferedList<int>(100);
        for (int i = 0; i < size; i++)
        {
            list.Add(i);
        }
        
        Assert.AreEqual(size, list.Count);

        for (int i = 0; i < size; i++)
        {
            Assert.AreEqual(i, list[i]);
        }
    }
    
    [Test]    
    [TestCase(0)]
    [TestCase(100)]
    [TestCase(1000)]
    public void TestContains(int size)
    {
        var list = new DoubleBufferedList<int>(100);
        for (int i = 0; i < size; i++)
        {
            list.Add(i);
        }
        
        Assert.AreEqual(size, list.Count);

        for (int i = 0; i < size; i++)
        {
            Assert.IsTrue(list.Contains(i));
        }
    }

    [Test]
    [TestCase(0)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(100_000)]
    public void TestMutatingIterator(int size)
    {
        var list = new DoubleBufferedList<int>(100);
        for (int i = 0; i < size; i++)
        {
            list.Add(i);
        }
        
        int itemsToRemove = size / 2;
        list.MutatingIterator((i) =>
        {
            if (itemsToRemove <= 0) return false;
            itemsToRemove--;
            return true;

        });
        
        Assert.AreEqual(size / 2, list.Count);
    }
    
    [Test]
    [TestCase(0)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(100_000)]
    public void TestImmutableIterator(int size)
    {
        var list = new DoubleBufferedList<int>(2);
        for (int i = 0; i < size; i++)
        {
            list.Add(i);
        }
        
        var concurrentBag = new ConcurrentBag<int>();
        list.ParallelImmutableIterator((i) =>
        {
            concurrentBag.Add(i);
        });
        
        var newList = concurrentBag.ToList();
        newList.Sort();

        Assert.AreEqual(size, list.Count);
        Assert.AreEqual(size, newList.Count);

        for (int i = 0; i < size; i++)
        {
            Assert.AreEqual(i, newList[i]);
        }
    }

}