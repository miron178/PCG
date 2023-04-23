using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class OutlineTests
{
    [Test]
    public void StartVertex()
    {
        {
            var shape = new LinkedList<Vector2>();
            shape.AddLast(new Vector2(0, 0));
            shape.AddLast(new Vector2(1, 1));
            shape.AddLast(new Vector2(2, 0));
            var start = Outline.StartVertex(shape);
            Assert.AreEqual(start.Value, new Vector2(0, 0));
        }
        {
            var shape = new LinkedList<Vector2>();
            shape.AddLast(new Vector2(0, 0));
            shape.AddLast(new Vector2(2, 0));
            shape.AddLast(new Vector2(1, 1));
            var start = Outline.StartVertex(shape);
            Assert.AreEqual(start.Value, new Vector2(0, 0));
        }
        {
            var shape = new LinkedList<Vector2>();
            shape.AddLast(new Vector2(1, 1));
            shape.AddLast(new Vector2(0, 0));
            shape.AddLast(new Vector2(2, 0));
            var start = Outline.StartVertex(shape);
            Assert.AreEqual(start.Value, new Vector2(0, 0));
        }
        {
            var shape = new LinkedList<Vector2>();
            shape.AddLast(new Vector2(1, 1));
            shape.AddLast(new Vector2(2, 0));
            shape.AddLast(new Vector2(0, 0));
            var start = Outline.StartVertex(shape);
            Assert.AreEqual(start.Value, new Vector2(0, 0));
        }
        {
            var shape = new LinkedList<Vector2>();
            shape.AddLast(new Vector2(2, 0));
            shape.AddLast(new Vector2(0, 0));
            shape.AddLast(new Vector2(1, 1));
            var start = Outline.StartVertex(shape);
            Assert.AreEqual(start.Value, new Vector2(0, 0));
        }
        {
            var shape = new LinkedList<Vector2>();
            shape.AddLast(new Vector2(2, 0));
            shape.AddLast(new Vector2(1, 1));
            shape.AddLast(new Vector2(0, 0));
            var start = Outline.StartVertex(shape);
            Assert.AreEqual(start.Value, new Vector2(0, 0));
        }
    }

    [Test]
    public void NextVertex()
    {
        var shape = new LinkedList<Vector2>();
        shape.AddLast(new Vector2(0, 0));
        shape.AddLast(new Vector2(1, 1));
        shape.AddLast(new Vector2(2, 2));
        var start = Outline.StartVertex(shape);
        var next = Outline.NextVertex(start);
        Assert.AreEqual(next.Value, new Vector2(1, 1));
        next = Outline.NextVertex(next);
        Assert.AreEqual(next.Value, new Vector2(2, 2));
        next = Outline.NextVertex(next);
        Assert.AreEqual(next.Value, new Vector2(0, 0));
        next = Outline.NextVertex(next);
        Assert.AreEqual(next.Value, new Vector2(1, 1));
        next = Outline.NextVertex(next);
        Assert.AreEqual(next.Value, new Vector2(2, 2));
    }

    [Test]
    public void InsertVertexStart()
    {
        var shape = new LinkedList<Vector2>();
        shape.AddLast(new Vector2(0, 0));
        var start = shape.AddLast(new Vector2(1, 1));
        var end = shape.AddLast(new Vector2(2, 2));

        var inserted = Outline.InsertVertex(start, new Vector2(1, 1));

        Assert.AreEqual(shape.Count, 3);
        Assert.AreEqual(start, inserted);
    }

    [Test]
    public void InsertVertexEnd()
    {
        var shape = new LinkedList<Vector2>();
        shape.AddLast(new Vector2(0, 0));
        var start = shape.AddLast(new Vector2(1, 1));
        var end = shape.AddLast(new Vector2(2, 2));

        var inserted = Outline.InsertVertex(start, new Vector2(2, 2));

        Assert.AreEqual(shape.Count, 3);
        Assert.AreEqual(end, inserted);
    }

    [Test]
    public void InsertVertexBetween()
    {
        var shape = new LinkedList<Vector2>();
        shape.AddLast(new Vector2(0, 0));
        var start = shape.AddLast(new Vector2(1, 1));
        var end = shape.AddLast(new Vector2(2, 2));

        var inserted = Outline.InsertVertex(start, new Vector2(3, 3));

        Assert.AreEqual(shape.Count, 4);
        Assert.AreEqual(inserted.Value, new Vector2(3, 3));
        Assert.AreEqual(inserted.Previous, start);
        Assert.AreEqual(inserted.Next, end);
    }

    [Test]
    public void AddNeartestIntersetion()
    {
        var shape = new LinkedList<Vector2>();
        var one = shape.AddLast(new Vector2(-10, 0));
        var two = shape.AddLast(new Vector2(0, 10));
        var three = shape.AddLast(new Vector2(10, 0));

        var A = new Vector2(5, -5);
        var B = new Vector2(5, 10);

        var ab = Outline.AddNeartestIntersetion(A, B, one);
        Assert.AreEqual(shape.Count, 4);
        Assert.AreEqual(ab.Value, new Vector2(5, 0));
        Assert.AreEqual(Outline.NextVertex(one), two);
        Assert.AreEqual(Outline.NextVertex(two), three);
        Assert.AreEqual(Outline.NextVertex(three), ab);
        Assert.AreEqual(Outline.NextVertex(ab), one);

        var ba = Outline.AddNeartestIntersetion(B, A, one);
        Assert.AreEqual(shape.Count, 5);
        Assert.AreEqual(ba.Value, new Vector2(5, 5));
        Assert.AreEqual(Outline.NextVertex(one), two);
        Assert.AreEqual(Outline.NextVertex(two), ba);
        Assert.AreEqual(Outline.NextVertex(ba), three);
        Assert.AreEqual(Outline.NextVertex(three), ab);
        Assert.AreEqual(Outline.NextVertex(ab), one);

        var baAgain = Outline.AddNeartestIntersetion(B, A, three);
        Assert.AreEqual(shape.Count, 5);
        Assert.AreEqual(baAgain, ba);
    }

    [Test]
    public void AddSeparate()
    {
        var shapeA = new LinkedList<Vector2>();
        shapeA.AddLast(new Vector2(0, 0));
        shapeA.AddLast(new Vector2(1, 0));
        shapeA.AddLast(new Vector2(1, 1));

        var shapeB = new LinkedList<Vector2>();
        shapeB.AddLast(new Vector2(2, 0));
        shapeB.AddLast(new Vector2(3, 0));
        shapeB.AddLast(new Vector2(3, 1));

        LinkedList<Vector2> expected = new LinkedList<Vector2>();
        expected.AddLast(new Vector2(0, 0));
        expected.AddLast(new Vector2(1, 0));
        expected.AddLast(new Vector2(1, 1));
        expected.AddLast(new Vector2(2, 0));
        expected.AddLast(new Vector2(3, 0));
        expected.AddLast(new Vector2(3, 1));

        {
            var shapeAB = Outline.Add(shapeA, shapeB);
            Assert.AreEqual(shapeAB, expected);
        }

        {
            var shapeBA = Outline.Add(shapeB, shapeA);
            Assert.AreEqual(shapeBA, expected);
        }
    }

    [Test]
    public void AddInside()
    {
        var shapeA = new LinkedList<Vector2>();
        shapeA.AddLast(new Vector2(-2, -2));
        shapeA.AddLast(new Vector2(-2, +2));
        shapeA.AddLast(new Vector2(+2, +2));
        shapeA.AddLast(new Vector2(+2, -2));

        var shapeB = new LinkedList<Vector2>();
        shapeB.AddLast(new Vector2(-1, -1));
        shapeB.AddLast(new Vector2(-1, +1));
        shapeB.AddLast(new Vector2(+1, +1));
        shapeB.AddLast(new Vector2(+1, -1));

        LinkedList<Vector2> expected = new LinkedList<Vector2>();
        expected.AddLast(new Vector2(-2, -2));
        expected.AddLast(new Vector2(-2, +2));
        expected.AddLast(new Vector2(+2, +2));
        expected.AddLast(new Vector2(+2, -2));

        {
            var shapeAB = Outline.Add(shapeA, shapeB);
            Assert.AreEqual(shapeAB, expected);
        }

        {
            var shapeBA = Outline.Add(shapeB, shapeA);
            Assert.AreEqual(shapeBA, expected);
        }
    }

    [Test]
    public void AddIntersecting()
    {
        var shapeA = new LinkedList<Vector2>();
        shapeA.AddLast(new Vector2(0, 1));
        shapeA.AddLast(new Vector2(2, -1));
        shapeA.AddLast(new Vector2(-2, -1));

        var shapeB = new LinkedList<Vector2>();
        shapeB.AddLast(new Vector2(-1, 2));
        shapeB.AddLast(new Vector2(1, 2));
        shapeB.AddLast(new Vector2(1, -2));
        shapeB.AddLast(new Vector2(-1, -2));

        LinkedList<Vector2> expected = new LinkedList<Vector2>();
        expected.AddLast(new Vector2(-2, -1));
        expected.AddLast(new Vector2(-1, 0));
        expected.AddLast(new Vector2(-1, 2));
        expected.AddLast(new Vector2(1, 2));
        expected.AddLast(new Vector2(1, 0));
        expected.AddLast(new Vector2(2, -1));
        expected.AddLast(new Vector2(1, -1));
        expected.AddLast(new Vector2(1, -2));
        expected.AddLast(new Vector2(-1, -2));
        expected.AddLast(new Vector2(-1, -1));

        {
            var shapeAB = Outline.Add(shapeA, shapeB);
            Assert.AreEqual(shapeAB, expected);
        }
        {
            var shapeBA = Outline.Add(shapeB, shapeA);
            Assert.AreEqual(shapeBA, expected);
        }
    }
}