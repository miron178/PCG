using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class IntersectionTests
{
    [Test]
    public void IsBetween()
    {
        Assert.IsFalse(Intersection.IsBetweenInclusive(0.9f, 1, 2));
        Assert.IsTrue(Intersection.IsBetweenInclusive(1.5f, 1, 2));
        Assert.IsFalse(Intersection.IsBetweenInclusive(3.0f, 1, 2));

        Assert.IsFalse(Intersection.IsBetweenInclusive(0.9f, 2, 1));
        Assert.IsTrue(Intersection.IsBetweenInclusive(1.5f, 2, 1));
        Assert.IsFalse(Intersection.IsBetweenInclusive(3.0f, 2, 1));
    }

    [Test]
    public void IsBetweenEdge()
    {
        Assert.IsTrue(Intersection.IsBetweenInclusive(1.0f, 1, 2));
        Assert.IsTrue(Intersection.IsBetweenInclusive(2.0f, 1, 2));

        Assert.IsTrue(Intersection.IsBetweenInclusive(1.0f, 2, 1));
        Assert.IsTrue(Intersection.IsBetweenInclusive(2.0f, 2, 1));

        Assert.IsFalse(Intersection.IsBetweenExclusive(1.0f, 1, 2));
        Assert.IsFalse(Intersection.IsBetweenExclusive(2.0f, 1, 2));

        Assert.IsFalse(Intersection.IsBetweenExclusive(1.0f, 2, 1));
        Assert.IsFalse(Intersection.IsBetweenExclusive(2.0f, 2, 1));
    }


    [Test]
    public void FindOverlapSeparate()
    {
        Assert.IsFalse(Intersection.FindOverlap(1, 2, 3, 4));
        Assert.IsFalse(Intersection.FindOverlap(2, 1, 3, 4));
        Assert.IsFalse(Intersection.FindOverlap(1, 2, 4, 3));
        Assert.IsFalse(Intersection.FindOverlap(2, 1, 4, 3));
    }

    [Test]
    public void FindOverlapTouching()
    {
        Assert.IsTrue(Intersection.FindOverlap(1, 2, 2, 4));
        Assert.IsTrue(Intersection.FindOverlap(2, 1, 2, 4));
        Assert.IsTrue(Intersection.FindOverlap(1, 3, 4, 3));
        Assert.IsTrue(Intersection.FindOverlap(3, 1, 4, 3));
    }

    [Test]
    public void FindOverlapPartial()
    {
        Assert.IsTrue(Intersection.FindOverlap(1, 3, 2, 4));
        Assert.IsTrue(Intersection.FindOverlap(3, 1, 2, 4));
        Assert.IsTrue(Intersection.FindOverlap(1, 3, 4, 2));
        Assert.IsTrue(Intersection.FindOverlap(3, 1, 4, 2));
    }

    [Test]
    public void FindOverlapFull()
    {
        Assert.IsTrue(Intersection.FindOverlap(-1, 4, 2, 3));
        Assert.IsTrue(Intersection.FindOverlap(5, 1, 2, 4));
        Assert.IsTrue(Intersection.FindOverlap(1, 3, 4, 0));
        Assert.IsTrue(Intersection.FindOverlap(4, 1, 3, 2));
    }

    [Test]
    public void Line()
    {
        float a, b;
        Assert.IsTrue(Intersection.Line(new Vector2(0, 0), new Vector2(1, 1), out a, out b));
        Assert.AreEqual(a, 1f);
        Assert.AreEqual(b, 0f);

        Assert.IsTrue(Intersection.Line(new Vector2(0, -1), new Vector2(1, 1), out a, out b));
        Assert.AreEqual(a, 2f);
        Assert.AreEqual(b, -1f);

        Assert.IsTrue(Intersection.Line(new Vector2(1, 1), new Vector2(0, -1), out a, out b));
        Assert.AreEqual(a, 2f);
        Assert.AreEqual(b, -1f);
    }

    [Test]
    public void LineVertical()
    {
        float a, b;
        Assert.IsFalse(Intersection.Line(new Vector2(-1, 0), new Vector2(-1, 1), out a, out b));
    }

    [Test]
    public void LineSamePoint()
    {
        float a, b;
        Assert.IsFalse(Intersection.Line(new Vector2(-1, -2), new Vector2(-1, -2), out a, out b));
    }
    [Test]
    public void FindIntersectionVerticalBoth()
    {
        Assert.AreEqual(Intersection.FindIntersection(
            new Vector2(1, 4),
            new Vector2(1, 2),
            new Vector2(1, -1),
            new Vector2(1, 1),
            out _), Intersection.Result.None);

        Assert.AreEqual(Intersection.FindIntersection(
            new Vector2(1, 4),
            new Vector2(1, 1),
            new Vector2(1, 3),
            new Vector2(1, 0),
            out _), Intersection.Result.Overlap);

        Assert.AreEqual(Intersection.FindIntersection(
            new Vector2(0, 4),
            new Vector2(0, 5),
            new Vector2(0, 4),
            new Vector2(0, 3),
            out _), Intersection.Result.Overlap);

        Assert.AreEqual(Intersection.FindIntersection(
            new Vector2(-1, 4),
            new Vector2(-1, 1),
            new Vector2(-1, -1),
            new Vector2(-1, 0),
            out _), Intersection.Result.None);
    }

    [Test]
    public void FindIntersectionVerticalOne()
    {
        Vector2 I;

        Assert.AreEqual(
            Intersection.FindIntersection(
                new Vector2(1, 4),
                new Vector2(1, 2),
                new Vector2(1, 2),
                new Vector2(2, 2), 
                out _), Intersection.Result.None);

        Assert.AreEqual(Intersection.FindIntersection(
            new Vector2(1, 4),
            new Vector2(1, 0),
            new Vector2(0, 2),
            new Vector2(2, 2),
            out I), Intersection.Result.Point);
        Assert.AreEqual(I.x, 1f);
        Assert.AreEqual(I.y, 2f);

        Assert.AreEqual(Intersection.FindIntersection(
            new Vector2(0, 2),
            new Vector2(2, 2),
            new Vector2(1, 4),
            new Vector2(1, 0),
            out I), Intersection.Result.Point);
        Assert.AreEqual(I.x, 1f);
        Assert.AreEqual(I.y, 2f);
    }

    [Test]
    public void FindIntersectionCross()
    {
        Vector2 I;

        Assert.AreEqual(
            Intersection.FindIntersection(
                new Vector2(0, 0),
                new Vector2(4, -2),
                new Vector2(-3, -2),
                new Vector2(-1, 4),
                out _), Intersection.Result.None);

        Assert.AreEqual(Intersection.FindIntersection(
            new Vector2(-3, -2),
            new Vector2(-1, 4),
            new Vector2(0, 0),
            new Vector2(-4, 2),
            out I), Intersection.Result.Point);
        Assert.AreEqual(I.x, -2f);
        Assert.AreEqual(I.y, 1f);

        Assert.AreEqual(Intersection.FindIntersection(
            new Vector2(0, 0),
            new Vector2(-2, 1),
            new Vector2(-3, -2),
            new Vector2(-2, 1),
            out _), Intersection.Result.None);
    }
}