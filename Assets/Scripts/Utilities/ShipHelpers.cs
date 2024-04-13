using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipHelpers
{
    public float calculatefloatValue(float input, float returned, float accelerationRate, float decelerationRate)
    {
        return calculatefloatValue(input, returned, true, accelerationRate, decelerationRate);
    }

    public float calculatefloatValue(float input, float value, bool resetBackToZero, float accelerationRate, float decelerationRate)
    {
        float currentValue = value;
        if (input > 0)
        {
            //if (currentValue < 0)
            //{
                currentValue = incrementFloat(currentValue, 1, accelerationRate);
            //} else {
            //    currentValue = incrementFloat(currentValue, 1);
            //}
        }
        else if (input < 0)
        {
            //if (currentValue > 0)
            //{
                currentValue = decrementFloat(currentValue, -1, accelerationRate);
            //} else
            //{
            //    currentValue = decrementFloat(currentValue, -1);
            //}
        }
        else
        {
            if (resetBackToZero)
            {
                if (currentValue > 0)
                {
                    currentValue = decrementFloat(currentValue, 0, decelerationRate);
                }
                else if (currentValue < 0)
                {
                    currentValue = incrementFloat(currentValue, 0, decelerationRate);

                }
            }
        }
        return currentValue;
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
