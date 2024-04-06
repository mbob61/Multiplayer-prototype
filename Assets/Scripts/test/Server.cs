//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Cinemachine;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.InputSystem;

//// Network variables should be value objects
//public struct InputPayload : INetworkSerializable
//{
//    public int tick;
//    public Vector3 inputVector;

//    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//    {
//        serializer.SerializeValue(ref tick);
//        serializer.SerializeValue(ref inputVector);
//    }
//}

//public struct StatePayload : INetworkSerializable
//{
//    public int tick;
//    public Vector3 position;
//    public Quaternion rotation;
//    public Vector3 velocity;
//    public Vector3 angularVelocity;

//    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//    {
//        serializer.SerializeValue(ref tick);
//        serializer.SerializeValue(ref position);
//        serializer.SerializeValue(ref rotation);
//        serializer.SerializeValue(ref velocity);
//        serializer.SerializeValue(ref angularVelocity);
//    }
//}

//public class Server : NetworkBehaviour
//{
//    [SerializeField] Rigidbody rb;

//    // Netcode general
//    NetworkTimer timer;
//    const float k_serverTickRate = 60f; // 60 FPS
//    const int k_bufferSize = 1024;

//    // Netcode client specific
//    CircularBuffer<StatePayload> clientStateBuffer;
//    CircularBuffer<InputPayload> clientInputBuffer;
//    StatePayload lastServerState;
//    StatePayload lastProcessedState;

//    // Netcode server specific
//    CircularBuffer<StatePayload> serverStateBuffer;
//    Queue<InputPayload> serverInputQueue;

//    [Header("Netcode")]
//    [SerializeField] float reconciliationCooldown = 0.2f;
//    [SerializeField] float reconciliationThreshold = 10f;
//    [SerializeField] GameObject serverCube;
//    [SerializeField] GameObject clientCube;


//    [SerializeField] private float thrustModifier = 1;
//    [SerializeField] private float upDownModifier = 1;
//    [SerializeField] private float yawModifier = 1;
//    [SerializeField] private float pitchModifier = 1;
//    [SerializeField] private float rollModifier = 1;

//    [Header("Movement Types")]
//    [SerializeField] private bool requireNetwork = true;
//    [SerializeField] private bool clientAuthorititiveMovement = false;
//    [SerializeField] private bool planeControls = false;
//    [SerializeField] private bool airshipControls = true;
//    [SerializeField] private bool holdToThrust = true;

//    private float thrustInput, thrust = 0;
//    private Vector3 thrustForce = Vector3.zero;

//    private float upDownInput, upDown = 0;
//    private Vector3 upDownForce = Vector3.zero;

//    private float yawInput, yaw = 0;
//    private float pitchInput, pitch = 0;
//    private float rollInput, roll = 0;

//    private CountdownTimer reconciliationTimer;

//    public void OnThrust(InputAction.CallbackContext context)
//    {
//        thrustInput = context.ReadValue<float>();
//    }

//    public void OnUpDown(InputAction.CallbackContext context)
//    {
//        upDownInput = context.ReadValue<float>();
//    }

//    public void OnYaw(InputAction.CallbackContext context)
//    {
//        yawInput = context.ReadValue<float>();
//    }

//    public void OnPitch(InputAction.CallbackContext context)
//    {
//        pitchInput = context.ReadValue<float>();
//    }

//    public void OnRoll(InputAction.CallbackContext context)
//    {
//        rollInput = context.ReadValue<float>();
//    }

//    void Awake()
//    {
       
//        rb = GetComponent<Rigidbody>();
       

//        timer = new NetworkTimer(k_serverTickRate);
//        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
//        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

//        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
//        serverInputQueue = new Queue<InputPayload>();
//    }

  

//    public override void OnNetworkSpawn()
//    {
//        if (!IsOwner)
//        {
//            //playerAudioListener.enabled = false;
//            //playerCamera.Priority = 0;
//            return;
//        }

//        //playerCamera.Priority = 100;
//        //playerAudioListener.enabled = true;
//    }

//    void Update()
//    {
//        timer.Update(Time.deltaTime);
//        if (Input.GetKeyDown(KeyCode.J))
//        {
//            transform.position += transform.forward * 20f;
//        }
//    }

//    void FixedUpdate()
//    {
//        if (!IsOwner) return;

//        while (timer.ShouldTick())
//        {
//            HandleClientTick();
//            HandleServerTick();
//        }
//    }

//    void HandleServerTick()
//    {
//        var bufferIndex = -1;
//        while (serverInputQueue.Count > 0)
//        {
//            InputPayload inputPayload = serverInputQueue.Dequeue();

//            bufferIndex = inputPayload.tick % k_bufferSize;

//            var previousBufferIndex = bufferIndex - 1;
//            if (previousBufferIndex < 0) previousBufferIndex = k_bufferSize - 1;

//            StatePayload statePayload = SimulateMovement(inputPayload);
//            serverStateBuffer.Add(statePayload, bufferIndex);
//        }

//        if (bufferIndex == -1) return;
//        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
//    }

//    StatePayload SimulateMovement(InputPayload inputPayload)
//    {
//        Physics.simulationMode = SimulationMode.Script;

//        Move(inputPayload.inputVector);
//        Physics.Simulate(Time.fixedDeltaTime);
//        Physics.simulationMode = SimulationMode.FixedUpdate;

//        return new StatePayload()
//        {
//            tick = inputPayload.tick,
//            position = transform.position,
//            rotation = transform.rotation,
//            velocity = rb.velocity,
//            angularVelocity = rb.angularVelocity
//        };
//    }

//    [ClientRpc]
//    void SendToClientRpc(StatePayload statePayload)
//    {
//        if (!IsOwner) return;
//        lastServerState = statePayload;
//    }

