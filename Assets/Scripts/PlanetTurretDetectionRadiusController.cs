using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTurretDetectionRadiusController : MonoBehaviour
{
    private GameObject target;
    private int teamToProtect = -1;

    void Update()
    {
        if (!target || !target.activeSelf)
        {
            target = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!target)
        {
            // -1 check here to make sure a team has been assigned before trying to set a target
            if (other.GetComponent<ShipControllerV3>().GetTeamID() != teamToProtect && teamToProtect != -1)
            {
                target = other.gameObject;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (target == other.gameObject)
        {
            target = null;
        }
    }

    public void SetTeamToProtect(int _id)
    {
        teamToProtect = _id;
    }

    public int GetTeamToProtect()
    {
        return teamToProtect;
    }

    public GameObject GetTarget()
    {
        return target;
    }
}
