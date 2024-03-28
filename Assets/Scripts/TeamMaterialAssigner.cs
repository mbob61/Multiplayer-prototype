using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamMaterialAssigner : MonoBehaviour
{
    [SerializeField] private List<Material> teamMaterials;

    public List<Material> GetTeamMaterials()
    {
        return teamMaterials;
    }

    public Material GetMaterialForTeamWithID(int id)
    {

        // -1 is here because the list here is zero-based, but the team IDs are 1 based;
        // For example, if we want material for team 1, that would be index 0.
        // For example, if we want material for team 2, that would be index 1. etc..
        return GetTeamMaterials()[id - 1];
    }
   
}
