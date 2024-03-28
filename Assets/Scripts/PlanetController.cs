using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlanetController : MonoBehaviour
{
    [Header("GameObject References")]
    [SerializeField] private PlanetDetectionRadiusController detectionRadius;
    [SerializeField] private GameObject planetBody;

    [Header("Conversion Values")]
    [SerializeField] private float totalConversionRequired = 10;
    [SerializeField] private float convertedRate = 1.0f;
    [SerializeField] private float resetRate = 2.0f;
    [SerializeField] private Material defaultMaterial;

    private float conversionAmount = 0;
    private bool converted = false;
    private int team = -1;
    private Renderer planetBodyRenderer;

    // Start is called before the first frame update
    void Start()
    {
        planetBodyRenderer = planetBody.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {



        //If a player is inside the conversion radius
        if (detectionRadius.GetTeamCountInsidRadius() == 1)
        {
            // If the planet is not currently owned by a player
            if (!converted)
            {
                float amountToIncrease = Time.deltaTime * convertedRate;
                if (conversionAmount + amountToIncrease >= totalConversionRequired)
                {
                    conversionAmount = totalConversionRequired;
                    converted = true;
                    detectionRadius.IsAllowedToSetColor(false);
                    detectionRadius.SetColor(detectionRadius.GetDefaultMaterial().color);
                    planetBodyRenderer.materials[0].color = detectionRadius.GetOnlyTeamInsideRadius();
                }
                else
                {
                    conversionAmount += amountToIncrease;
                }
                print(conversionAmount);
            }
            else
            {
                if (planetBodyRenderer.materials[0].color != detectionRadius.GetOnlyTeamInsideRadius())
                {
                    detectionRadius.IsAllowedToSetColor(true);

                    float amountToDecrease = Time.deltaTime * convertedRate;
                    if (conversionAmount - amountToDecrease <= 0)
                    {
                        conversionAmount = 0;
                        converted = false;
                        planetBodyRenderer.materials[0].color = defaultMaterial.color;
                    }
                    else
                    {
                        conversionAmount -= amountToDecrease;
                    }
                }
            }
        } else if (detectionRadius.GetTeamCountInsidRadius() > 1)
        {
            detectionRadius.IsAllowedToSetColor(true);
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