//    void HandleClientTick()
//    {
//        if (!IsClient) return;

//        var currentTick = timer.CurrentTick;
//        var bufferIndex = currentTick % k_bufferSize;

//        InputPayload inputPayload = new InputPayload()
//        {
//            tick = currentTick,
//            inputVector = new Vector3(thrustInput, upDownInput, yawInput)
//        };

//        clientInputBuffer.Add(inputPayload, bufferIndex);
//        SendToServerRpc(inputPayload);

//        StatePayload statePayload = ProcessMovement(inputPayload);
//        clientStateBuffer.Add(statePayload, bufferIndex);

//        HandleServerReconciliation();
//    }

//    bool ShouldReconcile()
//    {
//        bool isNewServerState = !lastServerState.Equals(default);
//        bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default)
//                                               || !lastProcessedState.Equals(lastServerState);

//        return isNewServerState && isLastStateUndefinedOrDifferent;
//    }

//    void HandleServerReconciliation()
//    {
//        if (!ShouldReconcile()) return;

//        float positionError;
//        int bufferIndex;
//        StatePayload rewindState = default;

//        bufferIndex = lastServerState.tick % k_bufferSize;
//        if (bufferIndex - 1 < 0) return; // Not enough information to reconcile

//        rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState; // Host RPCs execute immediately, so we can use the last server state
//        positionError = Vector3.Distance(rewindState.position, clientStateBuffer.Get(bufferIndex).position);

//        if (positionError > reconciliationThreshold)
//        {
//            ReconcileState(rewindState);
//        }

//        lastProcessedState = lastServerState;
//    }

//    void ReconcileState(StatePayload rewindState)
//    {
//        transform.position = rewindState.position;
//        transform.rotation = rewindState.rotation;
//        rb.velocity = rewindState.velocity;
//        rb.angularVelocity = rewindState.angularVelocity;

//        if (!rewindState.Equals(lastServerState)) return;

//        //clientStateBuffer.Add(rewindState, rewindState.tick);
//        clientStateBuffer.Add(rewindState, rewindState.tick % k_bufferSize);

//        // Replay all inputs from the rewind state to the current state
//        int tickToReplay = lastServerState.tick;

//        while (tickToReplay < timer.CurrentTick)
//        {
//            int bufferIndex = tickToReplay % k_bufferSize;
//            StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
//            clientStateBuffer.Add(statePayload, bufferIndex);
//            tickToReplay++;
//        }
//    }

//    [ServerRpc]
//    void SendToServerRpc(InputPayload input)
//    {
//        serverInputQueue.Enqueue(input);
//    }

//    StatePayload ProcessMovement(InputPayload input)
//    {
//        Move(input.inputVector);

//        return new StatePayload()
//        {
//            tick = input.tick,
//            position = transform.position,
//            rotation = transform.rotation,
//            velocity = rb.velocity,
//            angularVelocity = rb.angularVelocity
//        };
//    }
//    void Move(Vector3 inputVector)
//    {
//        thrustForce = transform.forward * inputVector.x * thrustModifier * timer.MinTimeBetweenTicks;
//        upDownForce = transform.up * inputVector.y * upDownModifier * timer.MinTimeBetweenTicks;

//        Vector3 tempVector = new Vector3(-pitch * pitchModifier, inputVector.z * yawModifier, roll * rollModifier);
//        Quaternion rotation = Quaternion.Euler(tempVector * 2.0f * timer.MinTimeBetweenTicks);

//        rb.velocity = thrustForce + upDownForce;
//        //rb.MovePosition(transform.position + (thrustForce + upDownForce));
//        rb.MoveRotation(rb.rotation * rotation);

//    }
//}


//public abstract class Timer
//{
//    protected float initialTime;
//    protected float Time { get; set; }
//    public bool IsRunning { get; protected set; }

//    public float Progress => Time / initialTime;

//    public Action OnTimerStart = delegate { };
//    public Action OnTimerStop = delegate { };

//    protected Timer(float value)
//    {
//        initialTime = value;
//        IsRunning = false;
//    }

//    public void Start()
//    {
//        Time = initialTime;
//        if (!IsRunning)
//        {
//            IsRunning = true;
//            OnTimerStart.Invoke();
//        }
//    }

//    public void Stop()
//    {
//        if (IsRunning)
//        {
//            IsRunning = false;
//            OnTimerStop.Invoke();
//        }
//    }

//    public void Resume() => IsRunning = true;
//    public void Pause() => IsRunning = false;

//    public abstract void Tick(float deltaTime);
//}

//public class CountdownTimer : Timer
//{
//    public CountdownTimer(float value) : base(value) { }

//    public override void Tick(float deltaTime)
//    {
//        if (IsRunning && Time > 0)
//        {
//            Time -= deltaTime;
//        }

//        if (IsRunning && Time <= 0)
//        {
//            Stop();
//        }
//    }

//    public bool IsFinished => Time <= 0;

//    public void Reset() => Time = initialTime;

//    public void Reset(float newTime)
//    {
//        initialTime = newTime;
//        Reset();
//    }
//}

//namespace Utilities
//{
//    public static class Vector3Extensions
//    {
//        /// <summary>
//        /// Sets any values of the Vector3
//        /// </summary>
//        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
//        {
//            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
//        }

//        /// <summary>
//        /// Adds to any values of the Vector3
//        /// </summary>
//        public static Vector3 Add(this Vector3 vector, float? x = null, float? y = null, float? z = null)
//        {
//            return new Vector3(vector.x + (x ?? 0), vector.y + (y ?? 0), vector.z + (z ?? 0));
//        }
//    }
//}