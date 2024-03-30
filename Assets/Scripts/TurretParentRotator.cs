using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretParentRotator : MonoBehaviour
{
    private bool canRotate;
    [SerializeField] private Vector3 rotationAxis;
    [SerializeField] private float speed;

    // Update is called once per frame
    void Update()
    {
        if (canRotate)
        {
            transform.Rotate(rotationAxis * speed);
        }
    }

    public void CanRotate(bool _canRotate)
    {
        canRotate = _canRotate;
    }
}
