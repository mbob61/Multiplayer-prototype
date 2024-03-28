using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlanetDetectionRadiusController : MonoBehaviour
{
    [SerializeField] private Material contestedMaterial;
    [SerializeField] private Material defaultMaterial;
    private bool isInteracting = false;
    private Dictionary<Color, int> colorMap;
    private bool allowedToSetColor = true;

    // Start is called before the first frame update
    void Start()
    {
        colorMap = new Dictionary<Color, int>();
    }

    private void Update()
    {
        if (allowedToSetColor)
        {
            if (colorMap.Keys.Count > 1)
            {
                SetColor(GetContestedColor().color);
            }
            else if (colorMap.Keys.Count == 1)
            {
                SetColor(colorMap.ElementAt(0).Key);
            }
            else
            {
                SetColor(GetDefaultMaterial().color);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        ShipControllerV3 shipController = other.GetComponent<ShipControllerV3>();
        if (shipController)
        {
            isInteracting = true;

            if (!colorMap.ContainsKey(shipController.GetTeamMaterial().color))
            {
                colorMap.Add(shipController.GetTeamMaterial().color, 1);
            }
            else
            {
                colorMap[shipController.GetTeamMaterial().color] = colorMap[shipController.GetTeamMaterial().color] + 1;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ShipControllerV3 shipController = other.GetComponent<ShipControllerV3>();
        if (shipController)
        {
            isInteracting = false;
            
            if (colorMap[shipController.GetTeamMaterial().color] == 1)
            {
                colorMap.Remove(shipController.GetTeamMaterial().color);
            }
            else
            {
                colorMap[shipController.GetTeamMaterial().color] = colorMap[shipController.GetTeamMaterial().color] - 1;
            }
        }
    }

    public void SetColor(Color c)
    {
        GetComponent<Renderer>().materials[0].color = c;
    }

    public Material GetDefaultMaterial()
    {
        return defaultMaterial;
    }

    public Material GetContestedColor()
    {
        return contestedMaterial;
    }

    public Dictionary<Color, int> GetColorMap()
    {
        return colorMap;
    }

    public bool IsInteractingWithAPlayer()
    {
        return isInteracting;
    }

    public void IsAllowedToSetColor(bool allowed)
    {
        allowedToSetColor = allowed;
    }
}
