using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float viewDistance;

    [Range(0, 360)]
    [SerializeField] private float viewAngle;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private FOV_STATES fovDirection = FOV_STATES.Transform_Up;

    private int teamIDToProtect;
    private GameObject target = null;


    private enum FOV_STATES
    {
        Transform_Forward,
        Transform_Up
    }

    private void Start()
    {
        StartCoroutine("FindTargetsWithDelay", 0.2f);
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewDistance, targetMask);

        foreach (Collider c in targetsInViewRadius)
        {
            ShipControllerV6 ship = c.GetComponent<ShipControllerV6>();
            if (ship && ship.GetTeamID() != teamIDToProtect)
            {
                Transform viewTarget = c.transform;
                Vector3 directionToTarget = (viewTarget.position - transform.position).normalized;

                Vector3 fovdir = fovDirection == FOV_STATES.Transform_Forward ? transform.forward : transform.up;

                float angle = Vector3.Angle(fovdir, directionToTarget);

                if (angle < viewAngle / 2)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, viewTarget.position);
                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                    {
                        if (!target)
                        {
                            target = viewTarget.gameObject;
                        }
                        return;
                    }
                }
            }
        }
        target = null;
    }

    public GameObject GetTarget()
    {
        return target;
    }

    public void setTeamToProtect(int id)
    {
        teamIDToProtect = id;
    }
   
}
