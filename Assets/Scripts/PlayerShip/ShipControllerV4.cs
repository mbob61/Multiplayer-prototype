//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using Utilities;

//public class ShipControllerV4 : NetworkBehaviour
//{

//    // Network variables should be value objects
//    public struct InputPayload : INetworkSerializable
//    {
//        public int tick;
//        public DateTime timestamp;
//        public ulong networkObjectId;
//        public Vector3 inputVector;
//        public Vector3 position;

//        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//        {
//            serializer.SerializeValue(ref tick);
//            serializer.SerializeValue(ref timestamp);
//            serializer.SerializeValue(ref networkObjectId);
//            serializer.SerializeValue(ref inputVector);
//            serializer.SerializeValue(ref position);
//        }
//    }

//    public struct StatePayload : INetworkSerializable
//    {
//        public int tick;
//        public ulong networkObjectId;
//        public Vector3 position;
//        public Quaternion rotation;
//        public Vector3 velocity;
//        public Vector3 angularVelocity;

//        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//        {
//            serializer.SerializeValue(ref tick);
//            serializer.SerializeValue(ref networkObjectId);
//            serializer.SerializeValue(ref position);
//            serializer.SerializeValue(ref rotation);
//            serializer.SerializeValue(ref velocity);
//            serializer.SerializeValue(ref angularVelocity);
//        }
//    }





//    [SerializeField] private Rigidbody rb;
//    [Header("Movement Modifiers")]
//    //[SerializeField] private float thrustModifier, upDownModifier, yawModifier, pitchModifier, rollModifier = 1;
//    [SerializeField] private float thrustModifier = 1;
//    [SerializeField] private float upDownModifier = 1;
//    [SerializeField] private float yawModifier = 1;
//    [SerializeField] private float pitchModifier = 1;
//    [SerializeField] private float rollModifier = 1;

//    [Header("Movement Types")]
//    private float thrustInput, thrust = 0;
//    private Vector3 thrustForce = Vector3.zero;

//    private float upDownInput, upDown = 0;
//    private Vector3 upDownForce = Vector3.zero;

//    private float yawInput, yaw = 0;
//    private float pitchInput, pitch = 0;
//    private float rollInput, roll = 0;

//    private ShipHelpers shipHelpers = new ShipHelpers();
//    private TeamMaterialAssigner teamMaterialAssigner;

//    [Header("GameObject References")]
//    [SerializeField] private Cinemachine.CinemachineVirtualCamera virtualCam;
//    [SerializeField] private AudioListener audioListener;

//    [Header("Planet Conversion")]
//    [SerializeField] private int teamID = 0;

//    [Header("Server reconciliation")]
//    // Netcode general
//    NetworkTimer networkTimer;
//    const float k_serverTickRate = 60f; // 60 FPS
//    const int k_bufferSize = 1024;

//    // Netcode client specific
//    CircularBuffer<StatePayload> clientStateBuffer;
//    CircularBuffer<InputPayload> clientInputBuffer;
//    StatePayload lastServerState;
//    StatePayload lastProcessedState;

//    ClientNetworkTransform clientNetworkTransform;

//    // Netcode server specific
//    CircularBuffer<StatePayload> serverStateBuffer;
//    Queue<InputPayload> serverInputQueue;

//    [Header("Netcode")]
//    [SerializeField] float reconciliationCooldownTime = 1f;
//    [SerializeField] float reconciliationThreshold = 10f;
//    [SerializeField] GameObject serverCube;
//    [SerializeField] GameObject clientCube;
//    [SerializeField] float extrapolationLimit = 0.5f;
//    [SerializeField] float extrapolationMultiplier = 1.2f;
//    CountdownTimer reconciliationTimer;
//    CountdownTimer extrapolationTimer;
//    StatePayload extrapolationState;

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
//        clientNetworkTransform = GetComponent<ClientNetworkTransform>();



//        networkTimer = new NetworkTimer(k_serverTickRate);
//        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
//        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

//        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
//        serverInputQueue = new Queue<InputPayload>();

//        reconciliationTimer = new CountdownTimer(reconciliationCooldownTime);
//        extrapolationTimer = new CountdownTimer(0);

//        reconciliationTimer.OnTimerStart += () =>
//        {
//            extrapolationTimer.Stop();
//        };

//        extrapolationTimer.OnTimerStart += () =>
//        {
//            reconciliationTimer.Stop();
//            SwitchAuthorityMode(AuthorityMode.Server);
//        };
//        extrapolationTimer.OnTimerStop += () =>
//        {
//            extrapolationState = default;
//            SwitchAuthorityMode(AuthorityMode.Client);
//        };
//    }

//    void SwitchAuthorityMode(AuthorityMode mode)
//    {
//        clientNetworkTransform.authorityMode = mode;
//        bool shouldSync = mode == AuthorityMode.Client;
//        clientNetworkTransform.SyncPositionX = shouldSync;
//        clientNetworkTransform.SyncPositionY = shouldSync;
//        clientNetworkTransform.SyncPositionZ = shouldSync;
//    }


