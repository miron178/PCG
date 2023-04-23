using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
    public enum Result
    {
        None,
        Point,
        Overlap
    };

    public static bool Line(Vector2 A, Vector2 B, out float a, out float b)
    {
        if (Mathf.Approximately(A.x, B.x)) //vertical line, can't do y = ax + b
        {
            a = b = Mathf.Infinity;
            return false;
        }

        a = (A.y - B.y) / (A.x - B.x);
        b = A.y - a * A.x;
        return true;
    }

    public static bool IsBetweenInclusive(float value, float start, float end)
    {
        if (Mathf.Approximately(value, start) || Mathf.Approximately(value, end))
        {
            return true;
        }
        float min = Mathf.Min(start, end);
        float max = Mathf.Max(start, end);
        return value >= min && value <= max;
    }

    public static bool IsBetweenExclusive(float value, float start, float end)
    {
        if (Mathf.Approximately(value, start) || Mathf.Approximately(value, end))
        {
            return false;
        }
        float min = Mathf.Min(start, end);
        float max = Mathf.Max(start, end);
        return value > min && value < max;
    }

    //Find if two vertical line segments AB and CD overlap
    public static bool FindOverlap(float yA, float yB, float yC, float yD)
    {
        float minAB = Mathf.Min(yA, yB);
        float maxAB = Mathf.Max(yA, yB);
        float minCD = Mathf.Min(yC, yD);
        float maxCD = Mathf.Max(yC, yD);

        if (minAB > maxCD || minCD > maxAB)
        {
            return false;
        }
        return true;
    }

    //Find a non-vertical line segment AB intersects with a vertical segment CD
    public static Result FindIntersection(Vector2 A, Vector2 B, float xCD, float yC, float yD, out Vector2 I)
    {
        I = Vector2.zero;

        //Check if vertical line is between A and B
        if (!IsBetweenInclusive(xCD, A.x, B.x))
        {
            return Result.None;
        }

        //Line AB: y = aAB * x + bAB
        bool hasAB = Line(A, B, out float a, out float b);
        Debug.Assert(hasAB);

        //Possible intersection point
        I.x = xCD;
        I.y = a * xCD + b;

        //Check if intersection y coordinate is between C and D
        //Touching ends don't count as intersection
        return IsBetweenExclusive(I.y, yC, yD) ? Result.Point : Result.None;
    }

    public static Result FindIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D, out Vector2 I)
    {
        I = Vector2.zero;

        //Line AB: y = aAB * x + bAB
        bool hasAB = Line(A, B, out float aAB, out float bAB);

        //Line CD: y = aCD * x + bCD
        bool hasCD = Line(C, D, out float aCD, out float bCD);

        if (!hasAB && !hasCD)
        {
            //both lines are vertical
            Debug.Assert(Mathf.Approximately(A.x, B.x));
            Debug.Assert(Mathf.Approximately(C.x, D.x));
            return FindOverlap(A.y, B.y, C.y, D.y) ? Result.Overlap : Result.None;

        }
        else if (!hasAB && hasCD)
        {
            //only AB is vertical
            Debug.Assert(Mathf.Approximately(A.x, B.x));
            return FindIntersection(C, D, A.x, A.y, B.y, out I);

        }
        else if (hasAB && !hasCD)
        {
            //only CD is vertical
            Debug.Assert(Mathf.Approximately(C.x, D.x));
            return FindIntersection(A, B, C.x, C.y, D.y, out I);
        }
        //else hasAB && hasCD
        //both lines are non-vertical
        Debug.Assert(hasAB);
        Debug.Assert(hasCD);

        if (Mathf.Approximately(aAB, aCD))
        {
            if (Mathf.Approximately(bAB, bCD))
            {
                //AB and CD are on the same line, check overlap
                return FindOverlap(A.x, B.x, C.x, D.x) ? Result.Overlap : Result.None;
            }
            else
            {
                //AB and CD are parallel, there's no intersection
                return Result.None;
            }
        }

        //AB and CD are not parallel, there is one intersection point
        //but it may be outside of the AB or CD line section

        //Intersection point I
        I.x = (bCD - bAB) / (aAB - aCD);
        I.y = aAB * I.x + bAB;
        //Debug.Assert(Mathf.Abs(I.y - (aCD * I.x + bCD)) < 0.1);

        //Check if intersection is between AB and between CD
        return IsBetweenExclusive(I.x, A.x, B.x) && IsBetweenExclusive(I.x, C.x, D.x) ?
            Result.Point : Result.None;
    }
}
