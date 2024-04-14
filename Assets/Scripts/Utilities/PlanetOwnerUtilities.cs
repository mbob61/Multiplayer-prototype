using UnityEngine;
using System.Collections.Generic;


public class PlanetOwnerUtilities
{
}

public class PlanetTeamOwner
{
    private int teamID;
    private Color teamColor;

    public PlanetTeamOwner(int _teamID, Color _color)
    {
        teamID = _teamID;
        teamColor = _color;
    }

    public int GetTeamID()
    {
        return teamID;
    }

    public Color GetTeamColor()
    {
        return teamColor;
    }
}

public class CapturingTeam
{
    private int teamID;
    private int count;
    private Color teamColor;
    private List<ShipControllerV6> ships;

    public CapturingTeam(int _teamID, int _count, Color _color, List<ShipControllerV6> _ships)
    {
        teamID = _teamID;
        count = _count;
        teamColor = _color;
        ships = _ships;
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

    public List<ShipControllerV6> getShips()
    {
        return ships;
    }

    public void SetCount(int _count)
    {
        count = _count;
    }

    public void IncrementCount()
    {
        count++;
    }

    public void DecrementCount()
    {
        count--;
    }

    public void AddShip(ShipControllerV6 shipToAdd)
    {
        ships.Add(shipToAdd);
    }

    public void RemoveShip(ShipControllerV6 shipToRemove)
    {
        ships.Remove(shipToRemove);
    }
}