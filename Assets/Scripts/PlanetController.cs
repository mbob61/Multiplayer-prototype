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
    private PlanetTeamOwner currentlyConvertingTeam;
    private Renderer planetBodyRenderer;
    private float timeSinceConversion = 0;
    private float timeUntilSpawnDefences = 5.0f;

    private OCCUPIED_STATE occupiedState = OCCUPIED_STATE.empty;
    private CONVERSION_STATE conversionState = CONVERSION_STATE.not_converted;

    // Start is called before the first frame update
    void Start()
    {
        planetBodyRenderer = planetBody.GetComponent<Renderer>();
        resetOwningTeam();
    }

    private void resetOwningTeam()
    {
        owningTeam = new PlanetTeamOwner(-1, defaultMaterial.color);
        currentlyConvertingTeam = new PlanetTeamOwner(-1, defaultMaterial.color);
    }

    // Update is called once per frame
    void Update()
    {
        print(conversionAmount);

        //If a player is inside the conversion radius
        if (detectionRadius.GetTeamCountInsidRadius() == 1)
        {
            occupiedState = OCCUPIED_STATE.single;
            
        }
        // IF no one is inside the radius
        else if (detectionRadius.GetTeamCountInsidRadius() < 1)
        {
            occupiedState = OCCUPIED_STATE.empty;
        } else
        {
            occupiedState = OCCUPIED_STATE.multiple;
        }

        ActOnConversionAndOccupiedState(occupiedState, conversionState);

        //if (converted)
        //{
        //    timeSinceConversion += Time.deltaTime;
        //} else
        //{
        //    timeSinceConversion = 0;
        //}

    }

    private void ActOnConversionAndOccupiedState(OCCUPIED_STATE _occupiedState, CONVERSION_STATE _conversionState)
    { 
        switch (_occupiedState)
        {
            // If a single team is inside the conversion radius
            case OCCUPIED_STATE.single:
                switch (_conversionState)
                {
                    // If the planet owned by a team
                    case CONVERSION_STATE.converted:
                        // If the owning team is different from the team trying to take it
                        if (owningTeam.GetTeamID() != detectionRadius.GetOnlyTeamInsideRadius().GetTeamID())
                        {
                            float amountToDecrease = Time.deltaTime * convertedRate;
                            if (conversionAmount - amountToDecrease <= 0)
                            {
                                conversionAmount = 0;
                                conversionState = CONVERSION_STATE.not_converted;
                                resetOwningTeam();
                                planetBodyRenderer.materials[0].color = owningTeam.GetTeamColor();
                            }
                            else
                            {
                                conversionAmount -= amountToDecrease;
                            }
                        }
                        break;

                    // If the planet is not currently owned by a team
                    case CONVERSION_STATE.not_converted:
                        
                        CapturingTeam capturingTeam = detectionRadius.GetOnlyTeamInsideRadius();
                        // If the planet has been partially converted by another team,but your team is inside
                        // first erase their progress before continuing with your own
                        if (capturingTeam.GetTeamID() != currentlyConvertingTeam.GetTeamID())
                        {
                            float amountToIncrease = Time.deltaTime * convertedRate;

                            if (conversionAmount - amountToIncrease <= 0)
                            {
                                conversionAmount = 0;
                                currentlyConvertingTeam = new PlanetTeamOwner(capturingTeam.GetTeamID(), capturingTeam.GetTeamColor());
                            }
                            else
                            {
                                conversionAmount -= amountToIncrease;
                            }
                        }
                        // The team inside the detection radius is now capturing
                        else
                        {
                            float amountToIncrease = Time.deltaTime * convertedRate;

                            if (conversionAmount + amountToIncrease >= totalConversionRequired)
                            {
                                conversionAmount = totalConversionRequired;
                                conversionState = CONVERSION_STATE.converted;
                                owningTeam = new PlanetTeamOwner(capturingTeam.GetTeamID(), capturingTeam.GetTeamColor());

                                planetBodyRenderer.materials[0].color = owningTeam.GetTeamColor();
                            }
                            else
                            {
                                conversionAmount += amountToIncrease;
                                currentlyConvertingTeam = new PlanetTeamOwner(capturingTeam.GetTeamID(), capturingTeam.GetTeamColor());
                            }
                        }
                        break;
                }
                break;

            // If no team is inside the radius
            case OCCUPIED_STATE.empty:

                switch (_conversionState)
                {
                    // If its partially de-converted (and belongs to a team)
                    case CONVERSION_STATE.converted:
                        if (conversionAmount < totalConversionRequired)
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
                        break;

                    // If its partially converted (and belongs to noone)
                    case CONVERSION_STATE.not_converted:
                        if (conversionAmount > 0)
                        {
                            float amountToDecrease = Time.deltaTime * resetRate;

                            if (conversionAmount - amountToDecrease <= 0)
                            {
                                conversionAmount = 0;
                                currentlyConvertingTeam = new PlanetTeamOwner(-1, defaultMaterial.color);
                            }
                            else
                            {
                                conversionAmount -= amountToDecrease;
                            }
                        }
                        break;
                }
                break;
        }
    }

    private void decrementConversionBackToZero()
    {

    }

    private enum OCCUPIED_STATE
    {
        empty,
        single,
        multiple
    }

    private enum CONVERSION_STATE{
        converted,
        not_converted

    }
}