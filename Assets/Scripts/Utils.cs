using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float GetYFromX(Vector3 a, Vector3 b, float x)
    {
        var t = Mathf.InverseLerp(a.x, b.x, x);
        var y = Mathf.Lerp(a.y, b.y, t);

        return y;
    }
}
