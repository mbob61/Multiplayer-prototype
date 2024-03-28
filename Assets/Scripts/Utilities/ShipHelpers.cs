using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipHelpers
{
    public float calculatefloatValue(float input, float returned)
    {
        return calculatefloatValue(input, returned, true);
    }

    public float calculatefloatValue(float input, float returned, bool resetBackToZero)
    {
        float v = returned;
        if (input > 0)
        {
            v = incrementFloat(returned, 1);
        }
        else if (input < 0)
        {
            v = decrementFloat(returned, -1);
        }
        else
        {
            if (resetBackToZero)
            {
                if (returned > 0)
                {
                    v = decrementFloat(returned, 0);
                }
                else if (returned < 0)
                {
                    v = incrementFloat(returned, 0);

                }
            }
        }
        return v;
    }

    private float incrementFloat(float v, float target)
    {
        float a = v;
        if (v < target)
        {
            if (v + Time.fixedDeltaTime >= target)
            {
                a = target;
            }
            else
            {
                a += Time.deltaTime;
            }
        }
        return a;
    }

    private float decrementFloat(float v, float target)
    {
        if (v > target)
        {
            if (v - Time.fixedDeltaTime <= target)
            {
                v = target;
            }
            else
            {
                v -= Time.deltaTime;
            }
        }
        return v;
    }
}
