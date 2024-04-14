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
    private List<ShipControllerV6> addedShips;

    // Start is called before the first frame update
    void Start()
    {
        detectionRenderer = GetComponent<Renderer>();
        teamsMap = new Dictionary<int, CapturingTeam>();
        teamMaterialAssigner = FindFirstObjectByType<TeamMaterialAssigner>();

        addedShips = new List<ShipControllerV6>();
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


        for(int i = 0; i < addedShips.Count; i++)
        {
            ShipControllerV6 ship = addedShips[i];
            if (!ship || !ship.isActiveAndEnabled)
            {
                RemoveShip(ship);
                addedShips.Remove(ship);
                i--;
            }
        }
        

    }

    private void OnTriggerEnter(Collider other)
    {
        ShipControllerV6 ship = other.GetComponent<ShipControllerV6>();
        if (ship)
        {
            addedShips.Add(ship);

            if (!teamsMap.ContainsKey(ship.GetTeamID()))
            {
                addedShips = new List<ShipControllerV6>();
                addedShips.Add(ship);
                teamsMap.Add(ship.GetTeamID(), new CapturingTeam(ship.GetTeamID(), 1, teamMaterialAssigner.GetMaterialForTeamWithID(ship.GetTeamID()).color, addedShips));
            }
            else
            {
                //teamsMap[ship.GetTeamID()].SetCount(teamsMap[ship.GetTeamID()].GetCount() + 1);
                teamsMap[ship.GetTeamID()].IncrementCount();
                teamsMap[ship.GetTeamID()].AddShip(ship);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ShipControllerV6 ship = other.GetComponent<ShipControllerV6>();
        if (ship)
        {
            RemoveShip(ship);
        }
    }

    private void RemoveShip(ShipControllerV6 ship)
    {
        if (teamsMap[ship.GetTeamID()].GetCount() == 1)
        {
            teamsMap.Remove(ship.GetTeamID());
        }
        else
        {
            //teamsMap[ship.GetTeamID()].SetCount(teamsMap[ship.GetTeamID()].GetCount() - 1);
            teamsMap[ship.GetTeamID()].DecrementCount();
            teamsMap[ship.GetTeamID()].RemoveShip(ship);
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

