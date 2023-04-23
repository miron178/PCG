using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outline : MonoBehaviour
{
    public static LinkedListNode<Vector2> StartVertex(LinkedList<Vector2> shape)
    {
        Debug.Assert(shape != null);
        Debug.Assert(shape.First != null);

        LinkedListNode<Vector2> found = shape.First;
        LinkedListNode<Vector2> node  = found.Next;
        while(node != null)
        {
            if (node.Value.x < found.Value.x)
            {
                found = node;
            }
            node = node.Next;
        }
        return found;
    }

    public static LinkedListNode<Vector2> NextVertex(LinkedListNode<Vector2> node)
    {
        Debug.Assert(node != null);

        return node.Next != null ? node.Next : node.List.First;
    }

    public static bool Approximately(Vector2 a, Vector2 b)
    {
        return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
    }

    /// <summary>
    /// Insert a unique vertex into a shape
    /// If vertex is identical to one of the ends of the edge don't insert it.
    /// </summary>
    /// <param name="node">Start of the edge.</param>
    /// <param name="vertex">Vertex to add between edge ends unless identical to one of the ends</param>
    /// <returns>An inserted (or existing identical) vertex node.</returns>
    public static LinkedListNode<Vector2> InsertVertex(LinkedListNode<Vector2> node, Vector2 vertex)
    {
        //check if it is identical as start
        if (Approximately(node.Value, vertex))
        {
            return node;
        }

        //check if it is identical as end
        node = NextVertex(node);
        if (Approximately(node.Value, vertex))
        {
            return node;
        }

        return node.List.AddBefore(node, vertex);
    }

    /// <summary>
    /// Insert a nearest intersection point, if any, into a shape.
    /// 
    /// Look for any edge in the shape that intersects with AB.
    /// Find an intersetion that is closest to point A.
    /// </summary>
    /// <param name="A">Start of the edge.</param>
    /// <param name="B">End of the edge.</param>
    /// <param name="shapeStart">The first vertex on a shape</param>
    /// <returns>An intersection vertex neares to A or null if there's no intersection.</returns>
    public static LinkedListNode<Vector2> AddNeartestIntersetion(Vector2 A, Vector2 B, LinkedListNode<Vector2> shapeStart)
    {
        Vector2 I = Vector2.zero;
        LinkedListNode<Vector2> found = null;
        LinkedListNode<Vector2> current = shapeStart;
        LinkedListNode<Vector2> next;
        float nearest = Mathf.Infinity;
        do
        {
            next = NextVertex(current);
            Vector2 C = current.Value;
            Vector2 D = next.Value;

            Intersection.Result result;
            Vector2 J;
            if (Approximately(B, C))
            {
                result = Intersection.Result.Point;
                J = B;
            }
            else if (Approximately(A, D))
            {
                result = Intersection.Result.None;
                J = Vector2.zero;
            }
            else
            {
                result = Intersection.FindIntersection(A, B, C, D, out J);
            }

            if (result == Intersection.Result.Point)
            {
                float sqrDistance = (J - A).sqrMagnitude;
                if (nearest > sqrDistance)
                {
                    nearest = sqrDistance;
                    found = current;
                    I = J;
                }
            }
            else if (result == Intersection.Result.Overlap)
            {
                //TODO: handle this case
            }
            current = next;
        }
        while (current != shapeStart);

        if (found != null)
        {
            found = InsertVertex(found, I);
        }
        return found;
    }

    public struct Box
    {
        public Vector2 min;
        public Vector2 max;
    };

    public static Box BoundingBox(LinkedList<Vector2> shape)
    {
        Box box = new Box();
        box.min = new Vector2(Mathf.Infinity, Mathf.Infinity);
        box.max = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);

        LinkedListNode<Vector2> vertex = shape.First;
        while (vertex != null)
        {
            if (box.min.x > vertex.Value.x)
                box.min.x = vertex.Value.x;
            if (box.min.y > vertex.Value.y)
                box.min.y = vertex.Value.y;
            if (box.max.x < vertex.Value.x)
                box.max.x = vertex.Value.x;
            if (box.max.y < vertex.Value.y)
                box.max.y = vertex.Value.y;

            vertex = vertex.Next;
        }
        return box;
    }

    public static bool Separate(LinkedList<Vector2> shapeA, LinkedList<Vector2> shapeB)
    {
        var boxA = BoundingBox(shapeA);
        var boxB = BoundingBox(shapeB);
        if (boxA.max.x < boxB.min.x ||
            boxA.max.y < boxB.min.y ||
            boxA.min.x > boxB.max.x ||
            boxA.min.y > boxB.max.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static LinkedList<Vector2> Add(LinkedList<Vector2> shapeA, LinkedList<Vector2> shapeB)
    {
        LinkedList<Vector2> sum = new LinkedList<Vector2>();
        bool intersect = false;

        // Find starting vertex
        LinkedListNode<Vector2> startA = StartVertex(shapeA);
        LinkedListNode<Vector2> startB = StartVertex(shapeB);
        LinkedListNode<Vector2> start;
        LinkedListNode<Vector2> current;
        LinkedListNode<Vector2> other;

        if (startA.Value.x < startB.Value.x)
        {
            start = current = startA;
            other = startB;
        }
        else
        {
            start = current = startB;
            other = startA;
        }

        // Add to result
        sum.AddLast(start.Value);

        do
        {
            LinkedListNode<Vector2> next = NextVertex(current);
            LinkedListNode<Vector2> found = AddNeartestIntersetion(current.Value, next.Value, other);
            if (found == null)
            {
                if (next != start)
                {
                    sum.AddLast(next.Value);
                }
                current = next;
            }
            else
            {
                intersect = true;
                sum.AddLast(found.Value);
                other = InsertVertex(current, found.Value);
                current = found;
            }
        } while (current != start);

        if (!intersect && Separate(shapeA, shapeB))
        {
            // Add entire other shape
            LinkedListNode<Vector2> vertex = other;
            do
            {
                sum.AddLast(vertex.Value);
                vertex = NextVertex(vertex);
            }
            while (vertex != other);
        }
        //if !intersect and not separate then one inside the other
        //We startet with the outside shape so nothing to do

        return sum;
    }
}