//    public override void OnNetworkSpawn()
//    {
//        if (!IsOwner)
//        {
//            //audioListener.enabled = false;
//            //virtualCam.Priority = 0;
//            return;
//        }

//        //virtualCam.Priority = 100;
//        //audioListener.enabled = true;
//    }

//    void Update()
//    {
//        networkTimer.Update(Time.deltaTime);
//        reconciliationTimer.Tick(Time.deltaTime);
//        extrapolationTimer.Tick(Time.deltaTime);
//        Extraplolate();

//        if (Input.GetKeyDown(KeyCode.J))
//        {
//            transform.position += transform.forward * 20f;
//        }
//    }

//    void FixedUpdate()
//    {
//        while (networkTimer.ShouldTick())
//        {
//            HandleClientTick();
//            HandleServerTick();
//        }

//        Extraplolate();
//    }

//    void HandleServerTick()
//    {
//        if (!IsServer) return;

//        var bufferIndex = -1;
//        InputPayload inputPayload = default;
//        while (serverInputQueue.Count > 0)
//        {
//            inputPayload = serverInputQueue.Dequeue();

//            bufferIndex = inputPayload.tick % k_bufferSize;

//            StatePayload statePayload = ProcessMovement(inputPayload);
//            serverStateBuffer.Add(statePayload, bufferIndex);
//        }

//        if (bufferIndex == -1) return;
//        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
//        HandleExtrapolation(serverStateBuffer.Get(bufferIndex), CalculateLatencyInMillis(inputPayload));
//    }

//    static float CalculateLatencyInMillis(InputPayload inputPayload) => (DateTime.Now - inputPayload.timestamp).Milliseconds / 1000f;

//    void Extraplolate()
//    {
//        if (IsServer && extrapolationTimer.IsRunning)
//        {
//            transform.position += extrapolationState.position.With(y: 0);
//        }
//    }

//    void HandleExtrapolation(StatePayload latest, float latency)
//    {
//        if (ShouldExtrapolate(latency))
//        {
//            // Calculate the arc the object would traverse in degrees
//            float axisLength = latency * latest.angularVelocity.magnitude * Mathf.Rad2Deg;
//            Quaternion angularRotation = Quaternion.AngleAxis(axisLength, latest.angularVelocity);

//            if (extrapolationState.position != default)
//            {
//                latest = extrapolationState;
//            }

//            // Update position and rotation based on extrapolation
//            var posAdjustment = latest.velocity * (1 + latency * extrapolationMultiplier);
//            extrapolationState.position = posAdjustment;
//            extrapolationState.rotation = angularRotation * transform.rotation;
//            extrapolationState.velocity = latest.velocity;
//            extrapolationState.angularVelocity = latest.angularVelocity;
//            extrapolationTimer.Start();
//        }
//        else
//        {
//            extrapolationTimer.Stop();
//        }
//    }

//    bool ShouldExtrapolate(float latency) => latency < extrapolationLimit && latency > Time.fixedDeltaTime;

//    [ClientRpc]
//    void SendToClientRpc(StatePayload statePayload)
//    {
//        serverCube.transform.position = statePayload.position.With(y: 4);
//        if (!IsOwner) return;
//        lastServerState = statePayload;
//    }

//    void HandleClientTick()
//    {
//        if (!IsClient || !IsOwner) return;

//        var currentTick = networkTimer.currentTick;
//        var bufferIndex = currentTick % k_bufferSize;

//        InputPayload inputPayload = new InputPayload()
//        {
//            tick = currentTick,
//            timestamp = DateTime.Now,
//            networkObjectId = NetworkObjectId,
//            inputVector = new Vector3(thrustInput, upDownInput, yawInput),
//            position = transform.position
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

//        return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationTimer.IsRunning && !extrapolationTimer.IsRunning;
//    }

//    void HandleServerReconciliation()
//    {
//        if (!ShouldReconcile()) return;

//        float positionError;
//        int bufferIndex;

//        bufferIndex = lastServerState.tick % k_bufferSize;
//        if (bufferIndex - 1 < 0) return; // Not enough information to reconcile

//        StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState; // Host RPCs execute immediately, so we can use the last server state
//        StatePayload clientState = clientStateBuffer.Get(bufferIndex);
//        positionError = Vector3.Distance(rewindState.position, clientState.position);


//        print(positionError);
//        if (positionError > reconciliationThreshold)
//        {
//            print("I need to reconcile");
//            ReconcileState(rewindState);
//            reconciliationTimer.Start();
//        }

//        lastProcessedState = rewindState;
//    }

//    void ReconcileState(StatePayload rewindState)
//    {
//        transform.position = rewindState.position;
//        transform.rotation = rewindState.rotation;
//        rb.velocity = rewindState.velocity;
//        rb.angularVelocity = rewindState.angularVelocity;

//        if (!rewindState.Equals(lastServerState)) return;

//        clientStateBuffer.Add(rewindState, rewindState.tick % k_bufferSize);

