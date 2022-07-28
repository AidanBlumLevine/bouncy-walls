using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Vector2 point, home, velocity;
    public Point before, after;
    public float aDist, bDist;
    public Point(Vector2 point)
    {
        this.point = point;
        home = point;
    }

    public void SetNeighbors(Point b, Point a)
    {
        before = b;
        after = a;
        bDist = Vector2.Distance(home, before.home);
        aDist = Vector2.Distance(home, after.home);
    }

    public void Push(Vector2 p)
    {
        velocity += p;
        if (p.sqrMagnitude > .0002f)
        {
            before.Push(p * .2f);
            after.Push(p * .2f);
        }
    }

    public Vector2 normal()
    {
        return Quaternion.AngleAxis(90, Vector3.forward) * (after.home - before.home).normalized;
    }

    public static Vector3[] toV3(List<Point> l)
    {
        Vector3[] v = new Vector3[l.Count];
        for (int i = 0; i < l.Count; i++)
        {
            v[i] = l[i].point;
        }
        return v;
    }

    public static Vector2[] toV2(List<Point> l)
    {
        Vector2[] v = new Vector2[l.Count];
        for (int i = 0; i < l.Count; i++)
        {
            v[i] = l[i].point;
        }
        return v;
    }

    // public static IPoint[] toPoints(List<Point> l)
    // {
    //     IPoint[] v = new IPoint[l.Count];
    //     for (int i = 0; i < l.Count; i++)
    //     {
    //         v[i] = new DelaunatorSharp.Point(l[i].point.x, l[i].point.y);
    //     }
    //     return v;
    // }
}