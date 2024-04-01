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

    private GameObject target = null;

    private void Start()
    {
        StartCoroutine("FindTargetsWithDelay", 0.2f);
    }

    //private void Update()
    //{
    //    turretRotator.SetTarget(target);
    //}

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
            Transform viewTarget = c.transform;
            Vector3 directionToTarget = (viewTarget.position - transform.position).normalized;

            float angle = Vector3.Angle(transform.forward, directionToTarget);

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
        target = null;
    }

    public GameObject GetTarget()
    {
        return target;
    }
   
}