//        // Replay all inputs from the rewind state to the current state
//        int tickToReplay = lastServerState.tick;

//        while (tickToReplay < networkTimer.currentTick)
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
//        clientCube.transform.position = input.position.With(y: 4);
//        serverInputQueue.Enqueue(input);
//    }

//    StatePayload ProcessMovement(InputPayload input)
//    {
//        MoveAndRotate(input.inputVector);

//        return new StatePayload()
//        {
//            tick = input.tick,
//            networkObjectId = NetworkObjectId,
//            position = transform.position,
//            rotation = transform.rotation,
//            velocity = rb.velocity,
//            angularVelocity = rb.angularVelocity
//        };
//    }





//    private void MoveAndRotate(Vector3 playerInputs)
//    {
//        thrustForce = transform.forward * playerInputs.x * thrustModifier;
//        upDownForce = transform.up * playerInputs.y * upDownModifier;

//        Vector3 tempVector = new Vector3(-pitch * pitchModifier, playerInputs.z * yawModifier, roll * rollModifier);
//        Quaternion rotation = Quaternion.Euler(tempVector * 2.0f * networkTimer.MinTimeBetweenTicks);

//        //rb.MovePosition((transform.position + (thrustForce + upDownForce)) * Time.fixedDeltaTime );
//        rb.velocity = (thrustForce + upDownForce) * networkTimer.MinTimeBetweenTicks;
//        rb.MoveRotation(rb.rotation * rotation);
//    }

//    public int GetTeamID()
//    {
//        return teamID;
//    }

//    private float ConvertToDecimalValues(float input)
//    {
//        return calculatefloatValue(input);

//    }










//    public float calculatefloatValue(float input)
//    {
//        return calculatefloatValue(input, true);
//    }

//    public float calculatefloatValue(float input, bool resetBackToZero)
//    {
//        float v = input;
//        if (input > 0)
//        {
//            if (v < 0)
//            {
//                v = incrementFloat(v, 1, 3.0f);
//            }
//            else
//            {
//                v = incrementFloat(v, 1);
//            }
//        }
//        else if (input < 0)
//        {
//            if (v > 0)
//            {
//                v = decrementFloat(v, -1, 3.0f);
//            }
//            else
//            {
//                v = decrementFloat(v, -1);
//            }
//        }
//        else
//        {
//            if (resetBackToZero)
//            {
//                if (v > 0)
//                {
//                    v = decrementFloat(v, 0);
//                }
//                else if (v < 0)
//                {
//                    v = incrementFloat(v, 0);

//                }
//            }
//        }
//        return v;
//    }

//    private float incrementFloat(float v, float target)
//    {
//        return incrementFloat(v, target, 1.0f);
//    }

//    private float decrementFloat(float v, float target)
//    {
//        return decrementFloat(v, target, 1.0f);
//    }

//    private float incrementFloat(float v, float target, float incrementModifier)
//    {
//        float a = v;
//        if (v < target)
//        {
//            if (v + (Time.fixedDeltaTime * incrementModifier) >= target)
//            {
//                a = target;
//            }
//            else
//            {
//                a += Time.deltaTime * incrementModifier;
//            }
//        }
//        return a;
//    }

//    private float decrementFloat(float v, float target, float decrementModifier)
//    {
//        if (v > target)
//        {
//            if (v - (Time.fixedDeltaTime * decrementModifier) <= target)
//            {
//                v = target;
//            }
//            else
//            {
//                v -= Time.deltaTime * decrementModifier;
//            }
//        }
//        return v;
//    }


//    public abstract class Timer
//    {
//        protected float initialTime;
//        protected float Time { get; set; }
//        public bool IsRunning { get; protected set; }

//        public float Progress => Time / initialTime;

//        public Action OnTimerStart = delegate { };
//        public Action OnTimerStop = delegate { };

//        protected Timer(float value)
//        {
//            initialTime = value;
//            IsRunning = false;
//        }

//        public void Start()
//        {
//            Time = initialTime;
//            if (!IsRunning)
//            {
//                IsRunning = true;
//                OnTimerStart.Invoke();
//            }
//        }

//        public void Stop()
//        {
//            if (IsRunning)
//            {
//                IsRunning = false;
//                OnTimerStop.Invoke();
//            }
//        }

//        public void Resume() => IsRunning = true;
//        public void Pause() => IsRunning = false;

//        public abstract void Tick(float deltaTime);
//    }

//    public class CountdownTimer : Timer
//    {
//        public CountdownTimer(float value) : base(value) { }

//        public override void Tick(float deltaTime)
//        {
//            if (IsRunning && Time > 0)
//            {
//                Time -= deltaTime;
//            }

//            if (IsRunning && Time <= 0)
//            {
//                Stop();
//            }
//        }

//        public bool IsFinished => Time <= 0;

//        public void Reset() => Time = initialTime;

//        public void Reset(float newTime)
//        {
//            initialTime = newTime;
//            Reset();
//        }
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