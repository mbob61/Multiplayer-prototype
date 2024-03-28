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
    private PlanetTeamOwner owningTeam;
    private Renderer planetBodyRenderer;

    // Start is called before the first frame update
    void Start()
    {
        planetBodyRenderer = planetBody.GetComponent<Renderer>();
        resetOwningTeam();
    }

    private void resetOwningTeam()
    {
        owningTeam = new PlanetTeamOwner(-1, defaultMaterial.color);
    }

    // Update is called once per frame
    void Update()
    {
        //print(conversionAmount);
        DebugUtilities.DumpToConsole(owningTeam);

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
                    CapturingTeam capturingTeam = detectionRadius.GetOnlyTeamInsideRadius();
                    owningTeam = new PlanetTeamOwner(capturingTeam.GetTeamID(), capturingTeam.GetTeamColor());

                    planetBodyRenderer.materials[0].color = owningTeam.GetTeamColor();

                }
                else
                {
                    conversionAmount += amountToIncrease;
                }
            }
            // If the planet owned by a team
            else
            {
                // If the owning team is different from the team trying to take it
                if (owningTeam.GetTeamID() != detectionRadius.GetOnlyTeamInsideRadius().GetTeamID())
                {
                    float amountToDecrease = Time.deltaTime * convertedRate;
                    if (conversionAmount - amountToDecrease <= 0)
                    {
                        conversionAmount = 0;
                        converted = false;
                        resetOwningTeam();
                        planetBodyRenderer.materials[0].color = owningTeam.GetTeamColor();
                    }
                    else
                    {
                        conversionAmount -= amountToDecrease;
                    }
                }
            }
        }
        // IF no one is inside the radius
        else if (detectionRadius.GetTeamCountInsidRadius() < 1)
        {
            //If its partially converted (and belongs to noone)
            if (!converted && conversionAmount > 0)
            {
                float amountToDecrease = Time.deltaTime * resetRate;

                if (conversionAmount - amountToDecrease <= 0)
                {
                    conversionAmount = 0;
                }
                else
                {
                    conversionAmount -= amountToDecrease;
                }
            }
            // If its partially de-converted (and belongs to a team
            else if (converted && conversionAmount < totalConversionRequired)
            {
                float amountToDecrease = Time.deltaTime * resetRate;

                if (conversionAmount + amountToDecrease >= totalConversionRequired)
                {
                    conversionAmount = totalConversionRequired;
                }
                else
                {
                    conversionAmount += amountToDecrease;
                }
            }
        }
    }
}