using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlanetDetectionRadiusController : MonoBehaviour
{
    [SerializeField] private Material contestedMaterial;
    [SerializeField] private Material defaultMaterial;
    private Dictionary<int, CapturingTeam> teamsMap;
    private Renderer detectionRenderer;
    private TeamMaterialAssigner teamMaterialAssigner;

    // Start is called before the first frame update
    void Start()
    {
        detectionRenderer = GetComponent<Renderer>();
        teamsMap = new Dictionary<int, CapturingTeam>();
        teamMaterialAssigner = FindFirstObjectByType<TeamMaterialAssigner>();
    }

    private void Update()
    {
        if (GetTeamCountInsidRadius() > 1)
        {
            SetColor(GetContestedColor().color);
        }
        else if (GetTeamCountInsidRadius() == 1)
        {
            SetColor(GetOnlyTeamInsideRadius().GetTeamColor());
        }
        else
        {
            SetColor(GetDefaultMaterial().color);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ShipControllerV6 ship = other.GetComponent<ShipControllerV6>();
        if (ship)
        {
            print("ship entered");
            if (!teamsMap.ContainsKey(ship.GetTeamID()))
            {
                teamsMap.Add(ship.GetTeamID(), new CapturingTeam(ship.GetTeamID(), 1, teamMaterialAssigner.GetMaterialForTeamWithID(ship.GetTeamID()).color));
            }
            else
            {
                teamsMap[ship.GetTeamID()].SetCount(teamsMap[ship.GetTeamID()].GetCount() + 1);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ShipControllerV6 ship = other.GetComponent<ShipControllerV6>();
        if (ship)
        {            
            if (teamsMap[ship.GetTeamID()].GetCount() == 1)
            {
                teamsMap.Remove(ship.GetTeamID());
            }
            else
            {
                teamsMap[ship.GetTeamID()].SetCount(teamsMap[ship.GetTeamID()].GetCount() - 1);
            }
        }
    }

    public void SetColor(Color c)
    {
        detectionRenderer.materials[0].color = new Color(c.r, c.g, c.b, 0.25f);
    }

    public Material GetDefaultMaterial()
    {
        return defaultMaterial;
    }

    public Material GetContestedColor()
    {
        return contestedMaterial;
    }

    public Dictionary<int, CapturingTeam> GetTeamsMap()
    {
        return teamsMap;
    }

    public int GetTeamCountInsidRadius()
    {
        return teamsMap.Keys.Count;
    }

    public CapturingTeam GetOnlyTeamInsideRadius()
    {
        return teamsMap.ElementAt(0).Value;
    }
}

