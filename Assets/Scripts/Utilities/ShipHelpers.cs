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
            if (returned < 0)
            {
                v = incrementFloat(returned, 1, 3.0f);
            } else {
                v = incrementFloat(returned, 1);
            }
        }
        else if (input < 0)
        {
            if (returned > 0)
            {
                v = decrementFloat(returned, -1, 3.0f);
            } else
            {
                v = decrementFloat(returned, -1);
            }
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
        return incrementFloat(v, target, 1.0f);
    }

    private float decrementFloat(float v, float target)
    {
        return decrementFloat(v, target, 1.0f);
    }

    private float incrementFloat(float v, float target, float incrementModifier)
    {
        float a = v;
        if (v < target)
        {
            if (v + (Time.fixedDeltaTime * incrementModifier) >= target)
            {
                a = target;
            }
            else
            {
                a += Time.deltaTime * incrementModifier;
            }
        }
        return a;
    }

    private float decrementFloat(float v, float target, float decrementModifier)
    {
        if (v > target)
        {
            if (v - (Time.fixedDeltaTime * decrementModifier)  <= target)
            {
                v = target;
            }
            else
            {
                v -= Time.deltaTime * decrementModifier;
            }
        }
        return v;
    }
}
