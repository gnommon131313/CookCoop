using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) 
    {
        Vector3 A = Vector3.Lerp(p0, p1, t);
        Vector3 B = Vector3.Lerp(p1, p2, t);
        Vector3 C = Vector3.Lerp(p2, p3, t);

        Vector3 D = Vector3.Lerp(A, B, t);
        Vector3 E = Vector3.Lerp(B, C, t);

        Vector3 F = Vector3.Lerp(D, E, t);

        return F;

        // the same thing only in the form of a mathematical formula

        //t = Mathf.Clamp01(t);
        //float oneMinusT = 1f - t;
        //return
        //    oneMinusT * oneMinusT * oneMinusT * p0 +
        //    3f * oneMinusT * oneMinusT * t * p1 +
        //    3f * oneMinusT * t * t * p2 +
        //    t * t * t * p3;
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }
}
