using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTurretRotator : MonoBehaviour
{

    private GameObject targetToAimAt;

    private void Update()
    {
        if (!targetToAimAt) return;

        transform.LookAt(targetToAimAt.transform.position);
    }

    public void SetTarget(GameObject _target)
    {
        targetToAimAt = _target;
    }

    public GameObject GetTarget()
    {
        return targetToAimAt;
    }
}
