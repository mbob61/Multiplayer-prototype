using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTurretV2 : MonoBehaviour
{
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private GameObject turretBase, turretCannon;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float turretForce;
    [SerializeField] private float turretAngle;
    [SerializeField] private GameObject turretProjectile;

    private GameObject target;
    private float currentAngle, currentSpeed;

    private void Start()
    {
        StartCoroutine("fireWithDelay", 2.0f);
    }

    private IEnumerator fireWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            if (target)
            {
                fireShot();
            }
        }
    }


    private void fireShot()
    {

        //GameObject p = Instantiate(turretProjectile, firePoint.position, Quaternion.identity);
        //p.GetComponent<Rigidbody>().velocity = turretCannon.transform.up * currentSpeed;

    }


    // Update is called once per frame
    void Update()
    {
        target = fieldOfView.GetTarget();

        if (!target) return;

        SetTargetWithSpeed(target.transform.position, turretForce, true);        
    }

    public void SetTargetWithAngle(Vector3 point, float angle)
    {
        currentAngle = angle;

        Vector3 direction = point - firePoint.position;
        float yOffset = -direction.y;
        direction = ProjectVectorOnPlane(Vector3.up, direction);
        float distance = direction.magnitude;

        currentSpeed = LaunchSpeed(distance, yOffset, Physics.gravity.magnitude, angle * Mathf.Deg2Rad);

        SetTurret(direction, currentAngle);
    }

    public void SetTargetWithSpeed(Vector3 point, float speed, bool useLowAngle)
    {
        currentSpeed = speed;

        Vector3 direction = point - firePoint.position;
        float yOffset = direction.y;
        direction = ProjectVectorOnPlane(Vector3.up, direction);
        float distance = direction.magnitude;

        float angle0, angle1;
        bool targetInRange = LaunchAngle(speed, distance, yOffset, Physics.gravity.magnitude, out angle0, out angle1);

        if (targetInRange)
            currentAngle = useLowAngle ? angle1 : angle0;

        SetTurret(direction, currentAngle * Mathf.Rad2Deg);
    }

    //Projects a vector onto a plane. The output is not normalized.
    public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
    {
        return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
    }

    private void SetTurret(Vector3 planarDirection, float turretAngle)
    {
        turretBase.transform.rotation = Quaternion.LookRotation(planarDirection) * Quaternion.Euler(-90, -90, 0);
        turretCannon.transform.localRotation = Quaternion.Euler(90, 90, 0) * Quaternion.AngleAxis(turretAngle, Vector3.forward);
    }



    /// <summary>
    /// Calculates the two possible initial angles that could be used to fire a projectile at the supplied
    /// speed to travel the desired distance
    /// </summary>
    /// <param name="speed">Initial speed of the projectile</param>
    /// <param name="distance">Distance along the horizontal axis the projectile will travel</param>
    /// <param name="yOffset">Elevation of the target with respect to the initial fire position</param>
    /// <param name="gravity">Downward acceleration in m/s^2</param>
    /// <param name="angle0"></param>
    /// <param name="angle1"></param>
    /// <returns>False if the target is out of range</returns>
    public static bool LaunchAngle(float speed, float distance, float yOffset, float gravity, out float angle0, out float angle1)
    {
        angle0 = angle1 = 0;

        float speedSquared = speed * speed;

        float operandA = Mathf.Pow(speed, 4);
        float operandB = gravity * (gravity * (distance * distance) + (2 * yOffset * speedSquared));

        // Target is not in range
        if (operandB > operandA)
            return false;

        float root = Mathf.Sqrt(operandA - operandB);

        angle0 = Mathf.Atan((speedSquared + root) / (gravity * distance));
        angle1 = Mathf.Atan((speedSquared - root) / (gravity * distance));

        return true;
    }

    public static float LaunchSpeed(float distance, float yOffset, float gravity, float angle)
    {
        float speed = (distance * Mathf.Sqrt(gravity) * Mathf.Sqrt(1 / Mathf.Cos(angle))) / Mathf.Sqrt(2 * distance * Mathf.Sin(angle) + 2 * yOffset * Mathf.Cos(angle));

        return speed;
    }
}
