using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipMovementComponent : NetworkBehaviour
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

    List<StatePayload> clientStatePayloadList;
    List<StatePayload> serverStatePayloadList;

    CircularBuffer<InputPayload> inputPayloadBuffer;
    StatePayload lastSuccessfulState;
    [SerializeField] private float reconciliationThreshold = 1.0f;

    [Header("Movement Modifiers")]
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

    [Header("GameObject References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ShipMovementComponent serverPairShip;

    private void Awake()
    {
        clientStatePayloadBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        inputPayloadBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        clientStatePayloadList = new List<StatePayload>();
        serverStatePayloadList = new List<StatePayload>();

        networkTimer = new NetworkTimer(k_serverTickRate);
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
            HandleServerTickServerRpc(inputPayload);
        }

        handleServerReconciliation();
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

                float serverPositionError = Vector3.Distance(serverState.position, lastSuccessfulState.position);

                if (positionError > reconciliationThreshold || serverPositionError > reconciliationThreshold)
                {
                    // Reconciliation needed
                    transform.position = lastSuccessfulState.position;
                    transform.rotation = lastSuccessfulState.rotation;
                    rb.velocity = lastSuccessfulState.velocity;
                    clientStatePayloadList.Clear();
                }
                else
                {
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
        addToList(serverStatePayloadList, statePayload);
    }

    private void addToList(List<StatePayload> list, StatePayload state)
    {
        // 2 in-game seconds worth of positions
        if (list.Count > 2 / (1 / Time.fixedDeltaTime))
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

    private void ConvertToDecimalValues()
    {
        thrust = shipHelpers.calculatefloatValue(thrustInput, thrust, holdToThrust);
        upDown = shipHelpers.calculatefloatValue(upDownInput, upDown);
        yaw = shipHelpers.calculatefloatValue(yawInput, yaw);
        pitch = shipHelpers.calculatefloatValue(pitchInput, pitch);
        roll = shipHelpers.calculatefloatValue(rollInput, roll);
    }
}
