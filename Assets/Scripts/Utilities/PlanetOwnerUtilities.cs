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

    public void IncrementCount()
    {
        count++;
    }

    public void DecrementCount()
    {
        count--;
    }

}