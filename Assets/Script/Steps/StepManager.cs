using UnityEngine;
using System;
using System.Threading.Tasks;

public class StepManager : MonoBehaviour
{
    public static StepManager Instance;

    [Header("Status")]
    public int CurrentSteps { get; private set; }
    public bool IsWalking { get; private set; }

    [Header("Save Settings")]
    public int saveStepInterval = 30;
    public float stopDelay = 1f;

    private int unsavedSteps = 0;
    private float lastStepTime;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject activity;
    private AndroidJavaObject sensorManager;
    private AndroidJavaObject stepSensor;
    private StepListener stepListener;
#endif

#if UNITY_EDITOR
    private float editorStepTimer;
#endif

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

    async void Start()
    {
        await SyncSteps();
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
            ProcessSensorSteps(sensorSteps);
        }
#endif

#if UNITY_EDITOR
        // Unity Editor用テスト
        // スペースキーで1歩追加
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddGameSteps(1);
        }

        // 押しっぱなしテスト：1秒ごとに1歩
        if (Input.GetKey(KeyCode.T))
        {
            editorStepTimer += Time.deltaTime;

            if (editorStepTimer >= 1f)
            {
                editorStepTimer = 0f;
                AddGameSteps(1);
            }
        }
        else
        {
            editorStepTimer = 0f;
        }
#endif

        IsWalking =
            (Time.time - lastStepTime) < stopDelay;
    }

    void ProcessSensorSteps(int sensorSteps)
    {
        if (FirebaseManager.Instance == null)
            return;

        var player = FirebaseManager.Instance.CurrentPlayer;

        if (player == null)
            return;

        string today = DateTime.Now.ToString("yyyy-MM-dd");

        // 初回
        if (player.lastSensorSteps <= 0)
        {
            player.lastSensorSteps = sensorSteps;
            player.todayBaseSensorSteps = sensorSteps;
            player.lastStepDate = today;

            CurrentSteps = player.totalSteps;

            Debug.Log("歩数センサー初期化: " + sensorSteps);
            return;
        }

        // 日付が変わったら今日歩数をリセット
        if (player.lastStepDate != today)
        {
            player.todaySteps = 0;
            player.todayBaseSensorSteps = sensorSteps;
            player.lastStepDate = today;

            Debug.Log("日付変更：今日の歩数をリセット");
        }

        // 端末再起動などでセンサー値が小さくなった場合
        if (sensorSteps < player.lastSensorSteps)
        {
            player.lastSensorSteps = sensorSteps;
            player.todayBaseSensorSteps = sensorSteps;
            player.todaySteps = 0;

            Debug.LogWarning("歩数センサーリセット検知");
            return;
        }

        int diff = sensorSteps - player.lastSensorSteps;

        if (diff <= 0)
            return;

        player.lastSensorSteps = sensorSteps;

        AddGameSteps(diff);

        // 今日の歩数は「今日の基準値」から計算
        player.todaySteps = Mathf.Max(
            0,
            sensorSteps - player.todayBaseSensorSteps
        );
    }

    void AddGameSteps(int amount)
    {
        if (amount <= 0)
            return;

        if (FirebaseManager.Instance == null)
            return;

        var player = FirebaseManager.Instance.CurrentPlayer;

        if (player == null)
            return;

        player.totalSteps += amount;
        player.usableSteps += amount;

        // 旧steps互換
        player.steps = player.totalSteps;

        CurrentSteps = player.totalSteps;

        unsavedSteps += amount;
        lastStepTime = Time.time;

        Debug.Log("歩数追加: " + amount + " / 未保存: " + unsavedSteps);

        if (unsavedSteps >= saveStepInterval)
        {
            _ = SaveStepsNow();
        }
    }

    public async Task SyncSteps()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (stepListener != null)
        {
            int sensorSteps = stepListener.CurrentSteps;
            ProcessSensorSteps(sensorSteps);
        }
#endif

        await SaveStepsNow();
    }

    public async Task SaveStepsNow()
    {
        if (FirebaseManager.Instance == null)
            return;

        if (FirebaseManager.Instance.CurrentPlayer == null)
            return;

        await FirebaseManager.Instance.SavePlayer();

        unsavedSteps = 0;

        Debug.Log("歩数保存完了");
    }

    async void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Debug.Log("バックグラウンドへ移行：歩数保存");
            await SaveStepsNow();
        }
        else
        {
            Debug.Log("アプリ復帰：歩数同期");
            await SyncSteps();
        }
    }

    async void OnApplicationQuit()
    {
        Debug.Log("アプリ終了：歩数保存");
        await SaveStepsNow();
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