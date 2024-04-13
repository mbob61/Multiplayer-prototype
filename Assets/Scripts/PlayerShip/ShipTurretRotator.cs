using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipTurretRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float accelerationRate = 1.0f;
    [SerializeField] private float decelerationRate = 1.0f;
    private float rotation, rotationInput;
    private ShipHelpers shipHelpers;

    private void Start()
    {
        shipHelpers = new ShipHelpers();
    }

    public void OnRotation(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<float>();
    }

    private void Update()
    {
        ConvertToDecimalValues();

        transform.Rotate(Vector3.up * rotation * rotationSpeed * Time.deltaTime);
    }

    private void ConvertToDecimalValues()
    {
        rotation = shipHelpers.calculatefloatValue(rotationInput, rotation, accelerationRate, decelerationRate);
    }
}
