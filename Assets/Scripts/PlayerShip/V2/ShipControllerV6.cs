using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipControllerV6 : NetworkBehaviour
{
    // Network variables should be value objects
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public ulong networkObjectId;
        public Vector3 inputVector;
        public Vector3 position;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref inputVector);
            serializer.SerializeValue(ref position);
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public ulong networkObjectId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
        }
    }

    [Header("Server reconciliation")]
    // Netcode general
    NetworkTimer networkTimer;
    const float k_serverTickRate = 60f; // 60 FPS
    const int k_bufferSize = 1024;

    // Netcode client specific
    CircularBuffer<StatePayload> clientStatePayloadBuffer;
    CircularBuffer<StatePayload> serverStatePayloadBuffer;

    List<StatePayload> clientStatePayloadList;
    List<StatePayload> serverStatePayloadList;

    CircularBuffer<InputPayload> inputPayloadBuffer;
    StatePayload lastSuccessfulState, lastServerState;
    [SerializeField] private float reconciliationThreshold = 1.0f;




    [SerializeField] private Rigidbody rb;
    [Header("Movement Modifiers")]
    //[SerializeField] private float thrustModifier, upDownModifier, yawModifier, pitchModifier, rollModifier = 1;
    [SerializeField] private float thrustModifier = 1;
    [SerializeField] private float upDownModifier = 1;
    [SerializeField] private float yawModifier = 1;
    [SerializeField] private float pitchModifier = 1;
    [SerializeField] private float rollModifier = 1;

    [Header("Movement Types")]
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
    //[SerializeField] private Transform firePoint;
    //[SerializeField] private GameObject networkedBullet;
    //[SerializeField] private GameObject nonNetworkedBullet;
    //[SerializeField] private GameObject bodyGraphic;
    //[SerializeField] private GameObject turret;
    [SerializeField] private ShipControllerV6 serverPairShip;

    [Header("Planet Conversion")]
    [SerializeField] private int teamID = 0;

    private void Awake()
    {
        teamMaterialAssigner = FindObjectOfType<TeamMaterialAssigner>();
        //bodyGraphic.GetComponent<Renderer>().materials[0].color = teamMaterialAssigner.GetMaterialForTeamWithID(teamID).color;

        clientStatePayloadBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverStatePayloadBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        inputPayloadBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        clientStatePayloadList = new List<StatePayload>();
        serverStatePayloadList = new List<StatePayload>();

        networkTimer = new NetworkTimer(k_serverTickRate);
        //statePayloadBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        //inputPayloadBuffer = new CircularBuffer<InputPayload>(k_bufferSize);
    }

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

    private void Update()
    {
        if (!IsOwner) return;
        if (clientAuthorititiveMovement)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                transform.position += transform.forward * 20f;
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        networkTimer.Update(Time.fixedDeltaTime);


        ConvertToDecimalValues();

        var currentTick = networkTimer.CurrentTick;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            networkObjectId = NetworkObjectId,
            inputVector = new Vector3(thrustInput, upDownInput, yawInput),
            position = transform.position
        };

        inputPayloadBuffer.Add(inputPayload, bufferIndex);


        if (clientAuthorititiveMovement)
        {
            HandleClientTick(inputPayload);
        }
        else
        {
            print("do i get in here?");
            HandleServerTickServerRpc(inputPayload);
        }

        handleServerReconciliation();

        //if (clientAuthorititiveMovement)
        //{

        //    StatePayload clientStatePayload = clientStatePayloadBuffer.Get(bufferIndex);
        //    StatePayload serverStatePayload = serverPairShip.GetServerStatePayloadBuffer().Get(bufferIndex);

        //    float distanceBetweenClientAndServer = Vector3.Distance(clientStatePayload.position, serverStatePayload.position);
        //    float distanceBetweenNowAndSuccessfulState = Vector3.Distance(clientStatePayload.position, lastSuccessfulState.position);

        //    //if (distanceBetweenClientAndServer > reconciliationThreshold || distanceBetweenNowAndSuccessfulState > reconciliationThreshold)
        //    if (distanceBetweenClientAndServer > reconciliationThreshold)
        //        {
        //        // Reconciliation needed
        //        print("Positions are not within the accectpable threshold. We need to reconcile: " + distanceBetweenClientAndServer);
        //        transform.position = serverStatePayload.position;
        //        transform.rotation = serverStatePayload.rotation;
        //        rb.velocity = serverStatePayload.velocity;
        //        lastSuccessfulState = clientStatePayloadBuffer.Get(bufferIndex);
        //    }
        //    else
        //    {
        //        print("Positions are within threshold, no need to reconcile: " + distanceBetweenClientAndServer);
        //        lastSuccessfulState = clientStatePayloadBuffer.Get(bufferIndex);
        //    }
        //}
    }

    private void handleServerReconciliation()
    {
        if (clientAuthorititiveMovement)
        {
            if (serverPairShip.GetServerStatePayloadList().Count > 0 && clientStatePayloadList.Count > 0)
            {
                StatePayload clientState = clientStatePayloadList[clientStatePayloadList.Count - 1];
                StatePayload serverState = serverPairShip.GetServerStatePayloadList()[serverPairShip.GetServerStatePayloadList().Count - 1];

                float positionError = Vector3.Distance(clientState.position, serverState.position);

                if (positionError > reconciliationThreshold)
                {
                    // Reconciliation needed
                    print("Positions are not within the accectpable threshold. We need to reconcile: " + positionError);
                    transform.position = serverState.position;
                    transform.rotation = serverState.rotation;
                    rb.velocity = serverState.velocity;
                    clientStatePayloadList.Clear();
                    lastSuccessfulState = serverState;
                }
                else
                {
                    //print("Positions are within threshold, no need to reconcile: " + positionError);
                    lastSuccessfulState = serverState;
                }
            }
        }
    }

    [ServerRpc]
    private void HandleServerTickServerRpc(InputPayload inputPayload)
    {
        StatePayload state = MoveWithForce(inputPayload);
        SendToClientRpc(state);
    }

    [ClientRpc]
    void SendToClientRpc(StatePayload statePayload)
    {
        print("do i ever make it here into the client rpc??");
        addToList(serverStatePayloadList, statePayload);
    }

    private void addToList(List<StatePayload> list, StatePayload state)
    {
        if (list.Count > 200)
        {
            list.Clear();
        }
        list.Add(state);
    }

    private void HandleClientTick(InputPayload inputPayload)
    {
        StatePayload state = MoveWithForce(inputPayload);
        addToList(clientStatePayloadList, state);
    }

    private StatePayload MoveWithForce(InputPayload inputPayload)
    {
        thrustForce = transform.forward * inputPayload.inputVector.x * thrustModifier * Time.fixedDeltaTime;
        upDownForce = transform.up * inputPayload.inputVector.y * upDownModifier * Time.fixedDeltaTime;

        Vector3 tempVector = new Vector3(-pitch * pitchModifier, inputPayload.inputVector.z * yawModifier, roll * rollModifier);
        Quaternion rotationForce = Quaternion.Euler(tempVector * 2.0f * Time.fixedDeltaTime);

        rb.MovePosition(transform.position + (thrustForce + upDownForce));
        rb.MoveRotation(rb.rotation * rotationForce);

        StatePayload statePayload = new StatePayload()
        {
            tick = inputPayload.tick,
            position = transform.position,
            rotation = transform.rotation,
            velocity = rb.velocity,
        };

        return statePayload;
    }

    private void addStateToBuffer(StatePayload inputPayload)
    {
        StatePayload statePayload = new StatePayload()
        {
            tick = inputPayload.tick,
            position = transform.position,
            rotation = transform.rotation,
            velocity = rb.velocity,
        };

        clientStatePayloadBuffer.Add(statePayload, inputPayload.tick);
    }

    //public void FireBullet(InputAction.CallbackContext context)
    //{
    //    if (requireNetwork)
    //    {
    //        if (!IsOwner) return;
    //    }

    //    if (context.performed)
    //    {
    //        if (!clientAuthorititiveMovement)
    //        {
    //            CreateBulletServerRpc();
    //        }
    //        else
    //        {
    //            CreateBullet();
    //        }
    //    }
    //}

    //[ServerRpc]
    //private void CreateBulletServerRpc()
    //{
    //    Debug.Log($"ServerRPc - {OwnerClientId}");
    //    GameObject spawnedBullet = spawnBulletAndApplyForce(networkedBullet);
    //    spawnedBullet.GetComponent<NetworkObject>().Spawn(true);
    //}

    //private void CreateBullet()
    //{
    //    spawnBulletAndApplyForce(nonNetworkedBullet);
    //}

    //private GameObject spawnBulletAndApplyForce(GameObject bullet)
    //{
    //    GameObject spawnedBullet = Instantiate(bullet, firePoint.position, turret.transform.rotation);
    //    spawnedBullet.GetComponent<Rigidbody>().AddForce(spawnedBullet.transform.forward * 2.0f, ForceMode.Impulse);
    //    return spawnedBullet;
    //}

    public CircularBuffer<StatePayload> GetClientStatePayloadBuffer()
    {
        return clientStatePayloadBuffer;
    }

    public CircularBuffer<InputPayload> GetInputPayloadBuffer()
    {
        return inputPayloadBuffer;
    }

    public List<StatePayload> GetServerStatePayloadList()
    {
        return serverStatePayloadList;
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
