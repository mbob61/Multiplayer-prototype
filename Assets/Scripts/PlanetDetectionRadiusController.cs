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
    private Renderer detectionRenderer;

    // Start is called before the first frame update
    void Start()
    {
        detectionRenderer = GetComponent<Renderer>();
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
                SetColor(GetOnlyTeamInsideRadius());
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
        detectionRenderer.materials[0].color = c;
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

    public Color GetOnlyTeamInsideRadius()
    {
        return colorMap.ElementAt(0).Key;
    }

    public void IsAllowedToSetColor(bool allowed)
    {
        allowedToSetColor = allowed;
    }
}
