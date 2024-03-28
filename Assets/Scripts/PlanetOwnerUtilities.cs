using UnityEngine;

public class PlanetOwnerUtilities
{
}

public class PlanetTeamOwner
{
    public int teamID;
    public Color teamColor;

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
