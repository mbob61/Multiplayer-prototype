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

    private ShipHelpers shipHelpers = new ShipHelpers();
    private TeamMaterialAssigner teamMaterialAssigner;

    [Header("GameObject References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject networkedBullet;
    [SerializeField] private GameObject nonNetworkedBullet;
    [SerializeField] private GameObject bodyGraphic;

    [Header("Planet Conversion")]
    [SerializeField] private int teamID = 0;

    private void Start()
    {
        teamMaterialAssigner = FindObjectOfType<TeamMaterialAssigner>();
        bodyGraphic.GetComponent<Renderer>().materials[0].color = teamMaterialAssigner.GetMaterialForTeamWithID(teamID).color;
    }


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
            MoveAndRotateServerRpc(thrustForce, upDownForce, rotation);
        } else
        {
            MoveAndRotate(thrustForce, upDownForce, rotation);

        }
        
    }


    [ServerRpc]
    private void MoveAndRotateServerRpc(Vector3 _thrust, Vector3 _up, Quaternion rotation)
    {
        MoveAndRotate(_thrust, _up, rotation);
    }

    private void MoveAndRotate(Vector3 _thrust, Vector3 _up, Quaternion rotation)
    {
        rb.MovePosition(transform.position + (_thrust + _up));
        rb.MoveRotation(rb.rotation * rotation);
    }

    public void FireBullet(InputAction.CallbackContext context)
    {
        if (requireNetwork)
        {
            if (!IsOwner) return;
        }

        if (context.performed)
        {
            if (!clientAuthorititiveMovement)
            {
                CreateBulletServerRpc();
            }
            else
            {
                CreateBullet();
            }
        }
    }

    [ServerRpc]
    private void CreateBulletServerRpc()
    {
        Debug.Log($"ServerRPc - {OwnerClientId}");

        GameObject spawnedObjectTransform = Instantiate(networkedBullet, firePoint.position, Quaternion.identity);
        spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private void CreateBullet()
    {
        GameObject spawnedObjectTransform = Instantiate(nonNetworkedBullet, firePoint.position, Quaternion.identity);
    }

    public int GetTeamID()
    {
        return teamID;
    }

    private void ConvertToDecimalValues()
    {
        thrust = shipHelpers.calculatefloatValue(thrustInput, thrust, holdToThrust);
        upDown = shipHelpers.calculatefloatValue(upDownInput, upDown);
        yaw = shipHelpers.calculatefloatValue(yawInput, yaw);
        pitch = shipHelpers.calculatefloatValue(pitchInput, pitch);
        roll = shipHelpers.calculatefloatValue(rollInput, roll);
    }
}
