using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [Header("==== Ship Movement Settings ===")]
    [SerializeField] private float yawTorgue = 100;
    [SerializeField] private float pitchTorque = 100;
    [SerializeField] private float rollTorque = 100;
    [SerializeField] private float thrust = 500f;
    [SerializeField] private float upThrust = 500f;
    [SerializeField] private float strafeThrust = 500f;
    [SerializeField, Range(0.001f, 0.999f)] private float thrustGlideReduction = 0.999f;
    [SerializeField, Range(0.001f, 0.999f)] private float upDownGlideReduction = 111f;
    [SerializeField, Range(0.001f, 0.999f)] private float leftRightGlideReduction = .0111f;

    private float thrust1D;
    private float upDown1D;
    private float strafe1D;
    private float roll1D;
    private Vector2 pitchYaw;
    private float glide, verticalGlide, horizontalGlide = 0f;

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Roll Torque
        rb.AddRelativeTorque(Vector3.back * roll1D * rollTorque * Time.fixedDeltaTime);

        // Pitch
        // Remove the - from pitchYaw to switch from non-inverted to inverted controls
        rb.AddRelativeTorque(Vector3.right * Mathf.Clamp(-pitchYaw.y, -1f, 1f) * pitchTorque * Time.fixedDeltaTime);

        // Yaw
        rb.AddRelativeTorque(Vector3.up * Mathf.Clamp(pitchYaw.x, -1f, 1f) * yawTorgue * Time.fixedDeltaTime);

        // Thrust
        if (thrust1D > 0.1f || thrust1D < -0.1f)
        {
            float currentThrust = thrust;
            rb.AddRelativeForce(Vector3.forward * thrust1D * currentThrust * Time.fixedDeltaTime);
            glide = thrust;
        } else
        {
            rb.AddRelativeForce(Vector3.forward * glide * Time.fixedDeltaTime);
            glide *= thrustGlideReduction;
        }

        // Up / Down
        if (upDown1D > 0.1f || upDown1D < -0.1f)
        {
            rb.AddRelativeForce(Vector3.up * upDown1D * upThrust * Time.fixedDeltaTime);
            verticalGlide = upDown1D * upThrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.up * verticalGlide * Time.fixedDeltaTime);
            verticalGlide *= upDownGlideReduction;
        }

        //Strafing
        if (strafe1D > 0.1f || strafe1D < -0.1f)
        {
            rb.AddRelativeForce(Vector3.right * strafe1D * strafeThrust * Time.fixedDeltaTime);
            horizontalGlide = strafe1D * strafeThrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.right * horizontalGlide * Time.fixedDeltaTime);
            horizontalGlide *= leftRightGlideReduction;
        }

    }

    #region Input Methods

    public void OnThrust(InputAction.CallbackContext context)
    {
        thrust1D = context.ReadValue<float>();
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        strafe1D = context.ReadValue<float>();
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        upDown1D = context.ReadValue<float>();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        roll1D = context.ReadValue<float>();
    }

    public void OnPitchYaw(InputAction.CallbackContext context)
    {
        pitchYaw = context.ReadValue<Vector2>();
    }


    #endregion
}
