using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipControllerV3 : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float thrustModifier, upDownModifier, yawModifier, pitchModifier, rollModifier = 1;

    private float thrustInput, thrust = 0;
    private Vector3 thrustForce = Vector3.zero;

    private float upDownInput, upDown = 0;
    private Vector3 upDownForce = Vector3.zero;

    private float yawInput, yaw = 0;
    private float pitchInput, pitch = 0;
    private float rollInput, roll = 0;

    private Quaternion shipRotation = Quaternion.identity;

    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletToSpawn;

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
        upDownInput = context.ReadValue<float>();
    }

    public void OnYaw(InputAction.CallbackContext context)
    {
        yawInput = context.ReadValue<float>();
    }

    public void OnPitch(InputAction.CallbackContext context)
    {
        pitchInput = context.ReadValue<float>();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        rollInput = context.ReadValue<float>();
    }

    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        else
        {

            ConvertToDecimalValues();

            thrustForce = transform.forward * thrust * thrustModifier * Time.fixedDeltaTime;
            upDownForce = transform.up * upDown * upDownModifier * Time.fixedDeltaTime;

            Vector3 tempVector = new Vector3(-pitch * pitchModifier, yaw * yawModifier, roll * rollModifier);
            Quaternion rotation = Quaternion.Euler(tempVector * 2.0f * Time.fixedDeltaTime);

            HandleMovementServerAuth(thrustForce, upDownForce, rotation);
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

    private void ConvertToDecimalValues()
    {
        thrust = calculatefloatValue(thrustInput, thrust);
        upDown = calculatefloatValue(upDownInput, upDown);
        yaw = calculatefloatValue(yawInput, yaw);
        pitch = calculatefloatValue(pitchInput, pitch);
        roll = calculatefloatValue(rollInput, roll);
    }

    private float calculatefloatValue(float input, float returned)
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
            if (returned > 0)
            {
                v = decrementFloat(returned, 0);
            }
            else if (returned < 0)
            {
                v = incrementFloat(returned, 0);

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
