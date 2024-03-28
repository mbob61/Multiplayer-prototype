using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlanetDetectionRadiusController : MonoBehaviour
{
    [SerializeField] private Material contestedMaterial;
    [SerializeField] private Material defaultMaterial;
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
            if (GetTeamCountInsidRadius() > 1)
            {
                SetColor(GetContestedColor().color);
            }
            else if (GetTeamCountInsidRadius() == 1)
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

    public int GetTeamCountInsidRadius()
    {
        return colorMap.Keys.Count;
    }

    public void IsAllowedToSetColor(bool allowed)
    {
        allowedToSetColor = allowed;
    }
}
