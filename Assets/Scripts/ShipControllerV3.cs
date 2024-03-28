using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipControllerV3 : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [Header("Movement Modifiers")]
    //[SerializeField] private float thrustModifier, upDownModifier, yawModifier, pitchModifier, rollModifier = 1;
    [SerializeField] private float thrustModifier = 1;
    [SerializeField] private float upDownModifier = 1;
    [SerializeField] private float yawModifier = 1;
    [SerializeField] private float pitchModifier = 1;
    [SerializeField] private float rollModifier = 1;

    [Header("Movement Types")]
    [SerializeField] private bool requireNetwork = true;
    [SerializeField] private bool clientAuthorititiveMovement = false;
    [SerializeField] private bool planeControls = false; 
    [SerializeField] private bool airshipControls = true; 
    [SerializeField] private bool holdToThrust = true;

    private float thrustInput, thrust = 0;
    private Vector3 thrustForce = Vector3.zero;

    private float upDownInput, upDown = 0;
    private Vector3 upDownForce = Vector3.zero;

    private float yawInput, yaw = 0;
    private float pitchInput, pitch = 0;
    private float rollInput, roll = 0;

    [Header("GameObject References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletToSpawn;

    [Header("Planet Conversion")]
    [SerializeField] private Material teamMaterial;


    private struct MyShipData : INetworkSerializable
    {
        public int health;
        public bool isAlive;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref health);
            serializer.SerializeValue(ref isAlive);
        }
    }

    //private NetworkVariable<MyShipData> randomData = new NetworkVariable<MyShipData>(new MyShipData { health = 5, isAlive = false,}, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //public override void OnNetworkSpawn()
    //{
    //    randomData.OnValueChanged += (MyShipData previous, MyShipData newvalue) =>
    //    {
    //        Debug.Log($"{OwnerClientId}: health: {newvalue.health} isAlive: {newvalue.isAlive}");
    //    };
    //}

    public void OnThrust(InputAction.CallbackContext context)
    {
        thrustInput = context.ReadValue<float>();
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        if (airshipControls)
        {
            upDownInput = context.ReadValue<float>();
        }
    }

    public void OnYaw(InputAction.CallbackContext context)
    {
        if (airshipControls)
        {
            yawInput = context.ReadValue<float>();
        }
    }

    public void OnPitch(InputAction.CallbackContext context)
    {
        if (planeControls)
        {
            pitchInput = context.ReadValue<float>();
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (planeControls)
        {
            rollInput = context.ReadValue<float>();
        }
    }

    void FixedUpdate()
    {
        if (requireNetwork)
        {
            if (!IsOwner) return;
        }

        ConvertToDecimalValues();

        thrustForce = transform.forward * thrust * thrustModifier * Time.fixedDeltaTime;
        upDownForce = transform.up * upDown * upDownModifier * Time.fixedDeltaTime;

        Vector3 tempVector = new Vector3(-pitch * pitchModifier, yaw * yawModifier, roll * rollModifier);
        Quaternion rotation = Quaternion.Euler(tempVector * 2.0f * Time.fixedDeltaTime);

        if (!clientAuthorititiveMovement)
        {
            HandleMovementServerAuth(thrustForce, upDownForce, rotation);
        } else
        {
            rb.MovePosition(transform.position + (thrustForce + upDownForce));
            rb.MoveRotation(rb.rotation * rotation);
        }
        
    }

    private void HandleMovementServerAuth(Vector3 _thrust, Vector3 _up, Quaternion _rotation)
    {
        DoMoveServerRpc(_thrust, _up, _rotation);
    }

    [ServerRpc]
    private void DoMoveServerRpc(Vector3 _thrust, Vector3 _up, Quaternion rotation)
    {
        rb.MovePosition(transform.position + (_thrust + _up));

        rb.MoveRotation(rb.rotation * rotation);
    }

    [ServerRpc]
    private void TestServerRpc()
    {
        Debug.Log($"ServerRPc - {OwnerClientId}");
        GameObject spawnedObjectTransform = Instantiate(bulletToSpawn, firePoint.position, Quaternion.identity);
        spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);


    }

    public void RandonInteraction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            TestServerRpc();
        }
    }

    public Material GetTeamMaterial()
    {
        return teamMaterial;
    }

    private void ConvertToDecimalValues()
    {
        thrust = calculatefloatValue(thrustInput, thrust, holdToThrust);
        upDown = calculatefloatValue(upDownInput, upDown);
        yaw = calculatefloatValue(yawInput, yaw);
        pitch = calculatefloatValue(pitchInput, pitch);
        roll = calculatefloatValue(rollInput, roll);
    }

    private float calculatefloatValue(float input, float returned)
    {
        return calculatefloatValue(input, returned, true);
    }

    private float calculatefloatValue(float input, float returned, bool resetBackToZero)
    {
        float v = returned;
        if (input > 0)
        {
            v = incrementFloat(returned, 1);
        }
        else if (input < 0)
        {
            v = decrementFloat(returned, -1);
        }
        else
        {
            if (resetBackToZero)
            {
                if (returned > 0)
                {
                    v = decrementFloat(returned, 0);
                }
                else if (returned < 0)
                {
                    v = incrementFloat(returned, 0);

                }
            }
        }
        return v;
    }

    private float incrementFloat(float v, float target)
    {
        float a = v;
        if (v < target)
        {
            if (v + Time.fixedDeltaTime >= target)
            {
                a = target;
            }
            else
            {
                a += Time.deltaTime;
            }
        }
        return a;
    }

    private float decrementFloat(float v, float target)
    {
        if (v > target)
        {
            if (v - Time.fixedDeltaTime <= target)
            {
                v = target;
            }
            else
            {
                v -= Time.deltaTime;
            }
        }
        return v;
    }
}
