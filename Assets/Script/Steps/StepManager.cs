using UnityEngine;

public class StepManager : MonoBehaviour
{
    public static StepManager Instance;

    public int CurrentSteps { get; private set; }
    public bool IsWalking { get; private set; }

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject activity;
    private AndroidJavaObject sensorManager;
    private AndroidJavaObject stepSensor;
    private StepListener stepListener;
#endif
#pragma warning disable CS0414
    private int lastSensorValue = -1;
#pragma warning restore CS0414
    private float lastStepTime;
    public float stopDelay = 1f;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID && !UNITY_EDITOR
        InitializeStepCounter();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    void InitializeStepCounter()
    {
        AndroidJavaClass unityPlayer =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        activity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        sensorManager =
            activity.Call<AndroidJavaObject>("getSystemService", "sensor");

        // TYPE_STEP_COUNTER = 19
        stepSensor =
            sensorManager.Call<AndroidJavaObject>(
                "getDefaultSensor",
                19
            );

        if (stepSensor == null)
        {
            Debug.LogWarning("歩数センサーなし");
            return;
        }

        stepListener = new StepListener();

        sensorManager.Call<bool>(
            "registerListener",
            stepListener,
            stepSensor,
            0
        );

        Debug.Log("歩数センサー開始");
    }
#endif

    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (stepListener != null)
        {
            int sensorSteps = stepListener.CurrentSteps;

            if (lastSensorValue < 0)
            {
                lastSensorValue = sensorSteps;
            }

            int diff = sensorSteps - lastSensorValue;

            if (diff > 0)
            {
                CurrentSteps += diff;

                lastSensorValue = sensorSteps;
                lastStepTime = Time.time;
            }

            IsWalking =
                (Time.time - lastStepTime) < stopDelay;
        }

#endif
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (sensorManager != null && stepListener != null)
        {
            sensorManager.Call(
                "unregisterListener",
                stepListener
            );
        }

#endif
    }
}