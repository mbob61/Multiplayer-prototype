using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTurretController : MonoBehaviour
{
    [SerializeField] private PlanetTurretDetectionRadiusController detectionRadius;
    [SerializeField] private PlanetTurretRotator turretRotator;

    private int teamToProtect;

    private void Update()
    {
        detectionRadius.SetTeamToProtect(teamToProtect);
        turretRotator.SetTarget(detectionRadius.GetTarget());
    }

    public void SetTeamToProtect(int _id)
    {
        teamToProtect = _id;
    }
}
