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

    // Start is called before the first frame update
    void Start()
    {
        detectionRenderer = GetComponent<Renderer>();
        teamsMap = new Dictionary<int, CapturingTeam>();
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
        ShipControllerV3 ship = other.GetComponent<ShipControllerV3>();
        if (ship)
        {
            if (!teamsMap.ContainsKey(ship.GetTeamID()))
            {
                teamsMap.Add(ship.GetTeamID(), new CapturingTeam(ship.GetTeamID(), 1, ship.GetTeamMaterial().color));
            }
            else
            {
                teamsMap[ship.GetTeamID()].SetCount(teamsMap[ship.GetTeamID()].GetCount() + 1);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ShipControllerV3 ship = other.GetComponent<ShipControllerV3>();
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

public class CapturingTeam
{
    public int teamID;
    public int count;
    public Color teamColor;

    public CapturingTeam(int _teamID, int _count, Color _color)
    {
        teamID = _teamID;
        count = _count;
        teamColor = _color;
    }

    public int GetTeamID()
    {
        return teamID;
    }

    public int GetCount()
    {
        return count;
    }

    public Color GetTeamColor()
    {
        return teamColor;
    }

    public void SetCount(int _count)
    {
        count = _count;
    }
}
