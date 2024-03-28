using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    [SerializeField] private PlanetDetectionRadiusController detectionRadius;

    private bool converted = false;
    private float conversionAmount = 0;
    private float convertedRate = 1.0f;
    private float resetRate = 2.0f;
    private int team = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {



        //If a player is inside the conversion radius
        if (detectionRadius.IsInteractingWithAPlayer())
        {
            // If the planet is not currently owned by a player
            if (!converted)
            {
                float amountToIncrease = Time.deltaTime * convertedRate;
                if (conversionAmount + amountToIncrease >= 100)
                {
                    conversionAmount = 100f;
                    converted = true;
                    detectionRadius.IsAllowedToSetColor(false);
                    detectionRadius.SetColor(detectionRadius.GetDefaultMaterial().color);
                }
                else
                {
                    conversionAmount += amountToIncrease;
                }

            }
        }
        ////If no player is inside the radius
        //else
        //{
        //    // If the planet is unconverted, tick down the radius
        //    if (!converted)
        //    {
        //        float amountToDecrease = Time.deltaTime * resetRate;

        //        if (conversionAmount - amountToDecrease <= 0)
        //        {
        //            conversionAmount = 0;
        //        }
        //        else
        //        {
        //            conversionAmount -= amountToDecrease;
        //        }
        //    }
        //}
    }
}
